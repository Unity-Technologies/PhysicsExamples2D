using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Spins one kinematic body made of a large grid of separate box shapes, to show what a single body carrying many shapes costs to move.
/// The body is one Physics Pose carrying a Physics Area Polygon per box, so nothing here is a special case of a normal compound.
/// </summary>
/// <remarks>
/// A kinematic body is driven by its velocity alone and is not pushed back by anything it hits, so the grid turns at a steady rate however much it sweeps through.
/// </remarks>
public sealed class LargeKinematicContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current body and builds a new one at the current grid size and spacing.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        if (m_RotorPrefab == null)
            return;

        var spawned = Instantiate(m_RotorPrefab, Vector3.zero, Quaternion.identity);
        m_Rotor = spawned.GetComponent<PhysicsPose>();

        var step = BoxSize.x + m_GridSpacing;
        var span = m_GridSize / 2;
        var rotorTransform = spawned.transform;

        for (var row = -span; row < span; ++row)
        {
            var y = row * step;

            for (var column = -span; column < span; ++column)
            {
                // Each box is its own child rather than another component on the body, because a GameObject copies its whole component list every time one is added, which turns thousands of boxes into millions of copies.
                var box = new GameObject("Box");
                box.transform.SetParent(rotorTransform, false);
                box.transform.localPosition = new Vector3(column * step, y, 0f);

                // The area finds the body by searching up the hierarchy, so every box adds its shape to the one pose on the root.
                var area = box.AddComponent<PhysicsAreaPolygon>();
                area.geometry = PolygonGeometry.CreateBox(BoxSize);

                // Working out the mass as each box arrives would repeat that for every shape on the body, so it is left until they are all there.
                var definition = area.definition;
                definition.startMassUpdate = false;
                area.definition = definition;
            }
        }

        spawned.SetActive(true);

        var body = m_Rotor.body;

        if (body.isValid)
        {
            body.ApplyMassFromShapes();
            body.angularVelocity = m_AngularVelocity;
        }
    }

    /// <summary>
    /// Destroys the spinning body.
    /// </summary>
    public void Clear()
    {
        if (m_Rotor != null)
            Destroy(m_Rotor.gameObject);

        m_Rotor = null;
    }

    /// <summary>
    /// How many boxes the grid is across and down.
    /// Changing this does not rebuild the body until <see cref="Rebuild"/> is called.
    /// </summary>
    public int gridSize
    {
        get => m_GridSize;
        set => m_GridSize = value;
    }

    /// <summary>
    /// How far apart the boxes sit, in meters, on top of the one meter each of them already takes.
    /// Changing this does not rebuild the body until <see cref="Rebuild"/> is called.
    /// </summary>
    public float gridSpacing
    {
        get => m_GridSpacing;
        set => m_GridSpacing = value;
    }

    /// <summary>
    /// How fast the body turns, in degrees per second, with negative values turning it the other way.
    /// This takes effect on the body where it stands, since nothing about the grid depends on it.
    /// </summary>
    public float angularVelocity
    {
        get => m_AngularVelocity;
        set
        {
            m_AngularVelocity = value;

            if (m_Rotor == null)
                return;

            var body = m_Rotor.body;

            if (body.isValid)
                body.angularVelocity = m_AngularVelocity;
        }
    }

    private void Start() => Rebuild();

    #region Internal

    // One box of the grid.
    static readonly Vector2 BoxSize = new(1f, 1f);

    [SerializeField] GameObject m_RotorPrefab;
    [SerializeField, Range(10, 150)] int m_GridSize = 100;
    [SerializeField, Range(0f, 1f)] float m_GridSpacing;
    [SerializeField, Range(-360f, 360f)] float m_AngularVelocity = 90f;

    PhysicsPose m_Rotor;

    #endregion
}
