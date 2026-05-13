using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Player-controlled slicer in a circular arena. Press Left/Right to orbit, Space to fire a slice ray.
/// The ray cuts polygon shapes via PhysicsDestructor.Slice; reflections off the chain wall optionally
/// continue the slice through additional bodies. Demonstrates the slice-fragment-rebuild loop used for
/// 2D destructible geometry.
/// </summary>
public class SlicingPattern : MonoBehaviour
{
    public int ReflectionCount = 0;
    public int MaximumFragments = 5000;
    public uint RandomSeed = 1234;

    private PhysicsWorld.DrawFillOptions m_OldDrawFillOptions;

    private readonly PhysicsMask m_GroundMask = new(2);
    private readonly PhysicsMask m_DestructibleMask = new(3);

    private PhysicsShapeDefinition m_DestructibleShapeDef;
    private PhysicsBodyDefinition m_DestructibleBodyDef;

    private const float SeparateSpeed = 6f;
    private const float ArenaRadius = 20f;
    private const float ArenaInset = 0.1f;
    private PhysicsTransform m_PlayerTransform;
    private PhysicsTransform m_FireTransform;
    private float m_PlayerAngle;
    private float m_PlayerSpeed;
    private CapsuleGeometry m_PlayerGeometry;

    private Random m_Random;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;

        // Interior fill makes slice results visually clear.
        m_OldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;

        m_Random = new Random(RandomSeed);

        var destructibleColor = Color.seaGreen;
        m_DestructibleShapeDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter { categories = m_DestructibleMask, contacts = PhysicsMask.All },
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f, bounciness = 0.4f, tangentSpeed = 0.1f, customColor = destructibleColor },
            // Defer mass calc until we ApplyMassFromShapes() — faster when adding many shapes to one body.
            startMassUpdate = false
        };
        m_DestructibleBodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, gravityScale = 0f };

        m_PlayerGeometry = new CapsuleGeometry { center1 = Vector2.left, center2 = Vector2.right, radius = 1f };
        m_PlayerSpeed = PhysicsMath.PI;
        m_PlayerAngle = PhysicsMath.ToRadians(-90f);
        UpdatePlayerTransform();

        SetupScene();
    }

    private void OnDisable()
    {
        PhysicsWorld.defaultWorld.drawFillOptions = m_OldDrawFillOptions;
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

        // Chain Surround.
        {
            var groundBody = world.CreateBody();

            const int pointCount = 360;
            var chainPoints = new NativeArray<Vector2>(pointCount, Allocator.Temp);

            var tau = PhysicsMath.TAU;
            var rotate = PhysicsRotate.FromRadians(-tau / pointCount);
            var offset = Vector2.right * ArenaRadius;
            for (var i = 0; i < pointCount; ++i)
            {
                chainPoints[i] = new Vector2(offset.x, offset.y);
                offset = rotate.RotateVector(offset);
            }

            var chainDef = new PhysicsChainDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_GroundMask, contacts = PhysicsMask.All },
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0.5f }
            };
            groundBody.CreateChain(new ChainGeometry(chainPoints), chainDef);

            chainPoints.Dispose();
        }

        // Initial destructible block — single big polygon.
        {
            var body = world.CreateBody(m_DestructibleBodyDef);
            body.CreateShape(PolygonGeometry.CreateBox(Vector2.one * ArenaRadius * 1.25f), m_DestructibleShapeDef);
            // Now compute mass since we deferred it via startMassUpdate=false above.
            body.ApplyMassFromShapes();
        }
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        var leftPressed = Input.GetKey(KeyCode.LeftArrow);
        var rightPressed = Input.GetKey(KeyCode.RightArrow);
        var firePressed = Input.GetKeyDown(KeyCode.Space);

        var movement = m_PlayerSpeed * Time.deltaTime;
        if (leftPressed) m_PlayerAngle += movement;
        if (rightPressed) m_PlayerAngle -= movement;

        m_PlayerAngle = PhysicsRotate.UnwindAngle(m_PlayerAngle);
        UpdatePlayerTransform();

        if (firePressed)
            FirePressed();

        world.DrawGeometry(m_PlayerGeometry, m_PlayerTransform, Color.azure);
    }

    private void UpdatePlayerTransform()
    {
        var rotation = PhysicsRotate.FromRadians(m_PlayerAngle);
        m_PlayerTransform = new PhysicsTransform(rotation.direction * (ArenaRadius + 2f), rotation);
        m_FireTransform = new PhysicsTransform(rotation.direction * (ArenaRadius - ArenaInset), rotation);
    }

    private void FirePressed()
    {
        var world = PhysicsWorld.defaultWorld;

        if (world.counters.bodyCount > MaximumFragments)
            return;

        var fireOrigin = m_FireTransform.position;
        var fireTranslation = -m_FireTransform.rotation.direction * ArenaRadius * 3f;

        // For each reflection: ray-cast, find hit destructibles + ground reflection point, slice and rebuild.
        NativeHashSet<PhysicsBody> destructibleBodies = default;
        for (var n = 0; n <= ReflectionCount; ++n)
        {
            var sliceRayInput = new PhysicsQuery.CastRayInput { origin = fireOrigin, translation = fireTranslation };

            using var results = world.CastRay(
                sliceRayInput,
                new PhysicsQuery.QueryFilter { categories = PhysicsMask.All, hitCategories = m_DestructibleMask | m_GroundMask },
                PhysicsQuery.WorldCastMode.AllSorted);

            if (results.Length == 0)
                break;

            if (!destructibleBodies.IsCreated)
                destructibleBodies = new NativeHashSet<PhysicsBody>(results.Length, Allocator.Temp);

            foreach (var castHit in results)
            {
                var hitCategory = castHit.shape.contactFilter.categories;

                if (hitCategory == m_DestructibleMask)
                {
                    destructibleBodies.Add(castHit.shape.body);
                    continue;
                }

                if (hitCategory == m_GroundMask)
                {
                    fireOrigin = castHit.point + castHit.normal * ArenaInset;
                    var reflectAngle = PhysicsRotate.FromRadians(new PhysicsRotate(castHit.normal).radians + m_Random.NextFloat(-0.5f, 0.5f)).direction;
                    fireTranslation = reflectAngle * ArenaRadius * 2f;
                    world.DrawLine(sliceRayInput.origin, castHit.point, Color.ghostWhite, 30f / 60f);
                }
            }

            if (destructibleBodies.Count == 0)
                continue;

            var targetPolygons = new NativeList<PolygonGeometry>(10, Allocator.Temp);
            foreach (var hitBody in destructibleBodies)
            {
                using var targetShapes = hitBody.GetShapes();
                foreach (var shape in targetShapes)
                {
                    if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
                        targetPolygons.Add(shape.polygonGeometry);
                }

                if (targetPolygons.Length == 0)
                    continue;

                var targetGeometry = new PhysicsDestructor.FragmentGeometry(hitBody.transform, targetPolygons.AsArray());

                using var sliceResult = PhysicsDestructor.Slice(targetGeometry, sliceRayInput.origin, sliceRayInput.translation, Allocator.Temp);

                targetPolygons.Clear();

                var hasLeftSlice = sliceResult.leftGeometry.Length > 0;
                var hasRightSlice = sliceResult.rightGeometry.Length > 0;

                if (!hasLeftSlice && !hasRightSlice)
                    continue;

                hitBody.Destroy();

                var sliceTransform = sliceResult.transform;
                var bodyDef = m_DestructibleBodyDef;
                bodyDef.position = sliceTransform.position;
                bodyDef.rotation = sliceTransform.rotation;

                var sliceNormal = sliceRayInput.translation.normalized;
                var slicePerp = Vector2.Perpendicular(sliceNormal);

                var shapeDef = m_DestructibleShapeDef;

                if (hasLeftSlice)
                {
                    var body = world.CreateBody(bodyDef);
                    foreach (var polygon in sliceResult.leftGeometry)
                        body.CreateShape(polygon, shapeDef);
                    body.ApplyMassFromShapes();
                    body.linearVelocity = slicePerp * (Vector2.Dot(sliceNormal, sliceRayInput.origin - body.worldCenterOfMass) > 0f ? SeparateSpeed : -SeparateSpeed);
                }

                if (hasRightSlice)
                {
                    var body = world.CreateBody(bodyDef);
                    foreach (var polygon in sliceResult.rightGeometry)
                        body.CreateShape(polygon, shapeDef);
                    body.ApplyMassFromShapes();
                    body.linearVelocity = -slicePerp * (Vector2.Dot(sliceNormal, sliceRayInput.origin - body.worldCenterOfMass) > 0f ? SeparateSpeed : -SeparateSpeed);
                }
            }

            targetPolygons.Dispose();
            destructibleBodies.Clear();
        }

        if (destructibleBodies.IsCreated)
            destructibleBodies.Dispose();
    }
}
