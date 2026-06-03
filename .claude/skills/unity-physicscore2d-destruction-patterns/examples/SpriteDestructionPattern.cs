using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// "Destruct at point" pattern with split-into-islands behaviour. Two destructible polygon blocks; press Space
/// to fragment at a moving target position. The unbroken remainder is split into geometry islands — each island
/// becomes a static body if it intersects the virtual ground, or a dynamic body otherwise. The broken pieces
/// become a batch of dynamic debris bodies and an explosion impulse pushes them outward.
///
/// This is the destruction half of the Sandbox SpriteDestruction example with the sprite-rendering layer
/// removed; the destruction pattern itself is independent of how (or whether) you render the result.
/// </summary>
public class SpriteDestructionPattern : MonoBehaviour, PhysicsCallbacks.IContactCallback
{
    public Vector2 HitPosition = new Vector2(-7f, -2f);
    public float FragmentRadius = 2f;
    public bool FragmentCreate = true;
    public int FragmentCount = 20;
    public float FragmentFriction = 0.5f;
    public float FragmentBounciness = 0f;
    public float FragmentForce = 10f;
    public float GravityScale = 5f;
    public uint RandomSeed = 1234;

    private bool m_OldAutoContactCallbacks;
    private Vector2 m_OldGravity;
    private PhysicsWorld.DrawFillOptions m_OldDrawFillOptions;

    private readonly PhysicsMask m_ObstacleMask = new(1);
    private readonly PhysicsMask m_GroundMask = new(2);
    private readonly PhysicsMask m_DestructibleMask = new(3);
    private readonly PhysicsMask m_DebrisMask = new(4);

    private Color m_DestructibleColor;
    private PhysicsShape.ContactFilter m_DestructibleContactFilter;
    private PhysicsShape.SurfaceMaterial m_DestructibleSurfaceMaterial;

    private NativeArray<PolygonGeometry> m_FragmentGeometryMask;

    private SegmentGeometry m_VirtualGroundGeometry;
    private PhysicsTransform m_VirtualGroundTransform;

    private Random m_Random;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;

        m_OldGravity = world.gravity;
        world.gravity = m_OldGravity * GravityScale;

        m_OldAutoContactCallbacks = world.autoContactCallbacks;
        world.autoContactCallbacks = true;
        m_OldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;

        m_Random = new Random(RandomSeed);

        m_DestructibleColor = new Color(0.1f, 0f, 0f, 0f);
        m_DestructibleContactFilter = new PhysicsShape.ContactFilter { categories = m_DestructibleMask, contacts = m_GroundMask | m_DebrisMask | m_DestructibleMask };

        // Virtual ground: imaginary line used to decide which unbroken islands stay static vs become dynamic.
        m_VirtualGroundGeometry = new SegmentGeometry { point1 = new Vector2(-100f, 0f), point2 = new Vector2(100f, 0f) };
        m_VirtualGroundTransform = new PhysicsTransform(Vector2.down * 7.25f);

        CreateFragmentMaskGeometry();
        SetupScene();
    }

    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.autoContactCallbacks = m_OldAutoContactCallbacks;
        world.gravity = m_OldGravity;
        world.drawFillOptions = m_OldDrawFillOptions;

        if (m_FragmentGeometryMask.IsCreated)
            m_FragmentGeometryMask.Dispose();
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

        // Ground (4 walls of a closed box).
        {
            var groundBody = world.CreateBody();
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_GroundMask, contacts = m_DebrisMask | m_DestructibleMask },
            };
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(110f, 10f), radius: 0f, new PhysicsTransform(Vector2.up * 50f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(110f, 10f), radius: 0f, new PhysicsTransform(Vector2.down * 50f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, 100f), radius: 0f, new PhysicsTransform(Vector2.left * 50f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(10f, 100f), radius: 0f, new PhysicsTransform(Vector2.right * 50f)), shapeDef);
        }

        // Two static destructible blocks to fragment.
        CreateInitialBlock(Vector2.left * 7f);
        CreateInitialBlock(Vector2.right * 6f);
    }

    private void CreateInitialBlock(Vector2 worldPosition)
    {
        var world = PhysicsWorld.defaultWorld;

        var physicsBody = world.CreateBody(new PhysicsBodyDefinition { position = worldPosition });

        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = m_DestructibleContactFilter,
            contactEvents = true,
            surfaceMaterial = m_DestructibleSurfaceMaterial
        };

        physicsBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(5f, 5f)), shapeDef);
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        world.DrawGeometry(m_VirtualGroundGeometry, m_VirtualGroundTransform, Color.saddleBrown);
        world.DrawGeometry(m_FragmentGeometryMask, HitPosition, Color.dodgerBlue, 0f, PhysicsWorld.DrawFillOptions.Outline);

        if (Input.GetKeyDown(KeyCode.Space))
            DestructAtPosition(HitPosition);
    }

    private void DestructAtPosition(Vector2 hitPosition)
    {
        var world = PhysicsWorld.defaultWorld;

        // Refresh the mask each invocation so the random-radius lobes vary.
        CreateFragmentMaskGeometry();

        using var hits = world.OverlapPoint(hitPosition, new PhysicsQuery.QueryFilter { categories = m_DestructibleMask, hitCategories = m_DestructibleMask });
        if (hits.Length > 0)
        {
            var hit = hits[0];

            var destructibleShape = hit.shape;
            var destructibleBody = destructibleShape.body;
            var destructibleBodyType = destructibleBody.type;
            var destructibleBodyLinearVelocity = destructibleBody.linearVelocity;
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

            m_DestructibleSurfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = FragmentFriction, bounciness = FragmentBounciness, tangentSpeed = 0f, customColor = m_DestructibleColor };

            var fragmentTransform = fragmentResults.transform;

            var staticBodyDef = new PhysicsBodyDefinition
            {
                position = fragmentTransform.position,
                rotation = fragmentTransform.rotation
            };

            var dynamicBodyDef = new PhysicsBodyDefinition
            {
                type = PhysicsBody.BodyType.Dynamic,
                fastCollisionsAllowed = true,
                position = fragmentTransform.position,
                rotation = fragmentTransform.rotation,
                linearVelocity = destructibleBodyLinearVelocity,
                gravityScale = 1.0f
            };

            // Unbroken remainder split into geometry islands. Each island that intersects the virtual ground
            // stays static; the rest become dynamic and fall.
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

            if (FragmentCreate)
            {
                var brokenCount = fragmentResults.brokenGeometry.Length;
                if (brokenCount > 0)
                {
                    using var bodies = world.CreateBodyBatch(dynamicBodyDef, brokenCount);

                    var shapeDef = new PhysicsShapeDefinition
                    {
                        contactFilter = new PhysicsShape.ContactFilter { categories = m_DebrisMask, contacts = m_GroundMask | m_DestructibleMask | m_ObstacleMask | m_DebrisMask },
                        contactEvents = true,
                        surfaceMaterial = m_DestructibleSurfaceMaterial
                    };

                    for (var i = 0; i < brokenCount; ++i)
                    {
                        var body = bodies[i];
                        var geometry = fragmentResults.brokenGeometry[i];

                        var shape = body.CreateShape(geometry, shapeDef);
                        shape.callbackTarget = this;
                    }
                }
            }
        }

        if (FragmentCreate && FragmentForce > 0f)
        {
            world.Explode(new PhysicsWorld.ExplosionDefinition
            {
                position = hitPosition,
                hitCategories = m_DebrisMask,
                impulsePerLength = FragmentForce,
                radius = FragmentRadius * 2f
            });
        }
    }

    private void CreateFragmentMaskGeometry()
    {
        if (m_FragmentGeometryMask.IsCreated)
            m_FragmentGeometryMask.Dispose();

        var vertices = new NativeList<Vector2>(100, Allocator.Temp);
        var stride = PhysicsMath.TAU / 36f;
        for (var angle = 0f; angle < PhysicsMath.TAU; angle += stride)
        {
            PhysicsMath.CosSin(angle, out var cosine, out var sine);
            var radius = m_Random.NextFloat(FragmentRadius * 0.9f, FragmentRadius * 1.1f);
            vertices.Add(new Vector2(cosine * radius, sine * radius));
        }

        m_FragmentGeometryMask = PolygonGeometry.CreatePolygons(vertices.AsArray(), PhysicsTransform.identity, Vector2.one, Allocator.Persistent);
        vertices.Dispose();
    }

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        var shapeA = beginEvent.shapeA;
        var shapeB = beginEvent.shapeB;

        if (!shapeA.isValid || !shapeB.isValid)
            return;

        var categoryA = shapeA.contactFilter.categories;
        var categoryB = shapeB.contactFilter.categories;

        if (categoryA != m_GroundMask && categoryB != m_GroundMask)
            return;

        // Debris that hit a wall is destroyed.
        var destroyShape = categoryA == m_GroundMask ? shapeB : shapeA;
        if (destroyShape.body.isValid)
            destroyShape.body.Destroy();
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent) { }
}
