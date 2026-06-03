using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// 5-story card house — extremely thin rectangles balanced at angles, stacked in tiers with horizontal cards
/// connecting each tier to the next. Demonstrates stable stacking of thin shapes; sensitive to friction
/// (`shapeDef.surfaceMaterial.friction` is high here at 0.7) and to small-iteration solver settings.
/// </summary>
public class CardHouse : MonoBehaviour
{
    private void OnEnable()
    {
        SetupScene();
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.7f } };

        // Ground.
        {
            var groundBody = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(0f, -2f) });
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(80f, 4f)), shapeDef);
        }

        // Cards.
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
                // Horizontal connector card on top of every pair, except the last position in the row.
                if (i != nb - 1)
                {
                    bodyDef.position = new Vector2(z + 0.25f, y + cardHeight - 0.015f);
                    bodyDef.rotation = PhysicsRotate.FromRadians(angle2);
                    var body = world.CreateBody(bodyDef);
                    body.CreateShape(cardBox, shapeDef);
                }

                // Tilted card leaning right.
                {
                    bodyDef.position = new Vector2(z, y);
                    bodyDef.rotation = PhysicsRotate.FromRadians(angle1);
                    var body = world.CreateBody(bodyDef);
                    body.CreateShape(cardBox, shapeDef);
                }

                z += 0.175f;

                // Tilted card leaning left.
                {
                    bodyDef.position = new Vector2(z, y);
                    bodyDef.rotation = PhysicsRotate.FromRadians(angle0);
                    var body = world.CreateBody(bodyDef);
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
