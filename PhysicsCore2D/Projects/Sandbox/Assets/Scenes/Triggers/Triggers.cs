using System;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

[ExampleScene("Benchmarks", "A dense field of trigger shapes processing thousands of trigger events.")]
public sealed class Triggers : SandboxExampleBehaviour
{
    private int m_StepCount;
    private const int SpawnPeriod = 31;

    private int m_ColumnCount = 80;
    private const float ColumnSpacing = 2.5f;
    private const int RowCount = 40;

    private const UInt64 DestroyLayer = 1 << 0;
    private const UInt64 TriggerLayer = 1 << 1;
    private const UInt64 VisitorLayer = 1 << 2;

    private readonly Color m_DefaultFallingColor = Color.gray4;

    protected override float CameraSize => 120f;
    protected override Vector2 CameraPosition => new(0, 100f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(true);

        PhysicsEvents.PostSimulate += OnPostSimulation;
    }

    protected override void OnExampleDisable()
    {
        PhysicsEvents.PostSimulate -= OnPostSimulation;
    }

    protected override void SetupOptions()
    {
        // Column Count.
        AddSliderInt("Column Count", m_ColumnCount, 10, 500, v => m_ColumnCount = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        m_StepCount = 0;

        var shapeDef = new PhysicsShapeDefinition
        {
            isTrigger = true,
            triggerEvents = true,
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = new PhysicsMask { bitMask = TriggerLayer | DestroyLayer },
                contacts = new PhysicsMask { bitMask = VisitorLayer }
            },
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.gray3 }
        };

        // Ground Body.
        var groundBody = world.CreateBody();

        // Ground.
        {
            var gridSize = 3f;
            var gridArea = new Vector2(gridSize, gridSize);

            var y = 0.0f;
            var groundCount = (m_ColumnCount * ColumnSpacing) / gridSize;
            var x = (groundCount / 2f) * -gridSize;
            for (var i = 0; i < groundCount; ++i)
            {
                var box = PolygonGeometry.CreateBox(
                    size: gridArea,
                    radius: 0f,
                    transform: new PhysicsTransform(new Vector2(x, y)));

                groundBody.CreateShape(box, shapeDef);
                x += gridSize;
            }
        }

        // Triggers.
        {
            var xCenter = 0.5f * ColumnSpacing * m_ColumnCount;

            shapeDef.contactFilter = new PhysicsShape.ContactFilter
            {
                categories = new PhysicsMask { bitMask = TriggerLayer },
                contacts = new PhysicsMask { bitMask = VisitorLayer }
            };

            const float yStart = 10.0f;

            for (var j = 0; j < RowCount; ++j)
            {
                var y = j * 5f + yStart;
                for (var i = 0; i < m_ColumnCount; ++i)
                {
                    var x = i * ColumnSpacing - xCenter;
                    var yOffset = Random.NextFloat(-1.0f, 1.0f);

                    var box = PolygonGeometry.CreateBox(
                        size: Vector2.one,
                        radius: 0.1f,
                        transform: new PhysicsTransform(new Vector2(x, y + yOffset), new PhysicsRotate(Random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI))));

                    groundBody.CreateShape(box, shapeDef);
                }
            }
        }
    }

    private void CreateRow(float y)
    {
        // Get the default world.
        var world = World;

        var xCenter = 0.5f * ColumnSpacing * m_ColumnCount;

        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            gravityScale = 0f,
            linearVelocity = new Vector2(0f, -7.5f)
        };

        var shapeDef = new PhysicsShapeDefinition
        {
            triggerEvents = true,
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = new PhysicsMask { bitMask = VisitorLayer },
                contacts = new PhysicsMask { bitMask = DestroyLayer | TriggerLayer }
            },
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_DefaultFallingColor }
        };

        var circle = new CircleGeometry { radius = 0.5f };

        for (var i = 0; i < m_ColumnCount; ++i)
        {
            bodyDef.position = new Vector2(ColumnSpacing * i - xCenter, y);
            var body = world.CreateBody(bodyDef);
            body.CreateShape(circle, shapeDef);
        }
    }

    private void OnPostSimulation(PhysicsWorld world, float timeStep)
    {
        // Finish if not the sandbox world.
        if (!world.isDefaultWorld)
            return;

        // Spawn.
        if (++m_StepCount > SpawnPeriod)
        {
            m_StepCount = 0;
            CreateRow(10f + RowCount * 5f);
        }

        // Events.
        {
            // Begin.
            foreach (var beginEvent in world.triggerBeginEvents)
            {
                var shape = beginEvent.visitorShape;
                if (!shape.isValid)
                    continue;

                var triggerShape = beginEvent.triggerShape;

                if ((triggerShape.contactFilter.categories & DestroyLayer) != 0)
                {
                    shape.body.Destroy();
                    continue;
                }

                var surfaceMaterial = shape.surfaceMaterial;
                surfaceMaterial.customColor = Color.limeGreen;
                shape.surfaceMaterial = surfaceMaterial;
            }

            // End.
            foreach (var endEvent in world.triggerEndEvents)
            {
                var shape = endEvent.visitorShape;
                if (!shape.isValid)
                    continue;

                var surfaceMaterial = shape.surfaceMaterial;
                surfaceMaterial.customColor = m_DefaultFallingColor;
                shape.surfaceMaterial = surfaceMaterial;
            }
        }
    }
}
