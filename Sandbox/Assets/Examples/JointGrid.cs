using Unity.Collections;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Benchmarks", "A large grid of bodies connected by Hinge Joints.")]
public sealed class JointGrid : SandboxExampleBehaviour
{
    private int m_GridSize;

    protected override float CameraSize => 100f;
    protected override Vector2 CameraPosition => new(-10f, -10f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.Off);

        m_GridSize = 32;
    }

    protected override void SetupOptions()
    {
        // Grid Size.
        AddSliderInt("Grid Size", m_GridSize, 10, 100, v => m_GridSize = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        var hingeJointDef = PhysicsHingeJointDefinition.defaultDefinition;
        var gridShapeDefinition = PhysicsShapeDefinition.defaultDefinition;

        var gridScale = 150.0f / m_GridSize;
        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = 0.4f * gridScale };

        var offset = new Vector2(m_GridSize * -0.5f, m_GridSize * 0.5f);
        var index = 0;
        var bodyArray = new NativeArray<PhysicsBody>(m_GridSize * m_GridSize, Allocator.Temp);
        for (var k = 0; k < m_GridSize; ++k)
        {
            for (var i = 0; i < m_GridSize; ++i)
            {
                var fk = (float)k;
                var fi = (float)i;

                if (k >= m_GridSize / 2 - 3 && k <= m_GridSize / 2 + 3 && i == 0)
                {
                    bodyDef.type = PhysicsBody.BodyType.Static;
                }
                else
                {
                    bodyDef.type = PhysicsBody.BodyType.Dynamic;
                }

                bodyDef.position = (new Vector2(fk, -fi) + offset) * gridScale;
                var body = world.CreateBody(bodyDef);

                gridShapeDefinition.surfaceMaterial.customColor = ShapeColor;
                body.CreateShape(circleGeometry, gridShapeDefinition);

                if (i > 0)
                {
                    hingeJointDef.bodyA = bodyArray[index - 1];
                    hingeJointDef.bodyB = body;
                    hingeJointDef.localAnchorA = new Vector2(0f, -0.5f) * gridScale;
                    hingeJointDef.localAnchorB = new Vector2(0f, 0.5f) * gridScale;
                    world.CreateJoint(hingeJointDef);
                }

                if (k > 0)
                {
                    hingeJointDef.bodyA = bodyArray[index - m_GridSize];
                    hingeJointDef.bodyB = body;
                    hingeJointDef.localAnchorA = new Vector2(0.5f, 0f) * gridScale;
                    hingeJointDef.localAnchorB = new Vector2(-0.5f, 0f) * gridScale;
                    world.CreateJoint(hingeJointDef);
                }

                bodyArray[index++] = body;
            }
        }

        bodyArray.Dispose();
    }
}
