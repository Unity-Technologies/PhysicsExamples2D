using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Geometry-island handling after fragmentation. Six tall destructible columns; press Space repeatedly to fire
/// projectiles that fragment them. The unbroken remainder of each fragmented body is split into geometry islands
/// — each island that intersects a virtual ground line stays static; the rest become dynamic and fall. The broken
/// pieces become a debris-body batch.
///
/// This example focuses on the `unbrokenGeometryIslands` accessor and the per-island static-vs-dynamic decision —
/// the same core API reuses Fragment results to build multiple bodies from one source body.
/// </summary>
public class GeometryIslands : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    public enum FragmentColors { Off, White, Group, Individual }

    public float FragmentRadius = 1f;
    public int FragmentCount = 25;
    public FragmentColors Colors = FragmentColors.Individual;
    public bool FragmentExplode = false;
    public uint RandomSeed = 1234;

    private bool m_OldAutoContactCallbacks;
    private PhysicsWorld.DrawFillOptions m_OldDrawFillOptions;

    private readonly PhysicsMask m_GroundMask = new(2);
    private readonly PhysicsMask m_DestructibleMask = new(3);
    private readonly PhysicsMask m_DebrisMask = new(4);
    private readonly PhysicsMask m_ProjectileMask = new(5);

    private Color m_DestructibleColor;
    private PhysicsShape.ContactFilter m_DestructibleContactFilter;
    private PhysicsShape.SurfaceMaterial m_DestructibleSurfaceMaterial;

    private const float ProjectileRadius = 0.2f;
    private const float PlayerSpeed = 60f;
    private const float ProjectileSpeed = 40f;
    private const float ProjectileDelay = 0.05f;
    private float m_ProjectileTime;
    private Vector2 m_PlayerPosition;
    private CapsuleGeometry m_PlayerGeometry;
    private CircleGeometry m_FireGeometry;
    private PolygonGeometry m_DestructGeometry;
    private NativeArray<PolygonGeometry> m_FragmentGeometryMask;

    private SegmentGeometry m_VirtualGroundGeometry;
    private PhysicsTransform m_VirtualGroundTransform;

    private Random m_Random;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;

        m_OldAutoContactCallbacks = world.autoContactCallbacks;
        world.autoContactCallbacks = true;
        m_OldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;

        m_Random = new Random(RandomSeed);

        m_DestructibleColor = Color.seaGreen;
        m_DestructibleContactFilter = new PhysicsShape.ContactFilter { categories = m_DestructibleMask, contacts = m_GroundMask | m_ProjectileMask | m_DebrisMask | m_DestructibleMask };
        m_DestructibleSurfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f, bounciness = 0.4f, tangentSpeed = 0.1f, customColor = m_DestructibleColor };

        m_PlayerGeometry = new CapsuleGeometry { center1 = Vector2.zero, center2 = Vector2.up, radius = 0.5f };
        // Tall column geometry — ~3x40 quad with a slight asymmetry so the islands form interestingly.
        m_DestructGeometry = PolygonGeometry.Create(new Vector2[] { new(-0.5f, -0.5f), new(0.5f, -0.5f), new(10f, 0.5f), new(9f, 0.5f) }.AsSpan()).Transform(Matrix4x4.Scale(new Vector2(3f, 40f)), false);
        m_PlayerPosition = new Vector2(0f, 24f);

        m_VirtualGroundGeometry = new SegmentGeometry { point1 = new Vector2(-30f, 0f), point2 = new Vector2(25f, 0f) };
        m_VirtualGroundTransform = new PhysicsTransform(Vector2.down * 21.9f);

        UpdateFragmentGeometry();
        SetupScene();
    }

    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.autoContactCallbacks = m_OldAutoContactCallbacks;
        world.drawFillOptions = m_OldDrawFillOptions;

        if (m_FragmentGeometryMask.IsCreated)
            m_FragmentGeometryMask.Dispose();
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

        // Ground (a far-below plate for debris cleanup).
        {
            var groundBody = world.CreateBody();
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_GroundMask, contacts = m_ProjectileMask | m_DebrisMask | m_DestructibleMask },
            };
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(1000f, 50f), radius: 0f, new PhysicsTransform(Vector2.down * 150f)), shapeDef);
        }

        // Six tall destructible columns.
        for (var n = 0; n < 6; ++n)
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(n * 5f - 27f, -2f) });
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = m_DestructibleContactFilter,
                contactEvents = true,
                surfaceMaterial = m_DestructibleSurfaceMaterial
            };
            body.CreateShape(m_DestructGeometry, shapeDef);
        }
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        var leftPressed = Input.GetKey(KeyCode.LeftArrow);
        var rightPressed = Input.GetKey(KeyCode.RightArrow);
        var firePressed = Input.GetKey(KeyCode.Space);

        var movement = PlayerSpeed * Time.deltaTime;
        if (leftPressed) m_PlayerPosition.x -= movement;
        if (rightPressed) m_PlayerPosition.x += movement;
        m_PlayerPosition.x = Mathf.Clamp(m_PlayerPosition.x, -28f, 28f);

        m_ProjectileTime += Time.deltaTime;
        if (firePressed && m_ProjectileTime >= ProjectileDelay)
        {
            m_ProjectileTime = 0f;
            FirePressed();
        }

        world.DrawGeometry(m_PlayerGeometry, m_PlayerPosition, Color.azure);
        world.DrawGeometry(m_VirtualGroundGeometry, m_VirtualGroundTransform, m_DestructibleColor);
    }

    private void FirePressed()
    {
        var world = PhysicsWorld.defaultWorld;

        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            gravityScale = 10f,
            fastCollisionsAllowed = true,
            position = m_PlayerPosition + Vector2.down * (ProjectileRadius + 1.5f),
            linearVelocity = Vector2.down * ProjectileSpeed
        };
        var body = world.CreateBody(bodyDef);
        body.callbackTarget = this;

        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter { categories = m_ProjectileMask, contacts = m_DestructibleMask | m_GroundMask },
            contactEvents = true,
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.ghostWhite }
        };
        var shape = body.CreateShape(new CircleGeometry { radius = ProjectileRadius }, shapeDef);
        shape.callbackTarget = this;
    }

    private void UpdateFragmentGeometry()
    {
        m_FireGeometry = new CircleGeometry { radius = FragmentRadius };

        if (m_FragmentGeometryMask.IsCreated)
            m_FragmentGeometryMask.Dispose();

        var composer = PhysicsComposer.Create();
        composer.AddLayer(m_FireGeometry, PhysicsTransform.identity);
        m_FragmentGeometryMask = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Persistent);
        composer.Destroy();
    }

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        var shapeA = beginEvent.shapeA;
        var shapeB = beginEvent.shapeB;

        if (!shapeA.isValid || !shapeB.isValid)
            return;

        var categoryA = shapeA.contactFilter.categories;
        var categoryB = shapeB.contactFilter.categories;

        // Anything that hits the ground plate dies.
        if (categoryA == m_GroundMask || categoryB == m_GroundMask)
        {
            var destroyShape = categoryA == m_GroundMask ? shapeB : shapeA;
            destroyShape.body.Destroy();
            return;
        }

        // Projectile hits a destructible: fragment with island handling.
        if (categoryA == m_DestructibleMask || categoryB == m_DestructibleMask)
        {
            if (categoryA != m_ProjectileMask && categoryB != m_ProjectileMask)
                return;

            var contact = beginEvent.contactId.contact;
            var hitPosition = contact.manifold.points[0].point;

            var world = PhysicsWorld.defaultWorld;
            world.DrawCircle(hitPosition, FragmentRadius, Color.ivory, 2f / 60f, PhysicsWorld.DrawFillOptions.Outline);

            var destructibleShape = categoryA == m_DestructibleMask ? shapeA : shapeB;
            var destructibleBody = destructibleShape.body;
            var destructibleBodyType = destructibleBody.type;
            using var destructibleShapes = destructibleBody.GetShapes();

            var targetPolygons = new NativeList<PolygonGeometry>(initialCapacity: destructibleShapes.Length, Allocator.Temp);
            foreach (var shape in destructibleShapes)
            {
                if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
                    targetPolygons.Add(shape.polygonGeometry);
            }

            var targetGeometry = new PhysicsDestructor.FragmentGeometry(destructibleBody.transform, targetPolygons.AsReadOnly());
            targetPolygons.Dispose();

            var fragmentPoints = new NativeArray<Vector2>(FragmentCount, Allocator.Temp);
            fragmentPoints[0] = hitPosition;
            for (var i = 1; i < FragmentCount; ++i)
            {
                var rotate = PhysicsRotate.FromRadians(m_Random.NextFloat(0f, PhysicsMath.PI));
                var radius = m_Random.NextFloat(0.05f, FragmentRadius);
                fragmentPoints[i] = hitPosition + rotate.direction * radius;
            }

            var maskGeometry = new PhysicsDestructor.FragmentGeometry(hitPosition, m_FragmentGeometryMask);
            using var fragmentResults = PhysicsDestructor.Fragment(targetGeometry, maskGeometry, fragmentPoints, Allocator.Temp);

            fragmentPoints.Dispose();

            shapeA.body.Destroy();
            shapeB.body.Destroy();

            var fragmentTransform = fragmentResults.transform;

            var staticBodyDef = new PhysicsBodyDefinition { position = fragmentTransform.position, rotation = fragmentTransform.rotation };
            var dynamicBodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                fastCollisionsAllowed = true,
                position = fragmentTransform.position,
                rotation = fragmentTransform.rotation,
                gravityScale = 4f
            };

            // Walk geometry islands; each becomes its own body. Static if it touches the virtual ground.
            if (fragmentResults.unbrokenGeometry.Length > 0)
            {
                var unbrokenGeometry = fragmentResults.unbrokenGeometry;
                var unbrokenGeometryIslands = fragmentResults.unbrokenGeometryIslands;

                foreach (var islandRange in unbrokenGeometryIslands)
                {
                    var unbrokenAsDynamic = true;
                    var islandGeometry = unbrokenGeometry.GetSubArray(islandRange.start, islandRange.length);

                    if (destructibleBodyType == PhysicsBody.BodyType.Static)
                    {
                        foreach (var geometry in islandGeometry)
                        {
                            if (geometry.Intersect(fragmentTransform, m_VirtualGroundGeometry, m_VirtualGroundTransform).pointCount == 0)
                                continue;
                            unbrokenAsDynamic = false;
                            break;
                        }
                    }

                    var body = world.CreateBody(unbrokenAsDynamic ? dynamicBodyDef : staticBodyDef);

                    var shapeDef = new PhysicsShapeDefinition
                    {
                        contactFilter = m_DestructibleContactFilter,
                        contactEvents = true,
                        surfaceMaterial = m_DestructibleSurfaceMaterial
                    };

                    using var shapeBatch = body.CreateShapeBatch(islandGeometry, shapeDef);

                    if (unbrokenAsDynamic)
                    {
                        foreach (var shape in shapeBatch)
                            shape.callbackTarget = this;
                    }
                }
            }

            // Broken pieces → debris batch.
            {
                var brokenCount = fragmentResults.brokenGeometry.Length;
                if (brokenCount > 0)
                {
                    using var bodies = world.CreateBodyBatch(dynamicBodyDef, brokenCount);

                    var dynamicShapeColor = Colors switch
                    {
                        FragmentColors.Off or FragmentColors.Individual => m_DestructibleColor,
                        FragmentColors.White => Color.ivory,
                        FragmentColors.Group => Color.HSVToRGB(m_Random.NextFloat(0f, 1f), 0.7f, 0.9f),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var shapeDef = new PhysicsShapeDefinition
                    {
                        contactFilter = new PhysicsShape.ContactFilter { categories = m_DebrisMask, contacts = m_GroundMask | m_DestructibleMask },
                        contactEvents = true,
                        surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = dynamicShapeColor }
                    };

                    for (var i = 0; i < brokenCount; ++i)
                    {
                        var body = bodies[i];
                        var geometry = fragmentResults.brokenGeometry[i];

                        if (Colors == FragmentColors.Individual)
                            shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.HSVToRGB(m_Random.NextFloat(0f, 1f), 0.7f, 0.9f) };

                        var shape = body.CreateShape(geometry, shapeDef);
                        shape.callbackTarget = this;
                    }
                }
            }

            if (FragmentExplode)
            {
                world.Explode(new PhysicsWorld.ExplosionDefinition
                {
                    position = hitPosition + Vector2.up * FragmentRadius,
                    hitCategories = m_DebrisMask,
                    impulsePerLength = 2f,
                    radius = FragmentRadius * 3f
                });
            }
        }
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent) { }
}
