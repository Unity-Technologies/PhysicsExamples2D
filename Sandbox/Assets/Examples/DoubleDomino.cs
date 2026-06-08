using UnityEngine;
using Unity.U2D.Physics;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrating accuracy by simulating the classic double-domino wave.")]
public sealed class DoubleDomino : SandboxExampleBehaviour
{
    private const int DominoCount = 20;

    protected override float CameraSize => 9f;
    protected override Vector2 CameraPosition => new(0f, 0f);

    protected override void SetupScene()
    {
        // Create the domino shelves.
        for (var n = 0; n < 5; ++n)
            CreateDominoShelf((n % 2) == 0, 2.5f - n * 2f);
    }

    private void CreateDominoShelf(bool tipRight, float positionY)
    {
        var world = World;

        {
            var bodyDef = new PhysicsBodyDefinition { position = new Vector2(0f, positionY) };
            var body = world.CreateBody(bodyDef);
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.dimGray } };
            body.CreateShape(PolygonGeometry.CreateBox(new Vector2(200f, 0.5f)), shapeDef);
        }

        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.6f } };
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(0.25f, 1f));

            var x = -0.5f * DominoCount;
            for (var i = 0; i < DominoCount; ++i, x += 1f)
            {
                bodyDef.position = new Vector2(x, positionY + 0.75f);
                var dominoBody = world.CreateBody(bodyDef);

                // Fetch the appropriate shape color.
                shapeDef.surfaceMaterial.customColor = ShapeColor;

                dominoBody.CreateShape(boxGeometry, shapeDef);

                if (tipRight && i == 0)
                    dominoBody.ApplyLinearImpulse(new Vector2(0.2f, 0f), new Vector2(x, positionY + 1f));
                else if (!tipRight && i == (DominoCount - 1))
                    dominoBody.ApplyLinearImpulse(new Vector2(-0.2f, 0f), new Vector2(x, positionY + 1f));
            }
        }
    }
}
