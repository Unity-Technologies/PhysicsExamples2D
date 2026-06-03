using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Five domino shelves stacked vertically; each shelf has 20 dominos. Alternating shelves tip in opposite
/// directions via `body.ApplyLinearImpulse` to the first or last domino of each row. Demonstrates classic
/// chain-reaction stability + initial-impulse triggering.
/// </summary>
public class DominoChain : MonoBehaviour
{
    public int DominosPerShelf = 20;
    public int ShelfCount = 5;

    private void OnEnable()
    {
        SetupScene();
    }

    private void SetupScene()
    {
        for (var n = 0; n < ShelfCount; ++n)
            CreateDominoShelf((n % 2) == 0, 2.5f - n * 2f);
    }

    private void CreateDominoShelf(bool tipRight, float positionY)
    {
        var world = PhysicsWorld.defaultWorld;

        // Static shelf.
        {
            var bodyDef = new PhysicsBodyDefinition { position = new Vector2(0f, positionY) };
            var body = world.CreateBody(bodyDef);
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.dimGray } };
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(200f, 0.5f)), shapeDef);
        }

        // Dominos.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.6f } };
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(0.25f, 1f));

            var x = -0.5f * DominosPerShelf;
            for (var i = 0; i < DominosPerShelf; ++i, x += 1f)
            {
                bodyDef.position = new Vector2(x, positionY + 0.75f);
                var dominoBody = world.CreateBody(bodyDef);

                dominoBody.CreateShape(boxGeometry, shapeDef);

                // Trigger the chain reaction: tap the first or last domino with a small impulse.
                if (tipRight && i == 0)
                    dominoBody.ApplyLinearImpulse(new Vector2(0.2f, 0f), new Vector2(x, positionY + 1f));
                else if (!tipRight && i == (DominosPerShelf - 1))
                    dominoBody.ApplyLinearImpulse(new Vector2(-0.2f, 0f), new Vector2(x, positionY + 1f));
            }
        }
    }
}
