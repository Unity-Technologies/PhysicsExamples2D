using System;
using UnityEngine;
using Unity.U2D.Physics;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Benchmarks", "A kinematic pusher sweeping through thousands of small polygons in a walled yard.",
    Purpose = "Thousands of small pentagons dropped into a walled yard made of box shapes, while a kinematic block sweeps from side to side.\nIt stress-tests a large pile of bodies being continually pushed and reshuffled.",
    Controls = "Object Type: circles, capsules, pentagons or a mix of all three.\nColumn Count and Row Count: how many objects are dropped.\nPusher Height and Pusher Speed: how tall the block is and how fast it sweeps from side to side.\nGravity Scale: how hard gravity pulls.\nThe object type, counts and pusher height rebuild the scene.")]
public sealed class Junkyard : SandboxExampleBehaviour
{
    private enum ObjectType
    {
        Circle,
        Capsule,
        Polygon,
        Mix
    }

    // How far the pusher travels from the middle of the yard in each direction.
    private const float PusherRange = 60f;

    // How far from the middle of the yard the outermost columns are placed, which keeps them clear of the walls.
    private const float ColumnSpread = 79f;

    private Vector2 m_OldGravity;
    private float m_GravityScale;
    private ObjectType m_ObjectType;
    private int m_ColumnCount;
    private int m_RowCount;
    private float m_PusherHeight;
    private float m_PusherSpeed;

    private PhysicsBody m_PusherBody;
    private float m_PusherTime;

    protected override float CameraSize => 60f;
    protected override Vector2 CameraPosition => new(8f, 35f);

    protected override void OnExampleEnable()
    {
        // Use the physics world's state colors so shapes using continuous collision show up.
        SandboxManager.SetOverrideColorShapeState(true);

        // Register to move the pusher before each simulation step.
        PhysicsEvents.PreSimulate += MovePusher;

        m_OldGravity = World.gravity;
        m_GravityScale = 1f;
        m_ObjectType = ObjectType.Circle;
        m_PusherHeight = 8f;
        m_ColumnCount = 100;
        m_RowCount = 40;
        m_PusherSpeed = 1f;
    }

    protected override void OnExampleDisable()
    {
        // Unregister the pusher movement.
        PhysicsEvents.PreSimulate -= MovePusher;

        // Put the world's own gravity back.
        var world = World;
        world.gravity = m_OldGravity;
    }

    protected override void SetupOptions()
    {
        // Column Count.
        AddSliderInt("Column Count", m_ColumnCount, 10, 200, v => m_ColumnCount = v, rebuild: true);

        // Row Count.
        AddSliderInt("Row Count", m_RowCount, 1, 40, v => m_RowCount = v, rebuild: true);

        // Object Type.
        AddEnum("Object Type", m_ObjectType, v => m_ObjectType = v, rebuild: true);

        // Pusher Height.
        AddSlider("Pusher Height", m_PusherHeight, 8f, 40f, v => m_PusherHeight = v, rebuild: true);

        // Pusher Speed.
        AddSlider("Pusher Speed", m_PusherSpeed, 0f, 5f, v => m_PusherSpeed = v);

        // Gravity Scale.
        AddSlider("Gravity Scale", m_GravityScale, 0f, 5f, v =>
        {
            m_GravityScale = v;

            var world = World;
            world.gravity = m_OldGravity * m_GravityScale;
        });
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        m_PusherTime = 0f;

        // Yard: a floor and a roof with a wall at each end, all built from overlapping boxes and created together as one shape batch.
        {
            const float gridSize = 1f;
            const int spanCount = 161;
            const int wallCount = 50;

            var boxes = new PolygonGeometry[spanCount * 2 + wallCount * 2];
            var boxCount = 0;

            // Floor and roof, level with the top of the walls.
            for (var i = 0; i < spanCount; ++i)
            {
                var x = (i - 80f) * gridSize;
                boxes[boxCount++] = PolygonGeometry.CreateBox(new Vector2(1.1f * gridSize, gridSize), 0f, new PhysicsTransform(new Vector2(x, 0f), PhysicsRotate.identity));
                boxes[boxCount++] = PolygonGeometry.CreateBox(new Vector2(1.1f * gridSize, gridSize), 0f, new PhysicsTransform(new Vector2(x, (wallCount + 1) * gridSize), PhysicsRotate.identity));
            }

            for (var side = -1; side <= 1; side += 2)
            {
                for (var i = 0; i < wallCount; ++i)
                    boxes[boxCount++] = PolygonGeometry.CreateBox(new Vector2(gridSize, 1.1f * gridSize), 0f, new PhysicsTransform(new Vector2(80f * gridSize * side, (i + 1) * gridSize), PhysicsRotate.identity));
            }

            var groundBody = world.CreateBody();
            using var groundShapes = groundBody.CreateShapeBatch(boxes, new PhysicsShapeDefinition());
        }

        // Pentagons.
        {
            const float radius = 0.25f;

            // Five points spread around a circle by the golden angle, which the polygon creation wraps in a convex hull.
            var phi = Mathf.PI * (Mathf.Sqrt(5f) - 1f);
            var points = new Vector2[5];
            for (var i = 0; i < points.Length; ++i)
            {
                var theta = phi * i;
                points[i] = new Vector2(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta));
            }

            var polygon = PolygonGeometry.Create(points.AsSpan(), 0f, PhysicsTransform.identity);
            var circle = new CircleGeometry { center = Vector2.zero, radius = radius };
            var capsule = new CapsuleGeometry { center1 = new Vector2(-0.15f, 0f), center2 = new Vector2(0.15f, 0f), radius = 0.1f };

            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
            var shapeDef = new PhysicsShapeDefinition();
            shapeDef.surfaceMaterial.customColor = ShapeColor;

            // Alternate each body a little to the left and right so the columns do not stack perfectly.
            var side = -0.1f;
            const float yStart = 10f;
            var bodyCount = 0;

            for (var i = 0; i < m_ColumnCount; ++i)
            {
                var x = m_ColumnCount > 1 ? Mathf.Lerp(-ColumnSpread, ColumnSpread, i / (m_ColumnCount - 1f)) : 0f;

                for (var j = 0; j < m_RowCount; ++j)
                {
                    bodyDef.position = new Vector2(x + side, 4f * j * radius + yStart);
                    side = -side;

                    var body = world.CreateBody(bodyDef);

                    // A mix cycles through the three shape types body by body.
                    var objectType = m_ObjectType == ObjectType.Mix ? (ObjectType)(bodyCount++ % 3) : m_ObjectType;
                    switch (objectType)
                    {
                        case ObjectType.Circle:
                            body.CreateShape(circle, shapeDef);
                            break;

                        case ObjectType.Capsule:
                            body.CreateShape(capsule, shapeDef);
                            break;

                        default:
                            body.CreateShape(polygon, shapeDef);
                            break;
                    }
                }
            }
        }

        // Pusher.
        {
            var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic };
            m_PusherBody = world.CreateBody(bodyDef);

            var box = PolygonGeometry.CreateBox(new Vector2(4f, m_PusherHeight), 0f, new PhysicsTransform(new Vector2(0f, 0.5f * m_PusherHeight), PhysicsRotate.identity));
            var shapeDef = new PhysicsShapeDefinition();
            shapeDef.surfaceMaterial.customColor = ShapeColor;
            m_PusherBody.CreateShape(box, shapeDef);
        }
    }

    // Gives the pusher the velocity that carries it to where it should be at the end of this step, so it keeps pushing the pile as it moves.
    private void MovePusher(PhysicsWorld world, float deltaTime)
    {
        if (world != PhysicsWorld.defaultWorld || !m_PusherBody.isValid || deltaTime <= 0f)
            return;

        m_PusherTime += deltaTime;

        var targetX = PusherRange * Mathf.Sin(m_PusherSpeed * m_PusherTime);
        m_PusherBody.linearVelocity = new Vector2((targetX - m_PusherBody.position.x) / deltaTime, 0f);
    }
}
