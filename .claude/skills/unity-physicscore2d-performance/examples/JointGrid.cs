using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Joint stress test: a `GridSize × GridSize` grid of circle bodies wired together with hinge joints (vertical
/// and horizontal neighbours). At default 32: 1024 bodies + ~1984 hinge joints. A 7-body strip in the middle
/// row is anchored Static so the cloth-like grid hangs from those anchor points.
/// </summary>
public class JointGrid : MonoBehaviour
{
    public int GridSize = 32;

    private PhysicsWorld.DrawOptions m_OldDrawOptions;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;

        // Optional: focus debug drawing on joints during the test.
        m_OldDrawOptions = world.drawOptions;
        world.drawOptions = PhysicsWorld.DrawOptions.AllJoints;

        SetupScene();
    }

    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        if (world.isValid)
            world.drawOptions = m_OldDrawOptions;
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        var hingeJointDef = PhysicsHingeJointDefinition.defaultDefinition;
        var gridShapeDefinition = PhysicsShapeDefinition.defaultDefinition;

        var gridScale = 150.0f / GridSize;
        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = 0.4f * gridScale };

        var offset = new Vector2(GridSize * -0.5f, GridSize * 0.5f);
        var index = 0;
        var bodyArray = new NativeArray<PhysicsBody>(GridSize * GridSize, Allocator.Temp);

        for (var k = 0; k < GridSize; ++k)
        {
            for (var i = 0; i < GridSize; ++i)
            {
                var fk = (float)k;
                var fi = (float)i;

                // Anchor a 7-body strip in the middle row as Static.
                bodyDef.type = (k >= GridSize / 2 - 3 && k <= GridSize / 2 + 3 && i == 0)
                    ? PhysicsBody.BodyType.Static
                    : PhysicsBody.BodyType.Dynamic;

                bodyDef.position = (new Vector2(fk, -fi) + offset) * gridScale;
                var body = world.CreateBody(bodyDef);

                body.CreateShape(circleGeometry, gridShapeDefinition);

                // Vertical neighbour joint.
                if (i > 0)
                {
                    hingeJointDef.bodyA = bodyArray[index - 1];
                    hingeJointDef.bodyB = body;
                    hingeJointDef.localAnchorA = new Vector2(0f, -0.5f) * gridScale;
                    hingeJointDef.localAnchorB = new Vector2(0f, 0.5f) * gridScale;
                    world.CreateJoint(hingeJointDef);
                }

                // Horizontal neighbour joint.
                if (k > 0)
                {
                    hingeJointDef.bodyA = bodyArray[index - GridSize];
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
