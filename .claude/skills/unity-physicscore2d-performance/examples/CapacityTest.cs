using System;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Body/shape/contact capacity benchmark. Spawns batches of dynamic bodies until the per-frame simulation step
/// exceeds `SimulationLimit` ms for `LimitReachedCount` consecutive frames; then reports counts via
/// `world.counters` (bodyCount, shapeCount, contactCount). Disables sleeping during the test so all bodies
/// remain active. Useful for capacity-planning a target device.
/// </summary>
public class CapacityTest : MonoBehaviour
{
    public enum ShapeType { Circle, Polygon, Capsule }

    public ShapeType Type = ShapeType.Circle;
    public int SimulationLimit = 20;

    private const int LimitReachedCount = 60;
    private const int SimulationSpawnPeriod = 0x1F;

    private PolygonGeometry m_PolygonGeometry;
    private CircleGeometry m_CircleGeometry;
    private CapsuleGeometry m_CapsuleGeometry;
    private float m_SpawnOffset;

    private float m_OldMaximumDeltaTime;
    private bool m_OldWorldSleeping;

    private int m_SimulationCounter;
    private int m_LimitReachedCount;
    private bool m_Finished;

    private void OnEnable()
    {
        m_PolygonGeometry = PolygonGeometry.CreateBox(Vector2.one);
        m_CircleGeometry = new CircleGeometry { radius = 0.5f };
        m_CapsuleGeometry = new CapsuleGeometry { center1 = Vector2.left * 0.5f, center2 = Vector2.right * 0.5f, radius = 0.5f };
        m_SpawnOffset = 0f;

        // Match Time.maximumDeltaTime to fixedDeltaTime so we don't compound multiple sub-steps in one frame.
        m_OldMaximumDeltaTime = Time.maximumDeltaTime;
        Time.maximumDeltaTime = Time.fixedDeltaTime;

        // Disable world-level sleeping so all bodies stay in the active set during the test.
        var world = PhysicsWorld.defaultWorld;
        m_OldWorldSleeping = world.bodySleeping;
        world.bodySleeping = false;

        m_SimulationCounter = 0;
        m_LimitReachedCount = 0;
        m_Finished = false;

        SetupScene();
        PhysicsEvents.PostSimulate += OnPostSimulation;
    }

    private void OnDisable()
    {
        Time.maximumDeltaTime = m_OldMaximumDeltaTime;
        var world = PhysicsWorld.defaultWorld;
        if (world.isValid)
            world.bodySleeping = m_OldWorldSleeping;

        PhysicsEvents.PostSimulate -= OnPostSimulation;
    }

    private void SetupScene()
    {
        m_SimulationCounter = 0;
        m_LimitReachedCount = 0;
        m_Finished = false;

        var world = PhysicsWorld.defaultWorld;
        var groundBody = world.CreateBody(new PhysicsBodyDefinition { position = Vector2.down * 5f });
        groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(1600f, 10f)));
    }

    private void OnPostSimulation(PhysicsWorld world, float timeStep)
    {
        if (m_Finished)
            return;

        if (!world.isDefaultWorld)
            return;

        // world.profile.simulationStep is the wall-clock time spent inside the most recent step.
        var simulationStep = world.profile.simulationStep;

        if (simulationStep > SimulationLimit)
        {
            if (++m_LimitReachedCount > LimitReachedCount)
            {
                var counter = world.counters;
                m_Finished = true;
                // Final counts are visible via world.counters when finished.
                return;
            }
        }
        else
        {
            m_LimitReachedCount = 0;
        }

        if (++m_SimulationCounter % SimulationSpawnPeriod != 0)
            return;

        // Spawn batch of 200 dynamic bodies.
        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;

        const int count = 200;
        var position = new Vector2(-count + m_SpawnOffset, 200f);
        for (var n = 0; n < count; ++n)
        {
            position.y += 0.5f;
            bodyDef.position = position;
            position.x += 2f;

            switch (Type)
            {
                case ShapeType.Circle:
                    world.CreateBody(bodyDef).CreateShape(m_CircleGeometry, shapeDef);
                    continue;
                case ShapeType.Polygon:
                    world.CreateBody(bodyDef).CreateShape(m_PolygonGeometry, shapeDef);
                    continue;
                case ShapeType.Capsule:
                    world.CreateBody(bodyDef).CreateShape(m_CapsuleGeometry, shapeDef);
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        m_SpawnOffset = 0.5f - m_SpawnOffset;
    }
}
