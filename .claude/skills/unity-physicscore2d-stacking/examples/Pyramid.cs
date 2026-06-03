using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Large stable pyramid stack of dynamic boxes. `BaseCount` rows wide at the base, narrowing by one each row up.
/// Total body count is `BaseCount * (BaseCount + 1) / 2` — at the default 60, that's 1830 dynamic boxes. The
/// chamber walls plus floor catch any collapses. Useful for sleep/island convergence stress and structural load.
/// </summary>
public class Pyramid : MonoBehaviour
{
    public int BaseCount = 60;
    public float GravityScale = 1f;

    private Vector2 m_OldGravity;

    private void OnEnable()
    {
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        PhysicsWorld.defaultWorld.gravity = m_OldGravity * GravityScale;
        SetupScene();
    }

    private void OnDisable()
    {
        PhysicsWorld.defaultWorld.gravity = m_OldGravity;
    }

    private void SetupScene()
    {
        var world = PhysicsWorld.defaultWorld;

        // Ground + roof + walls — large enclosed chamber.
        {
            var groundBody = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(0f, -1f) });

            const float groundLength = 1000f;
            var defaultShape = PhysicsShapeDefinition.defaultDefinition;
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(groundLength, 2f)), defaultShape);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(groundLength, 2f), radius: 0f, new PhysicsTransform(new Vector2(0f, groundLength), PhysicsRotate.identity)), defaultShape);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, groundLength), radius: 0f, new PhysicsTransform(new Vector2(groundLength * -0.5f, groundLength * 0.5f), PhysicsRotate.identity)), defaultShape);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, groundLength), radius: 0f, new PhysicsTransform(new Vector2(groundLength * 0.5f, groundLength * 0.5f), PhysicsRotate.identity)), defaultShape);
        }

        // Pyramid: row i has BaseCount-i boxes, indexed from the bottom up.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = PhysicsShapeDefinition.defaultDefinition;

            const float halfHeight = 0.5f;
            const float radius = 0.05f;
            // Slight rounding on box corners helps stacking stability.
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(halfHeight - radius, halfHeight - radius) * 2f, radius);

            const float shift = 1.0f * halfHeight;

            for (var i = 0; i < BaseCount; ++i)
            {
                var y = (2.0f * i + 1.0f) * shift;

                for (var j = i; j < BaseCount; ++j)
                {
                    var x = (i + 1.0f) * shift + 2.0f * (j - i) * shift - halfHeight * BaseCount;

                    bodyDef.position = new Vector2(x, y);
                    var body = world.CreateBody(bodyDef);

                    body.CreateShape(boxGeometry, shapeDef);
                }
            }
        }
    }
}
