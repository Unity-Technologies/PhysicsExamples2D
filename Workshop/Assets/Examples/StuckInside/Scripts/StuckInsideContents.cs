using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Shows two tables and two ships, each one body carrying several shapes, and lets another body be dropped right inside each of them.
/// Every compound is one Physics Pose with a Physics Area Polygon per shape, and every intruder is instantiated from a prefab at its compound's pose.
/// </summary>
/// <remarks>
/// The first table and ship are made of shapes that only meet at their edges, where the second pair's shapes overlap each other, so the two can be compared with something dropped inside.
/// Each compound's bounds, the box around every shape on its body, are drawn every frame.
/// </remarks>
public sealed class StuckInsideContents : MonoBehaviour
{
    /// <summary>
    /// Drops one intruder inside each compound: a thin plank under each table top and a small circle inside each ship.
    /// </summary>
    public void Intrude()
    {
        SpawnIntruder(m_PlankPrefab, m_Table1);
        SpawnIntruder(m_PlankPrefab, m_Table2);
        SpawnIntruder(m_BallPrefab, m_Ship1);
        SpawnIntruder(m_BallPrefab, m_Ship2);
    }

    private void Update()
    {
        DrawBodyBounds(m_Table1, Color.red);
        DrawBodyBounds(m_Table2, Color.cyan);
        DrawBodyBounds(m_Ship1, Color.red);
        DrawBodyBounds(m_Ship2, Color.cyan);
    }

    // Creates one intruder where the compound's body currently is, turned the same way, so it lands among the compound's shapes wherever the compound has moved to.
    // The body is read rather than the transform, because the transform can trail the body by a step.
    private static void SpawnIntruder(GameObject prefab, PhysicsPose compound)
    {
        if (prefab == null || compound == null)
            return;

        var body = compound.body;

        if (!body.isValid)
            return;

        var position = body.position;
        var rotation = Quaternion.Euler(0f, 0f, body.rotation.degrees);
        var spawned = Instantiate(prefab, new Vector3(position.x, position.y, 0f), rotation);
        spawned.SetActive(true);
    }

    // Draws the box that contains every shape on the compound's body.
    private static void DrawBodyBounds(PhysicsPose compound, Color color)
    {
        if (compound == null)
            return;

        var body = compound.body;

        if (!body.isValid)
            return;

        var bounds = body.GetAABB();
        PhysicsWorld.defaultWorld.DrawBox(bounds.center, bounds.extents * 2f, 0f, color);
    }

    #region Internal

    [SerializeField] PhysicsPose m_Table1;
    [SerializeField] PhysicsPose m_Table2;
    [SerializeField] PhysicsPose m_Ship1;
    [SerializeField] PhysicsPose m_Ship2;
    [SerializeField] GameObject m_PlankPrefab;
    [SerializeField] GameObject m_BallPrefab;

    #endregion
}
