using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Many-body stress test: a `RowCount × ColumnCount` grid of rotating "tumblers" (kinematic 4-wall containers),
/// each filled with capsule debris over time. At default 15×15 = 225 tumblers × 4 walls + 225 × `SpawnCount`
/// debris bodies, this exercises broad-phase + narrow-phase across many active islands.
/// </summary>
public class ManyBodies : MonoBehaviour
{
    public int RowCount = 15;
    public int ColumnCount = 15;
    public int SpawnCount = 10;
    public float AngularVelocity = 45f;

    private int m_CurrentSpawnCounter;
    private const float SpawnPeriod = 0.1f;
    private float m_SpawnTime;

    private void OnEnable()
    {
        SetupScene();
    }

    private void Update()
    {
        if (m_CurrentSpawnCounter >= SpawnCount)
            return;

        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0f)
            return;

        m_SpawnTime = SpawnPeriod;
        m_CurrentSpawnCounter++;

        SpawnDebris();
    }

    private void SetupScene()
    {
        m_CurrentSpawnCounter = 0;

        var world = PhysicsWorld.defaultWorld;

        // Tumblers: each is a kinematic body with a constant angular velocity and 4 thin walls.
        var x = -4.0f * ColumnCount;
        for (var i = 0; i < ColumnCount; ++i, x += 8f)
        {
            var y = -4.0f * RowCount;
            for (var j = 0; j < RowCount; ++j, y += 8f)
            {
                var position = new Vector2(x, y);
                var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic, position = position, angularVelocity = AngularVelocity };
                var body = world.CreateBody(bodyDef);

                var shapeDef = new PhysicsShapeDefinition { density = 50f };

                body.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 4f), 0f, new PhysicsTransform(new Vector2(2f, 0f), PhysicsRotate.identity)), shapeDef);
                body.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 4f), 0f, new PhysicsTransform(new Vector2(-2f, 0f), PhysicsRotate.identity)), shapeDef);
                body.CreateShape(PolygonGeometry.CreateBox(new Vector2(4f, 0.5f), 0f, new PhysicsTransform(new Vector2(0f, 2f), PhysicsRotate.identity)), shapeDef);
                body.CreateShape(PolygonGeometry.CreateBox(new Vector2(4f, 0.5f), 0f, new PhysicsTransform(new Vector2(0f, -2f), PhysicsRotate.identity)), shapeDef);
            }
        }
    }

    private void SpawnDebris()
    {
        var world = PhysicsWorld.defaultWorld;

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(-0.1f, 0.0f), center2 = new Vector2(0.1f, 0.0f), radius = 0.075f };

        var x = -4.0f * ColumnCount;
        for (var i = 0; i < ColumnCount; ++i, x += 8f)
        {
            var y = -4.0f * RowCount;
            for (var j = 0; j < RowCount; ++j, y += 8f)
            {
                bodyDef.position = new Vector2(x, y);
                var body = world.CreateBody(bodyDef);
                body.CreateShape(capsuleGeometry, shapeDef);
            }
        }
    }
}
