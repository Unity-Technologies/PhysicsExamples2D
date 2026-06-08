using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrates how a Chain Shape doesn't produce \"ghost\" collisions.")]
public sealed class ChainShape : SandboxExampleBehaviour
{
    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Box = 2
    }

    private ObjectType m_ObjectType;
    private int m_ObjectCount;
    private const float Friction = 0.1f;
    private float m_GravityScale;
    private bool m_FastCollisionsAllowed;

    private int m_ItemsSpawned;
    private const float SpawnPeriod = 1.75f;
    private float m_SpawnTime;

    protected override float CameraSize => 50f;
    protected override Vector2 CameraPosition => new(0f, -3f);

    protected override void OnExampleEnable()
    {
        m_ObjectType = ObjectType.Box;
        m_ObjectCount = 100;

        m_GravityScale = 10f;
        m_FastCollisionsAllowed = false;
    }

    private void Update()
    {
        // Finish if the world is paused.
        if (SandboxManager.WorldPaused)
            return;

        if (m_ItemsSpawned >= m_ObjectCount)
            return;

        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0)
            return;

        m_SpawnTime = SpawnPeriod / math.sqrt(m_GravityScale);
        ++m_ItemsSpawned;

        // Sliding Object.
        {
            var world = World;

            var startPosition = new Vector2(-55f, 13.5f);
            var startLinearVelocity = new Vector2(2f, -1f);

            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, gravityScale = m_GravityScale, fastCollisionsAllowed = m_FastCollisionsAllowed, position = startPosition, linearVelocity = startLinearVelocity };
            var body = world.CreateBody(bodyDef);

            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = Friction, bounciness = 0f, customColor = ShapeColor } };

            switch (m_ObjectType)
            {
                case ObjectType.Circle:
                {
                    body.CreateShape(new CircleGeometry { radius = 1f }, shapeDef);
                    break;
                }

                case ObjectType.Capsule:
                {
                    body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-0.75f, 0f), center2 = new Vector2(0.75f, 0f), radius = 0.75f }, shapeDef);
                    break;
                }

                case ObjectType.Box:
                {
                    body.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 2f)), shapeDef);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    protected override void SetupOptions()
    {
        // Object Type.
        AddEnum("Object Type", m_ObjectType, v => m_ObjectType = v, rebuild: true);

        // Object Count.
        AddSliderInt("Object Count", m_ObjectCount, 1, 100, v => m_ObjectCount = v, rebuild: true);

        // Gravity Scale (applied per-body on spawn; rebuilds to respawn with the new scale).
        AddSlider("Gravity Scale", m_GravityScale, 1f, 20f, v => m_GravityScale = v, rebuild: true);

        // Fast Collisions.
        AddToggle("Fast Collisions", m_FastCollisionsAllowed, v => m_FastCollisionsAllowed = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        m_ItemsSpawned = 0;
        m_SpawnTime = 0f;

        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();

            using var points = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-60.885498f, 12.8985004f), new(-60.885498f, 16.2057495f), new(60.885498f, 16.2057495f), new(60.885498f, -30.2057514f),
                new(51.5935059f, -30.2057514f), new(43.6559982f, -10.9139996f), new(35.7184982f, -10.9139996f), new(27.7809982f, -10.9139996f),
                new(21.1664963f, -14.2212505f), new(11.9059982f, -16.2057514f), new(0f, -16.2057514f), new(-10.5835037f, -14.8827496f),
                new(-17.1980019f, -13.5597477f), new(-21.1665001f, -12.2370014f), new(-25.1355019f, -9.5909977f), new(-31.75f, -3.63799858f),
                new(-38.3644981f, 5.0840004f), new(-42.3334999f, 8.59125137f), new(-47.625f, 10.5755005f), new(-60.885498f, 12.8985004f),
            };

            var chainGeometry = new ChainGeometry(points.AsArray());
            var chainDef = new PhysicsChainDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0f } };
            groundBody.CreateChain(chainGeometry, chainDef);
        }
    }
}
