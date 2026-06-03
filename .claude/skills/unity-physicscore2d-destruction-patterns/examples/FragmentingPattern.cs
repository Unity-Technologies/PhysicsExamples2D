using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Top-down shooter that fragments a destructible block on impact. A projectile collides with the destructible
/// (`OnContactBegin2D`), triggers PhysicsDestructor.Fragment with a circular mask + random fragment seed points,
/// destroys the original body, then rebuilds two body groups: an unbroken remainder body and a batch of debris
/// bodies. Optional explosion impulse pushes the debris outward.
/// </summary>
public class FragmentingPattern : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    public enum FragmentColors { Off, White, Group, Individual }

    public float FragmentRadius = 2f;
    public int FragmentCount = 100;
    public FragmentColors Colors = FragmentColors.Individual;
    public bool FragmentExplode = false;
    public uint RandomSeed = 1234;

    private bool m_OldAutoContactCallbacks;
    private PhysicsWorld.DrawFillOptions m_OldDrawFillOptions;

    private readonly PhysicsMask m_ObstacleMask = new(1);
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
    private Vector2 m_PlayerPosition;
    private CapsuleGeometry m_PlayerGeometry;
    private CircleGeometry m_FireGeometry;
    private PolygonGeometry m_DestructGeometry;
    private NativeArray<PolygonGeometry> m_FragmentGeometryMask;

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
        m_DestructibleContactFilter = new PhysicsShape.ContactFilter { categories = m_DestructibleMask, contacts = m_ProjectileMask | m_DebrisMask };
        m_DestructibleSurfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f, bounciness = 0.4f, tangentSpeed = 0.1f, customColor = m_DestructibleColor };

        m_PlayerGeometry = new CapsuleGeometry { center1 = Vector2.zero, center2 = Vector2.up, radius = 0.5f };
        m_DestructGeometry = PolygonGeometry.CreateBox(new Vector2(50f, 20f));

        m_PlayerPosition = new Vector2(0f, -8f);

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

        // Ground above and below the play area.
        {
            var groundBody = world.CreateBody();
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_GroundMask, contacts = m_ProjectileMask | m_DebrisMask },
            };
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(500f, 50f), radius: 0f, new PhysicsTransform(Vector2.down * 40f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(500f, 50f), radius: 0f, new PhysicsTransform(Vector2.up * 68f)), shapeDef);
        }

        // Static obstacle field (catches falling debris).
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(-23f, 1f) });
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_ObstacleMask, contacts = m_DebrisMask },
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f, bounciness = 0.4f, tangentSpeed = 0.5f, customColor = Color.gray2 }
            };

            const float radius = 1.5f;
            for (var n = 0; n < 7; ++n)
            {
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, -8f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f - 4f, -5f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, -2f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f - 4f, 1f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, 4f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f - 4f, 7f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, 10f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f - 4f, 13f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, 16f), radius = radius }, shapeDef);
            }
        }

        // The destructible target.
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { position = Vector2.up * 30f });
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = m_DestructibleContactFilter,
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
        var firePressed = Input.GetKeyDown(KeyCode.Space);

        var movement = PlayerSpeed * Time.deltaTime;
        if (leftPressed) m_PlayerPosition.x -= movement;
        if (rightPressed) m_PlayerPosition.x += movement;
        m_PlayerPosition.x = Mathf.Clamp(m_PlayerPosition.x, -26f, 26f);

        if (firePressed)
            FirePressed();

        world.DrawGeometry(m_PlayerGeometry, m_PlayerPosition, Color.azure);
    }

    private void FirePressed()
    {
        var world = PhysicsWorld.defaultWorld;

        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            gravityScale = 0f,
            fastCollisionsAllowed = true,
            position = m_PlayerPosition + Vector2.up * (ProjectileRadius + 1.5f),
            linearVelocity = Vector2.up * ProjectileSpeed
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

        // A previous callback in this batch may have already destroyed one of these shapes.
        if (!shapeA.isValid || !shapeB.isValid)
            return;

        var categoryA = shapeA.contactFilter.categories;
        var categoryB = shapeB.contactFilter.categories;

        // Projectile hit ground → destroy the projectile.
        if (categoryA == m_GroundMask || categoryB == m_GroundMask)
        {
            var destroyShape = categoryA == m_GroundMask ? shapeB : shapeA;
            destroyShape.body.Destroy();
            return;
        }

        // Projectile hit destructible → fragment.
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
            using var destructibleShapes = destructibleBody.GetShapes();

            // Pull all polygon geometries off the destructible body. (Polygon-only here by construction.)
            var targetPolygons = new NativeList<PolygonGeometry>(initialCapacity: destructibleShapes.Length, Allocator.Temp);
            foreach (var shape in destructibleShapes)
            {
                if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
                    targetPolygons.Add(shape.polygonGeometry);
            }

            var targetGeometry = new PhysicsDestructor.FragmentGeometry(destructibleBody.transform, targetPolygons.AsReadOnly());
            targetPolygons.Dispose();

            // Random fragment seed points within the impact radius.
            var fragmentPoints = new NativeArray<Vector2>(FragmentCount, Allocator.Temp);
            fragmentPoints[0] = hitPosition;
            for (var i = 1; i < FragmentCount; ++i)
            {
                var rotate = PhysicsRotate.FromRadians(m_Random.NextFloat(0f, PhysicsMath.PI));
                var radius = m_Random.NextFloat(0.05f, FragmentRadius);
                fragmentPoints[i] = hitPosition + rotate.direction * radius;
            }

            // Fragment(target, mask, points) splits the target geometry into "broken" pieces (per fragment point)
            // and an "unbroken" remainder. The mask restricts which area gets fragmented.
            var maskGeometry = new PhysicsDestructor.FragmentGeometry(hitPosition, m_FragmentGeometryMask);
            using var fragmentResults = PhysicsDestructor.Fragment(targetGeometry, maskGeometry, fragmentPoints, Allocator.Temp);

            fragmentPoints.Dispose();

            // Destroy the colliding pair so the new bodies replace them.
            shapeA.body.Destroy();
            shapeB.body.Destroy();

            var fragmentTransform = fragmentResults.transform;

            // Rebuild the unbroken remainder as one body.
            if (fragmentResults.unbrokenGeometry.Length > 0)
            {
                var body = world.CreateBody(new PhysicsBodyDefinition { position = fragmentTransform.position, rotation = fragmentTransform.rotation });
                var shapeDef = new PhysicsShapeDefinition
                {
                    contactFilter = m_DestructibleContactFilter,
                    surfaceMaterial = m_DestructibleSurfaceMaterial
                };

                foreach (var geometry in fragmentResults.unbrokenGeometry)
                    body.CreateShape(geometry, shapeDef);
            }

            // Rebuild the broken pieces as a batch of dynamic debris bodies.
            {
                var brokenCount = fragmentResults.brokenGeometry.Length;

                var bodyDef = new PhysicsBodyDefinition
                {
                    type = PhysicsBody.BodyType.Dynamic,
                    fastCollisionsAllowed = true,
                    position = fragmentTransform.position,
                    rotation = fragmentTransform.rotation,
                    gravityScale = 4f
                };
                using var bodies = world.CreateBodyBatch(bodyDef, brokenCount);

                var shapeColor = Colors switch
                {
                    FragmentColors.Off or FragmentColors.Individual => m_DestructibleColor,
                    FragmentColors.White => Color.ivory,
                    FragmentColors.Group => Color.HSVToRGB(m_Random.NextFloat(0f, 1f), 0.7f, 0.9f),
                    _ => throw new ArgumentOutOfRangeException()
                };

                var shapeDef = new PhysicsShapeDefinition
                {
                    contactFilter = new PhysicsShape.ContactFilter { categories = m_DebrisMask, contacts = m_GroundMask | m_DestructibleMask | m_ObstacleMask },
                    contactEvents = true,
                    surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = shapeColor }
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

            if (FragmentExplode)
            {
                world.Explode(new PhysicsWorld.ExplosionDefinition
                {
                    position = hitPosition + Vector2.up * FragmentRadius,
                    hitCategories = m_DebrisMask,
                    impulsePerLength = 4f,
                    radius = FragmentRadius * 3f
                });
            }
        }
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent) { }
}
