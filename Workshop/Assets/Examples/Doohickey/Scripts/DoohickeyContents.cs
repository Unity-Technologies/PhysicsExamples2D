using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Drops a stack of doohickeys between two tall walls, each one a pair of wheels on a sprung, sliding bar that tumbles and rolls as it lands on the others.
/// Every doohickey is one prefab of Physics Pose and Physics Area components held together by Physics Constraint Hinge and Physics Constraint Slider components.
/// </summary>
public sealed class DoohickeyContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the current doohickeys and stacks new ones at the current count.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        if (m_DoohickeyPrefab == null)
            return;

        // Each doohickey starts a little higher than the one before, so they fall onto one another rather than overlapping.
        for (var n = 0; n < m_DoohickeyCount; ++n)
            m_Spawned.Add(Instantiate(m_DoohickeyPrefab, new Vector3(0f, FirstHeight + n * HeightStep, 0f), Quaternion.identity));
    }

    /// <summary>
    /// Destroys every doohickey, leaving the ground and walls alone.
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
    /// How many doohickeys are stacked.
    /// Changing this does not restack them until <see cref="Rebuild"/> is called.
    /// </summary>
    public int doohickeyCount
    {
        get => m_DoohickeyCount;
        set => m_DoohickeyCount = value;
    }

    private void Start() => Rebuild();

    #region Internal

    // The height the lowest doohickey starts at, and how much higher each one above it starts.
    const float FirstHeight = 4f;
    const float HeightStep = 1.2f;

    [SerializeField] GameObject m_DoohickeyPrefab;
    [SerializeField, Range(1, 10)] int m_DoohickeyCount = 5;

    readonly List<GameObject> m_Spawned = new();

    #endregion
}
