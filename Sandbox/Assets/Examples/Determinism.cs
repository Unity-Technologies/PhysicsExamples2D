using System;
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
//[ExampleScene("Collision", "Checking cross-platform determinism using a complex joint set-up. <b><color=#FF0000>NOTE: Cross-platform determinism is not supported yet!</color></b>")]
public sealed class Determinism : SandboxExampleBehaviour
{
    private Label m_SimulationStepsElement;
    private Label m_StartHashElement;
    private Label m_EndHashElement;

    private const int Columns = 8;
    private const int Rows = 40;

    private NativeList<PhysicsBody> m_Bodies;
    private const UInt32 BaseSceneHash = 5381;
    private UInt32 m_StartHash;
    private UInt32 m_EndHash;
    private UInt32 m_SimulationSteps;
    private bool m_EverythingAsleep;

    protected override float CameraSize => 15f;
    protected override Vector2 CameraPosition => new(0f, 10f);

    protected override void OnExampleEnable()
    {
        // Create bodies list.
        m_Bodies = new NativeList<PhysicsBody>(Allocator.Persistent);

        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        SandboxManager.SetOverrideColorShapeState(true);

        PhysicsEvents.PostSimulate += PostSimulate;
    }

    protected override void OnExampleDisable()
    {
        PhysicsEvents.PostSimulate -= PostSimulate;

        // Dispose of the bodies list.
        if (m_Bodies.IsCreated)
            m_Bodies.Dispose();
    }

    private void PostSimulate(PhysicsWorld world, float deltaTime)
    {
        if (m_EverythingAsleep || world != PhysicsWorld.defaultWorld)
            return;

        const int expectedBodyCount = Rows * Columns + 1;
        if (m_Bodies.Length != expectedBodyCount)
        {
            m_EndHashElement.text = $"Expected PhysicsBody Count Invalid!";
            return;
        }

        m_EverythingAsleep = world.bodyUpdateEvents.Length == 0;
        ++m_SimulationSteps;
        m_EndHash = CalculateTransformHash();
        UpdateHashStatus(m_EverythingAsleep);
    }

    protected override void SetupOptions()
    {
        // Read-only hash/step displays, written from the PostSimulate callback. The "hash-label"
        // USS class (SandboxStyleOverrides.uss) gives them their bordered box look.
        m_StartHashElement = AddElement(new Label("...") { name = "start-hash" });
        m_StartHashElement.AddToClassList("hash-label");

        m_EndHashElement = AddElement(new Label("...") { name = "end-hash" });
        m_EndHashElement.AddToClassList("hash-label");

        m_SimulationStepsElement = AddElement(new Label("...") { name = "simulation-steps" });
        m_SimulationStepsElement.AddToClassList("hash-label");
    }

    protected override void SetupScene()
    {
        // Clear the bodies.
        m_Bodies.Clear();

        m_StartHash = 0;
        m_EndHash = 0;
        m_SimulationSteps = 0;
        m_EverythingAsleep = false;

        // Get the default world.
        var world = World;

        // Ground.
        {
            var bodyDef = new PhysicsBodyDefinition { position = new Vector2(0f, -1f) };
            var groundBody = world.CreateBody(bodyDef);
            m_Bodies.Add(groundBody);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(32f, 2f)), PhysicsShapeDefinition.defaultDefinition);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 15f), radius: 0f, new PhysicsTransform(new Vector2(-15f, 6.5f), PhysicsRotate.identity)), PhysicsShapeDefinition.defaultDefinition);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 15f), radius: 0f, new PhysicsTransform(new Vector2(15f, 6.5f), PhysicsRotate.identity)), PhysicsShapeDefinition.defaultDefinition);
        }

        // Hinges.
        {
            const float height = 0.25f;
            const float radius = 0.1f * height;
            var box = PolygonGeometry.CreateBox(new Vector2(height - radius, height - radius), radius: radius);

            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.3f, customColor = ShapeColor } };

            const float offset = 0.4f * height;
            const float dx = 10.0f * height;
            var offsetX = -0.5f * dx * (Columns - 1.0f) - 4.5f;

            var jointDef = new PhysicsHingeJointDefinition
            {
                enableLimit = true,
                lowerAngleLimit = -0.1f * PhysicsMath.PI,
                upperAngleLimit = 0.2f * PhysicsMath.PI,
                enableSpring = true,
                springFrequency = 0.5f,
                springDamping = 0.5f,
                localAnchorA = new Vector2(height, height),
                localAnchorB = new Vector2(offset, -height)
            };

            for (var j = 0; j < Columns; ++j)
            {
                var x = offsetX + j * dx;

                var prevBody = new PhysicsBody();

                for (var i = 0; i < Rows; ++i)
                {
                    var rowIndex = (float)i;

                    var bodyDef = new PhysicsBodyDefinition
                    {
                        type = PhysicsBody.BodyType.Dynamic,
                        position = new Vector2(x + offset * i, height + 2f * height * i),
                        rotation = PhysicsRotate.FromRadians(0.1f * rowIndex - 1f) // This tests the deterministic cosine and sine functions
                    };

                    var body = world.CreateBody(bodyDef);
                    m_Bodies.Add(body);

                    if ((i & 1) == 0)
                    {
                        prevBody = body;
                    }
                    else
                    {
                        jointDef.bodyA = prevBody;
                        jointDef.bodyB = body;
                        world.CreateJoint(jointDef);
                        prevBody = new PhysicsBody();
                    }

                    shapeDef.surfaceMaterial.customColor = ShapeColor;
                    body.CreateShape(box, shapeDef);
                }
            }
        }

        m_StartHash = CalculateTransformHash();
        UpdateHashStatus(m_EverythingAsleep);
    }

    private void UpdateHashStatus(bool everythingAsleep)
    {
        var color = everythingAsleep ? "#7FFFD4" : "#999999";
        var colorTag = $"<color={color}>";
        const string endColorTag = "</color>";

        m_SimulationStepsElement.text = $"Simulation Steps: {colorTag}{m_SimulationSteps}{endColorTag}";

        m_StartHashElement.text = $"Start Hash: {colorTag}0x{m_StartHash:X8}{endColorTag}";
        m_EndHashElement.text = $"End Hash: {colorTag}0x{m_EndHash:X8}{endColorTag}";
    }

    private UInt32 CalculateTransformHash()
    {
        var hash = BaseSceneHash;
        foreach (var body in m_Bodies)
        {
            var bodyTransform = body.transform;
            hash = TransformHash(hash, ref bodyTransform);
        }

        return hash;
    }

    /// <summary>
    /// djb2 hash
    /// https://en.wikipedia.org/wiki/List_of_hash_functions
    /// </summary>
    private static unsafe UInt32 TransformHash(UInt32 hash, ref PhysicsTransform transform)
    {
        var transformSize = sizeof(PhysicsTransform);
        fixed (float* pX = &transform.position.x)
        {
            var data = (byte*)pX;
            for (var i = 0; i < transformSize; ++i)
            {
                hash = (hash << 5) + hash + data[i];
            }

            return hash;
        }
    }
}
