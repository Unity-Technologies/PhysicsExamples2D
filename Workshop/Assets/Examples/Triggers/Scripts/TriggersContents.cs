using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Drops a new row of small circles every half second or so down through a dense field of trigger shapes, to stress-test how the simulation copes with thousands of trigger events every step.
/// Every trigger is one static Physics Pose with a Physics Area Polygon, and every circle is one Physics Pose with a Physics Area Circle, all instantiated from prefabs.
/// </summary>
/// <remarks>
/// A circle turns green while it is inside a trigger and returns to gray when it leaves.
/// The row of wide triggers along the bottom also removes any circle that touches it, so the number of circles in flight stays steady.
/// Changing the column count rebuilds the scene, which removes every circle and trigger and lays the field out again.
/// </remarks>
public sealed class TriggersContents : MonoBehaviour
{
    /// <summary>
    /// Removes every circle and trigger, then lays the field out again with the current column count.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        m_StepCount = 0;
        m_Random = new Random(RandomSeed);

        m_FieldRoot = new GameObject("Triggers").transform;
        m_FieldRoot.SetParent(transform);

        m_VisitorRoot = new GameObject("Visitors").transform;
        m_VisitorRoot.SetParent(transform);

        SpawnField();
    }

    /// <summary>
    /// Removes every circle and trigger.
    /// </summary>
    public void Clear()
    {
        if (m_FieldRoot != null)
            Destroy(m_FieldRoot.gameObject);

        if (m_VisitorRoot != null)
            Destroy(m_VisitorRoot.gameObject);

        m_FieldRoot = null;
        m_VisitorRoot = null;
    }

    /// <summary>
    /// How many columns of triggers the field has, and how many circles are in every row dropped through it.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int columnCount
    {
        get => m_ColumnCount;
        set => m_ColumnCount = value;
    }

    private void OnEnable() => PhysicsEvents.PostSimulate += OnPostSimulate;

    private void OnDisable() => PhysicsEvents.PostSimulate -= OnPostSimulate;

    private void Start() => Rebuild();

    // Lays out the row of wide triggers along the bottom that removes circles, then the many rows of scattered triggers above it that color them.
    private void SpawnField()
    {
        if (m_RemoverPrefab != null)
        {
            var gridSize = 3f;
            var groundCount = m_ColumnCount * ColumnSpacing / gridSize;
            var x = groundCount / 2f * -gridSize;

            for (var i = 0; i < groundCount; ++i)
            {
                Instantiate(m_RemoverPrefab, new Vector3(x, 0f, 0f), Quaternion.identity, m_FieldRoot);
                x += gridSize;
            }
        }

        if (m_TriggerPrefab == null)
            return;

        var xCenter = 0.5f * ColumnSpacing * m_ColumnCount;

        for (var j = 0; j < RowCount; ++j)
        {
            var y = j * 5f + RowStart;

            for (var i = 0; i < m_ColumnCount; ++i)
            {
                var x = i * ColumnSpacing - xCenter;
                var yOffset = m_Random.NextFloat(-1f, 1f);
                var rotation = Quaternion.Euler(0f, 0f, m_Random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI) * Mathf.Rad2Deg);

                Instantiate(m_TriggerPrefab, new Vector3(x, y + yOffset, 0f), rotation, m_FieldRoot);
            }
        }
    }

    // Drops a new row of circles at the top of the field once enough steps have passed, then colors or removes the circles that entered or left a trigger this step.
    private void OnPostSimulate(PhysicsWorld world, float timeStep)
    {
        if (!world.isDefaultWorld || m_VisitorRoot == null)
            return;

        if (++m_StepCount > SpawnPeriod)
        {
            m_StepCount = 0;
            SpawnRow(RowStart + RowCount * 5f);
        }

        foreach (var beginEvent in world.triggerBeginEvents)
        {
            var shape = beginEvent.visitorShape;
            if (!shape.isValid)
                continue;

            if ((beginEvent.triggerShape.contactFilter.categories & DestroyLayer) != 0)
            {
                RemoveVisitor(shape);
                continue;
            }

            SetColor(shape, EnteredColor);
        }

        foreach (var endEvent in world.triggerEndEvents)
        {
            var shape = endEvent.visitorShape;
            if (!shape.isValid)
                continue;

            SetColor(shape, VisitorColor);
        }
    }

    // Drops one row of circles across the width of the field, each one falling straight down.
    private void SpawnRow(float y)
    {
        if (m_VisitorPrefab == null)
            return;

        var xCenter = 0.5f * ColumnSpacing * m_ColumnCount;

        for (var i = 0; i < m_ColumnCount; ++i)
            Instantiate(m_VisitorPrefab, new Vector3(ColumnSpacing * i - xCenter, y, 0f), Quaternion.identity, m_VisitorRoot);
    }

    // Removes a circle by its Physics Pose, since a body a component created can only be removed by removing the component.
    private static void RemoveVisitor(PhysicsShape shape)
    {
        if (shape.body.owner is not PhysicsPose pose)
            return;

        var target = pose.gameObject;
        target.SetActive(false);
        Destroy(target);
    }

    // Sets the color a circle is drawn in.
    private static void SetColor(PhysicsShape shape, Color color)
    {
        var surfaceMaterial = shape.surfaceMaterial;
        surfaceMaterial.customColor = color;
        shape.surfaceMaterial = surfaceMaterial;
    }

    #region Internal

    // How many steps pass between one row of circles and the next, and how far apart the columns are.
    const int SpawnPeriod = 31;
    const float ColumnSpacing = 2.5f;

    // How many rows of scattered triggers there are, and the height of the lowest one.
    const int RowCount = 40;
    const float RowStart = 10f;

    // The contact category that marks a trigger as one that removes circles.
    const ulong DestroyLayer = 1 << 0;

    // The colors of a circle falling free, and of a circle inside a trigger.
    static readonly Color VisitorColor = Color.gray4;
    static readonly Color EnteredColor = Color.limeGreen;

    // The seed the trigger positions and rotations start from, so every rebuild lays out the same field.
    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_TriggerPrefab;
    [SerializeField] GameObject m_RemoverPrefab;
    [SerializeField] GameObject m_VisitorPrefab;
    [SerializeField, Range(10, 500)] int m_ColumnCount = 80;

    Transform m_FieldRoot;
    Transform m_VisitorRoot;
    Random m_Random;
    int m_StepCount;

    #endregion
}
