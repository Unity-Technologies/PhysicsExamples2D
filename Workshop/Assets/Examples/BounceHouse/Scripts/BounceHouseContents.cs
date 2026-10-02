using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Fires a single fast shape across a bumper-lined room and keeps it bouncing forever, to check that very fast continuous collision stays stable.
/// The room is a Physics Area Contour outline plus five Physics Area Circle bumpers; the bouncing shape comes from one of three prefabs, chosen by <see cref="objectType"/>.
/// </summary>
/// <remarks>
/// The shape carries no gravity and loses no energy on a bounce, so it keeps the same speed forever once launched; only its direction changes.
/// </remarks>
public sealed class BounceHouseContents : MonoBehaviour
{
    /// <summary>
    /// The shape the bouncing body uses.
    /// </summary>
    public enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Polygon = 2
    }

    /// <summary>
    /// Destroys the current bouncing shape and launches a new one of the current shape type, from the same starting point and velocity.
    /// </summary>
    public void Rebuild()
    {
        if (m_Spawned != null)
            Destroy(m_Spawned);

        var prefab = m_ObjectType switch
        {
            ObjectType.Circle => m_CirclePrefab,
            ObjectType.Capsule => m_CapsulePrefab,
            _ => m_PolygonPrefab
        };

        if (prefab == null)
            return;

        m_Spawned = Instantiate(prefab, StartPosition, Quaternion.identity);
    }

    /// <summary>
    /// The shape the bouncing body uses.
    /// Changing this does not launch a new shape until <see cref="Rebuild"/> is called.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    private void Start() => Rebuild();

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        var hitEvents = world.contactHitEvents;
        if (hitEvents.Length > 0)
            m_LastHitEvent = hitEvents[0];

        if (!m_LastHitEvent.shapeA.isValid)
            return;

        var hitPoint = m_LastHitEvent.point;
        world.DrawCircle(hitPoint, 0.25f, Color.orangeRed, DrawLifetime);
        world.DrawLine(hitPoint, hitPoint + m_LastHitEvent.normal, Color.cornsilk, DrawLifetime);
    }

    #region Internal

    // How long the last hit's debug drawing stays on screen, in seconds.
    const float DrawLifetime = 2f;

    static readonly Vector2 StartPosition = new(0f, 5f);

    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Polygon;

    GameObject m_Spawned;
    PhysicsEvents.ContactHitEvent m_LastHitEvent;

    #endregion
}
