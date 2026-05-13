using System;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Bounciness sweep: 40 dynamic objects drop onto a flat ground; each one has a higher `surfaceMaterial.bounciness`
/// (0 → 1) so you can compare restitution behaviour side by side. Rotation is constrained so they stay upright.
/// </summary>
public class Bounciness : MonoBehaviour
{
    public enum ObjectType { Circle = 0, Capsule = 1, Box = 2 }

    public ObjectType Type = ObjectType.Capsule;

    private Vector2 m_OldGravity;

    private void OnEnable()
    {
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        SetupScene();
    }

    private void OnDisable()
    {
        PhysicsWorld.defaultWorld.gravity = m_OldGravity;
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

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

                switch (Type)
                {
                    case ObjectType.Circle:
                        body.CreateShape(circleGeometry, shapeDef);
                        break;
                    case ObjectType.Capsule:
                        body.CreateShape(capsuleGeometry, shapeDef);
                        break;
                    case ObjectType.Box:
                        body.CreateShape(boxGeometry, shapeDef);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                shapeDef.surfaceMaterial.bounciness += dr;
                x += dx;
            }
        }
    }
}
