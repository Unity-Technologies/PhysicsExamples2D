using System;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

[ExampleScene("Shapes", "Demonstrating the effect of Bounciness on shapes.")]
public sealed class Bounciness : SandboxExampleBehaviour
{
    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Box = 2
    }

    private ObjectType m_ObjectType;
    private Vector2 m_OldGravity;
    private float m_GravityScale;

    protected override float CameraSize => 35f;
    protected override Vector2 CameraPosition => new(1f, 26f);

    protected override void OnExampleEnable()
    {
        m_ObjectType = ObjectType.Capsule;
        m_OldGravity = World.gravity;
        m_GravityScale = 1f;
    }

    protected override void OnExampleDisable()
    {
        var world = World;
        world.gravity = m_OldGravity;
    }

    protected override void SetupOptions()
    {
        // Object Type.
        AddEnum("Object Type", m_ObjectType, v => m_ObjectType = v, rebuild: true);

        // Gravity Scale.
        AddSlider("Gravity Scale", m_GravityScale, 1f, 10f, v =>
        {
            m_GravityScale = v;
            var world = World;
            world.gravity = m_OldGravity * m_GravityScale;
        }, rebuild: true);
    }

    protected override void SetupScene()
    {
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 1f), radius: 0f, new PhysicsTransform(new Vector2(0f, 3f), PhysicsRotate.identity)), shapeDef);
        }

        // Bounciness Objects.
        {
            var circleGeometry = new CircleGeometry { radius = 0.5f };
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(1f, 1f));
            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, 0.5f), center2 = new Vector2(0f, -0.5f), radius = 0.5f };

            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, constraints = PhysicsBody.BodyConstraints.Rotation };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { bounciness = 0f } };

            const int ShapeCount = 40;

            const float dr = 1.0f / (ShapeCount > 1 ? ShapeCount - 1 : 1);
            const float dx = 2.0f;
            var x = -1.0f * (ShapeCount - 1);

            for (var i = 0; i < ShapeCount; ++i)
            {
                bodyDef.position = new Vector2(x, 44.0f);

                var body = world.CreateBody(bodyDef);

                shapeDef.surfaceMaterial.customColor = ShapeColor;

                switch (m_ObjectType)
                {
                    case ObjectType.Circle:
                    {
                        body.CreateShape(circleGeometry, shapeDef);
                        break;
                    }
                    case ObjectType.Capsule:
                    {
                        body.CreateShape(capsuleGeometry, shapeDef);
                        break;
                    }

                    case ObjectType.Box:
                    {
                        body.CreateShape(boxGeometry, shapeDef);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                shapeDef.surfaceMaterial.bounciness += dr;
                x += dx;
            }
        }
    }
}
