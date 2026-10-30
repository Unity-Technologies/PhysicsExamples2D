using System;
using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Drops thousands of small shapes into a walled yard while a kinematic block sweeps from side to side, to stress-test a large pile of bodies being continually pushed and reshuffled.
/// The yard is the body of a static Physics Pose given hundreds of box shapes in one batch, and the block and every dropped shape is one Physics Pose with a Physics Area, instantiated from a prefab.
/// </summary>
/// <remarks>
/// Only the object type, the column and row counts and the pusher height rebuild the scene, while the pusher speed and gravity scale act on the running scene.
/// The world's own gravity is put back when the example unloads.
/// </remarks>
public sealed class JunkyardContents : MonoBehaviour
{
    /// <summary>
    /// The kind of shape dropped into the yard.
    /// </summary>
    public enum ObjectType
    {
        Circle,
        Capsule,
        Polygon,
        Mix
    }

    /// <summary>
    /// Destroys the pusher and every dropped shape, then builds them again with the current settings.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        SpawnPusher();
        SpawnObjects();
    }

    /// <summary>
    /// Destroys the pusher and every dropped shape, leaving the yard alone.
    /// </summary>
    public void Clear()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
        m_Pusher = null;
        m_PusherTime = 0f;
    }

    /// <summary>
    /// The kind of shape dropped into the yard, where a mix cycles through circles, capsules and pentagons one body at a time.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    /// <summary>
    /// How many columns of shapes are dropped, spread evenly across the width of the yard.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int columnCount
    {
        get => m_ColumnCount;
        set => m_ColumnCount = value;
    }

    /// <summary>
    /// How many shapes are stacked in each column.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int rowCount
    {
        get => m_RowCount;
        set => m_RowCount = value;
    }

    /// <summary>
    /// How tall the pusher block is, in meters.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float pusherHeight
    {
        get => m_PusherHeight;
        set => m_PusherHeight = value;
    }

    /// <summary>
    /// How fast the pusher block sweeps from side to side, where a higher value crosses the yard more quickly.
    /// Changing this speeds up or slows down the running pusher without rebuilding the scene.
    /// </summary>
    public float pusherSpeed
    {
        get => m_PusherSpeed;
        set => m_PusherSpeed = value;
    }

    /// <summary>
    /// How strong gravity is, as a multiple of the world's own gravity.
    /// Changing this scales gravity straight away without rebuilding the scene.
    /// </summary>
    public float gravityScale
    {
        get => m_GravityScale;
        set
        {
            m_GravityScale = value;

            var world = PhysicsWorld.defaultWorld;
            world.gravity = m_WorldGravity * m_GravityScale;
        }
    }

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;
        m_WorldGravity = world.gravity;
        world.gravity = m_WorldGravity * m_GravityScale;

        PhysicsEvents.PreSimulate += MovePusher;
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay scaled into whichever example is loaded next.
    private void OnDisable()
    {
        PhysicsEvents.PreSimulate -= MovePusher;

        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;
    }

    private void Start()
    {
        BuildYard();
        Rebuild();
    }

    // Gives the yard's static body its floor, roof and two walls, all as overlapping boxes created together as one batch.
    private void BuildYard()
    {
        if (m_Yard == null || !m_Yard.body.isValid)
            return;

        var boxes = new PolygonGeometry[SpanCount * 2 + WallCount * 2];
        var boxCount = 0;

        // The floor and the roof, which sits level with the top of the walls.
        for (var i = 0; i < SpanCount; ++i)
        {
            var x = i - HalfSpan;
            boxes[boxCount++] = PolygonGeometry.CreateBox(new Vector2(1.1f, 1f), 0f, new PhysicsTransform(new Vector2(x, 0f), PhysicsRotate.identity));
            boxes[boxCount++] = PolygonGeometry.CreateBox(new Vector2(1.1f, 1f), 0f, new PhysicsTransform(new Vector2(x, WallCount + 1f), PhysicsRotate.identity));
        }

        // The wall at each end.
        for (var side = -1; side <= 1; side += 2)
        {
            for (var i = 0; i < WallCount; ++i)
                boxes[boxCount++] = PolygonGeometry.CreateBox(new Vector2(1f, 1.1f), 0f, new PhysicsTransform(new Vector2(HalfSpan * side, i + 1f), PhysicsRotate.identity));
        }

        using var yardShapes = m_Yard.body.CreateShapeBatch(boxes, new PhysicsShapeDefinition());
    }

    // Creates the pusher block at the middle of the yard, left inactive until it is set up so its body is built with those settings.
    private void SpawnPusher()
    {
        if (m_PusherPrefab == null)
            return;

        var spawned = Instantiate(m_PusherPrefab, Vector3.zero, Quaternion.identity);
        m_Pusher = spawned.GetComponent<PhysicsPose>();

        var area = spawned.GetComponent<PhysicsAreaPolygon>();
        area.geometry = PolygonGeometry.CreateBox(new Vector2(4f, m_PusherHeight), 0f, new PhysicsTransform(new Vector2(0f, 0.5f * m_PusherHeight), PhysicsRotate.identity));

        spawned.SetActive(true);
        m_Spawned.Add(spawned);
    }

    // Drops the columns of shapes, spread evenly across the yard and stacked above the floor.
    private void SpawnObjects()
    {
        // Five points spread around a circle by the golden angle, which the polygon creation wraps in a convex hull.
        var phi = Mathf.PI * (Mathf.Sqrt(5f) - 1f);
        var points = new Vector2[5];
        for (var i = 0; i < points.Length; ++i)
        {
            var theta = phi * i;
            points[i] = new Vector2(ShapeRadius * Mathf.Cos(theta), ShapeRadius * Mathf.Sin(theta));
        }

        var polygon = PolygonGeometry.Create(points.AsSpan(), 0f, PhysicsTransform.identity);
        var circle = new CircleGeometry { center = Vector2.zero, radius = ShapeRadius };
        var capsule = new CapsuleGeometry { center1 = new Vector2(-0.15f, 0f), center2 = new Vector2(0.15f, 0f), radius = 0.1f };

        // Alternate each body a little to the left and right so the columns do not stack perfectly.
        var side = -0.1f;
        var bodyCount = 0;

        for (var i = 0; i < m_ColumnCount; ++i)
        {
            var x = m_ColumnCount > 1 ? Mathf.Lerp(-ColumnSpread, ColumnSpread, i / (m_ColumnCount - 1f)) : 0f;

            for (var j = 0; j < m_RowCount; ++j)
            {
                var position = new Vector3(x + side, 4f * j * ShapeRadius + FirstRowHeight, 0f);
                side = -side;

                // A mix cycles through the three shape types body by body.
                var objectType = m_ObjectType == ObjectType.Mix ? (ObjectType)(bodyCount++ % 3) : m_ObjectType;

                GameObject spawned;
                switch (objectType)
                {
                    case ObjectType.Circle:
                        spawned = Instantiate(m_CirclePrefab, position, Quaternion.identity);
                        spawned.GetComponent<PhysicsAreaCircle>().geometry = circle;
                        break;

                    case ObjectType.Capsule:
                        spawned = Instantiate(m_CapsulePrefab, position, Quaternion.identity);
                        spawned.GetComponent<PhysicsAreaCapsule>().geometry = capsule;
                        break;

                    default:
                        spawned = Instantiate(m_PolygonPrefab, position, Quaternion.identity);
                        spawned.GetComponent<PhysicsAreaPolygon>().geometry = polygon;
                        break;
                }

                spawned.SetActive(true);
                m_Spawned.Add(spawned);
            }
        }
    }

    // Gives the pusher the velocity that carries it to where it should be at the end of this step, so it keeps pushing the pile as it moves.
    private void MovePusher(PhysicsWorld world, float deltaTime)
    {
        if (world != PhysicsWorld.defaultWorld || m_Pusher == null || !m_Pusher.body.isValid || deltaTime <= 0f)
            return;

        m_PusherTime += deltaTime;

        var body = m_Pusher.body;
        var targetX = PusherRange * Mathf.Sin(m_PusherSpeed * m_PusherTime);
        body.linearVelocity = new Vector2((targetX - body.position.x) / deltaTime, 0f);
    }

    #region Internal

    // How many boxes make up the floor and the roof, and how far from the middle the walls stand.
    const int SpanCount = 161;
    const float HalfSpan = 80f;

    // How many boxes make up each wall, which is also the height of the yard in meters.
    const int WallCount = 50;

    // How far from the middle of the yard the outermost columns are placed, which keeps them clear of the walls.
    const float ColumnSpread = 79f;

    // How far the pusher travels from the middle of the yard in each direction.
    const float PusherRange = 60f;

    // The size of a dropped shape and the height of the lowest row above the origin.
    const float ShapeRadius = 0.25f;
    const float FirstRowHeight = 10f;

    [SerializeField] PhysicsPose m_Yard;
    [SerializeField] GameObject m_PusherPrefab;
    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Circle;
    [SerializeField, Range(10, 200)] int m_ColumnCount = 100;
    [SerializeField, Range(1, 40)] int m_RowCount = 40;
    [SerializeField, Range(8f, 40f)] float m_PusherHeight = 8f;
    [SerializeField, Range(0f, 5f)] float m_PusherSpeed = 1f;
    [SerializeField, Range(0f, 5f)] float m_GravityScale = 1f;

    readonly List<GameObject> m_Spawned = new();
    PhysicsPose m_Pusher;
    float m_PusherTime;
    Vector2 m_WorldGravity;

    #endregion
}
