using Unity.Mathematics;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif

[ExampleScene("Shapes", "A delicate balance of forces.")]
public sealed class CardHouse : SandboxExampleBehaviour
{
    protected override float CameraSize => 1f;
    protected override Vector2 CameraPosition => new(0.7f, 0.9f);

    protected override void OnExampleEnable()
    {
        SandboxManager.SetOverrideColorShapeState(true);
    }

    protected override void SetupScene()
    {
        var world = World;

        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.7f } };

        // Ground.
        {
            var groundBody = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(0f, -2f) });
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(80f, 4f)), shapeDef);
        }

        // Cards.
        {
            var cardHeight = 0.2f;
            var cardThickness = 0.001f;

            var angle0 = math.radians(25.0f);
            var angle1 = math.radians(-25.0f);
            var angle2 = 0.5f * PhysicsMath.PI;

            var cardBox = PolygonGeometry.CreateBox(new Vector2(cardThickness, cardHeight) * 2f);
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };

            var nb = 5;
            var z0 = 0.0f;
            var y = cardHeight - 0.02f;
            while (nb > 0)
            {
                var z = z0;
                for (var i = 0; i < nb; i++)
                {
                    if (i != nb - 1)
                    {
                        bodyDef.position = new Vector2(z + 0.25f, y + cardHeight - 0.015f);
                        bodyDef.rotation = new PhysicsRotate(angle2);
                        var body = world.CreateBody(bodyDef);

                        shapeDef.surfaceMaterial.customColor = ShapeColor;
                        body.CreateShape(cardBox, shapeDef);
                    }

                    {
                        bodyDef.position = new Vector2(z, y);
                        bodyDef.rotation = new PhysicsRotate(angle1);
                        var body = world.CreateBody(bodyDef);

                        shapeDef.surfaceMaterial.customColor = ShapeColor;
                        body.CreateShape(cardBox, shapeDef);
                    }

                    z += 0.175f;

                    {
                        bodyDef.position = new Vector2(z, y);
                        bodyDef.rotation = new PhysicsRotate(angle0);
                        var body = world.CreateBody(bodyDef);

                        shapeDef.surfaceMaterial.customColor = ShapeColor;
                        body.CreateShape(cardBox, shapeDef);
                    }

                    z += 0.175f;
                }

                y += cardHeight * 2.0f - 0.03f;
                z0 += 0.175f;
                nb--;
            }
        }
    }
}
