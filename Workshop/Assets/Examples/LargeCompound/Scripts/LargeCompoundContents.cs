using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Drops a block of large compound bodies into a container, where each body is a solid square of many separate box shapes rather than one large box.
/// Every body is one Physics Pose carrying a grid of Physics Area Polygon components, which is what makes it a compound.
/// </summary>
/// <remarks>
/// The same amount of material is always dropped: more splits means more bodies, each made of fewer shapes.
/// A body's mass is worked out once after all of its shapes exist, rather than again for every shape as it is added.
/// </remarks>
public sealed class LargeCompoundContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current bodies and drops a new set built from the current settings.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        if (m_BodyPrefab == null)
            return;

        var splits = m_CompoundSplits + 1;

        // How many boxes a single body is across, which is what is left of the block once it has been divided up.
        var span = m_CompoundSize / splits;

        if (span < 1)
            return;

        for (var row = 0; row < splits; ++row)
        {
            var bodyY = (m_CompoundSize + 1 + row * span) * GridSize;

            for (var column = 0; column < splits; ++column)
            {
                var bodyX = -0.5f * GridSize * splits * span + column * span * GridSize;

                Spawn(new Vector2(bodyX, bodyY), span);
            }
        }
    }

    /// <summary>
    /// Destroys every compound body, leaving the container alone.
    /// </summary>
    public void Clear()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
    }

    /// <summary>
    /// How many boxes across the whole block of material is, before it is divided into separate bodies.
    /// Changing this does not drop a new set until <see cref="Rebuild"/> is called.
    /// </summary>
    public int compoundSize
    {
        get => m_CompoundSize;
        set => m_CompoundSize = value;
    }

    /// <summary>
    /// How many times the block is divided, in each direction, into separate bodies.
    /// Changing this does not drop a new set until <see cref="Rebuild"/> is called.
    /// </summary>
    public int compoundSplits
    {
        get => m_CompoundSplits;
        set => m_CompoundSplits = value;
    }

    private void Start() => Rebuild();

    // Creates one compound body: a square of box shapes, all on the same pose.
    // The instance stays inactive while its shapes are added, so the body is built once with the whole set rather than rebuilt for each box in turn.
    private void Spawn(Vector2 position, int span)
    {
        var spawned = Instantiate(m_BodyPrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity);

        m_Spawned.Add(spawned);

        var bodyTransform = spawned.transform;

        for (var row = 0; row < span; ++row)
        {
            for (var column = 0; column < span; ++column)
            {
                // Each box is its own child rather than another component on the body, because a GameObject copies its whole component list every time one is added, which turns thousands of boxes into millions of copies.
                var box = new GameObject("Box");
                box.transform.SetParent(bodyTransform, false);
                box.transform.localPosition = new Vector3(column * GridSize, row * GridSize, 0f);

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

        var pose = spawned.GetComponent<PhysicsPose>();

        if (pose != null && pose.body.isValid)
            pose.body.ApplyMassFromShapes();
    }

    #region Internal

    // One box of the grid a compound body is built from.
    const float GridSize = 1f;

    static readonly Vector2 BoxSize = new(GridSize, GridSize);

    [SerializeField] GameObject m_BodyPrefab;
    [SerializeField, Range(50, 100)] int m_CompoundSize = 75;
    [SerializeField, Range(1, 20)] int m_CompoundSplits = 5;

    readonly List<GameObject> m_Spawned = new();

    #endregion
}
