using Unity.U2D.Physics;
using Unity.U2D.Physics.Assets;
using UnityEngine;

/// <summary>
/// An out-of-view kill border: four large static trigger slabs framing the camera view.
/// Any bullet entering a slab is destroyed, catching every shot that misses.
/// </summary>
/// <remarks>
/// The slabs are built from the camera bounds at startup so any aspect ratio is framed correctly.
/// They sit outside the enemy spawn ellipse and their contact filter matches bullets only, so inbound enemies cross them freely.
/// </remarks>
public sealed class KillBorder : MonoBehaviour, PhysicsAreaCallbacks.ITriggerCallback
{
    /// <summary>
    /// Destroys any bullet that enters a border slab.
    /// The slabs only match the Bullet category, so the visitor is always a bullet.
    /// </summary>
    public void OnTriggerBegin2D(PhysicsAreaCallbacks.TriggerBeginEvent beginEvent)
    {
        var visitor = beginEvent.visitorArea;

        if (visitor != null)
            Destroy(visitor.gameObject);
    }

    /// <summary>
    /// Trigger exits need no handling; the visitor was already destroyed on entry.
    /// </summary>
    public void OnTriggerEnd2D(PhysicsAreaCallbacks.TriggerEndEvent endEvent)
    {
    }

    void Start()
    {
        m_Camera = Camera.main;
        Rebuild();
    }

    void Update()
    {
        // Resolution or fullscreen changes move the view edges, so the frame rebuilds to match.
        if (Mathf.Approximately(m_Camera.aspect, m_BuiltAspect) && Mathf.Approximately(m_Camera.orthographicSize, m_BuiltSize))
            return;

        for (var i = transform.childCount - 1; i >= 0; --i)
            Destroy(transform.GetChild(i).gameObject);

        Rebuild();
    }

    void Rebuild()
    {
        m_BuiltAspect = m_Camera.aspect;
        m_BuiltSize = m_Camera.orthographicSize;

        var halfHeight = m_Camera.orthographicSize;
        var halfWidth = halfHeight * m_Camera.aspect;

        // Inner edge of the slabs, beyond the view and the enemy spawn ellipse.
        var innerX = halfWidth + m_Offset;
        var innerY = halfHeight + m_Offset;

        // Slab extents, stretched past the corners so the frame has no diagonal gaps.
        var spanX = 2f * (innerX + m_Thickness);
        var spanY = 2f * (innerY + m_Thickness);

        BuildSlab("Right", new Vector2(innerX + m_Thickness * 0.5f, 0f), new Vector2(m_Thickness, spanY));
        BuildSlab("Left", new Vector2(-innerX - m_Thickness * 0.5f, 0f), new Vector2(m_Thickness, spanY));
        BuildSlab("Top", new Vector2(0f, innerY + m_Thickness * 0.5f), new Vector2(spanX, m_Thickness));
        BuildSlab("Bottom", new Vector2(0f, -innerY - m_Thickness * 0.5f), new Vector2(spanX, m_Thickness));
    }

    void BuildSlab(string slabName, Vector2 center, Vector2 size)
    {
        var slab = new GameObject(slabName);
        slab.transform.SetParent(transform, false);
        slab.transform.position = center;
        slab.AddComponent<PhysicsPose>();

        var area = slab.AddComponent<PhysicsAreaPolygon>();
        area.geometry = PolygonGeometry.CreateBox(size, 0f, false);

        var definition = area.definition;
        definition.isTrigger = true;
        definition.triggerEvents = true;

        if (m_Filter != null)
            definition.contactFilter = m_Filter.value;

        area.definition = definition;
        area.callbackSource = PhysicsArea.CallbackSourceType.Area;
        area.callbackTarget = this;
        area.ApplyDefinition();
    }

    #region Internal

    // The shared contact filter for the KillBorder category (matches bullets only).
    [SerializeField] PhysicsContactFilter m_Filter;

    // Distance from the view edge to a slab's inner face, and the slab thickness.
    [SerializeField] float m_Offset = 3f;
    [SerializeField] float m_Thickness = 4f;

    // The camera dimensions the slabs were last built for.
    Camera m_Camera;
    float m_BuiltAspect;
    float m_BuiltSize;

    #endregion
}
