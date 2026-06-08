using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Joints", "Demonstrating some fun contraptions using joints.")]
public sealed class Doohickey : SandboxExampleBehaviour
{
    private int m_DoohickeyCount;

    protected override float CameraSize => 9f;
    protected override Vector2 CameraPosition => new(0f, 7f);

    protected override void OnExampleEnable()
    {
        // Set Overrides.
        SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.AllJoints);
        SandboxManager.SetOverrideColorShapeState(true);

        m_DoohickeyCount = 5;
    }

    protected override void SetupOptions()
    {
        // Doohickey Count.
        AddSliderInt("Doohickey Count", m_DoohickeyCount, 1, 10, v => m_DoohickeyCount = v, rebuild: true);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();

            groundBody.CreateShape(new SegmentGeometry { point1 = Vector2.left * 20f, point2 = Vector2.right * 20f });
            groundBody.CreateShape(PolygonGeometry.CreateBox(Vector2.one * 2f, 0.1f, new PhysicsTransform(Vector2.up), true));
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 50f), 0f, new PhysicsTransform(Vector2.left * 8f + Vector2.up * 25f)));
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 50f), 0f, new PhysicsTransform(Vector2.right * 8f + Vector2.up * 25f)));
        }

        // Doohickey.
        {
            var y = 4f;
            for (var n = 0; n < m_DoohickeyCount; ++n, y += 1.2f)
            {
                using var dumbbell = DoohickeyFactory.SpawnDumbbell(world, SandboxManager, new Vector2(0f, y), 0.5f);
            }
        }
    }
}
