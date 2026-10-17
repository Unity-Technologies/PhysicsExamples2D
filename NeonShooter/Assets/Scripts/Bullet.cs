using Unity.U2D.Physics;
using Unity.U2D.RadianceCascades;
using UnityEngine;

/// <summary>
/// A single fired projectile, in one of two movement modes.
/// DynamicBody flies as a dynamic body and hits via contact events; QueryCast is a static body swept by the
/// <see cref="BulletMovementSystem"/> with geometry casts, which is the cheaper and tunnel-proof baseline.
/// A maximum lifetime is the backstop in both modes.
/// </summary>
[RequireComponent(typeof(PhysicsPose))]
[RequireComponent(typeof(PhysicsAreaCircle))]
public sealed class Bullet : MonoBehaviour, PhysicsAreaCallbacks.IContactCallback
{
    /// <summary>
    /// How the bullet moves and detects hits.
    /// </summary>
    public enum MovementMode
    {
        /// <summary>
        /// A dynamic body with velocity; hits arrive as contact events.
        /// </summary>
        DynamicBody,

        /// <summary>
        /// A static body swept by the bullet movement system with geometry casts.
        /// </summary>
        QueryCast
    }

    /// <summary>
    /// The tint of the bullet sprite; the shooter sets this right after launch.
    /// </summary>
    public Color color
    {
        get => m_SpriteInstance != null ? m_SpriteInstance.color : Color.white;
        set
        {
            if (m_SpriteInstance != null)
                m_SpriteInstance.color = value;
        }
    }

    /// <summary>
    /// Launches the bullet with the given velocity using its configured movement mode.
    /// Call this after instantiating the bullet active.
    /// </summary>
    public void Launch(Vector2 velocity)
    {
        if (m_Mode == MovementMode.QueryCast)
        {
            var movement = BulletMovementSystem.instance;

            if (movement != null)
                m_MovementIndex = movement.Register(this, m_Pose.body, transform.position, velocity, m_Area.activeGeometry.radius);

            return;
        }

        var body = m_Pose.body;

        body.linearVelocity = velocity;
    }

    /// <summary>
    /// Applies a hit found by the bullet movement system's sweep.
    /// Sparks fire along the struck surface's normal, then the bullet is destroyed.
    /// </summary>
    public void OnQueryHit(Enemy enemy, Vector2 point, Vector2 normal)
    {
        if (m_Consumed)
            return;

        m_Consumed = true;

        ApplyHit(enemy, point, normal);
    }

    /// <summary>
    /// Damages the enemy this bullet touched, then destroys the bullet (DynamicBody mode).
    /// Contacts with anything else (there should be none, per the contact filter) still consume the bullet.
    /// </summary>
    public void OnContactBegin2D(PhysicsAreaCallbacks.ContactBeginEvent beginEvent)
    {
        // Two enemies touching in the same step deliver two events; only the first counts.
        if (m_Consumed)
            return;

        m_Consumed = true;

        var other = beginEvent.areaA == m_Area ? beginEvent.areaB : beginEvent.areaA;

        // The struck area may be the enemy's shield child, so resolve upward through the hierarchy.
        var enemy = other != null ? other.GetComponentInParent<Enemy>() : null;

        // No contact normal is resolved here, so sparks kick back against the bullet's travel instead.
        ApplyHit(enemy, transform.position, -transform.right);
    }

    /// <summary>
    /// Contact ends need no handling; the bullet died on contact begin.
    /// </summary>
    public void OnContactEnd2D(PhysicsAreaCallbacks.ContactEndEvent endEvent)
    {
    }

    /// <summary>
    /// Called by the bullet movement system when a swap-back removal moves this bullet's state to a new index.
    /// </summary>
    public void UpdateMovementIndex(int index) => m_MovementIndex = index;

    void Awake()
    {
        m_Pose = GetComponent<PhysicsPose>();
        m_Area = GetComponent<PhysicsAreaCircle>();
    }

    void OnEnable()
    {
        m_Lifetime = m_MaxLifetime;

        if (SpriteRenderSystem.instance != null)
            m_SpriteInstance = SpriteRenderSystem.instance.Add(transform, m_Sprite, m_SpriteScale, Color.white);
    }

    void OnDisable()
    {
        if (m_MovementIndex >= 0 && BulletMovementSystem.instance != null)
        {
            BulletMovementSystem.instance.Unregister(m_MovementIndex);
            m_MovementIndex = -1;
        }

        if (m_SpriteInstance != null && SpriteRenderSystem.instance != null)
            SpriteRenderSystem.instance.Remove(m_SpriteInstance);

        m_SpriteInstance = null;
    }

    void Update()
    {
        m_Lifetime -= Time.deltaTime;

        if (m_Lifetime <= 0f)
            Destroy(gameObject);
    }

    // Applies a resolved hit shared by both movement modes: sparks along the surface normal, drops the shield or damages the enemy, then destroys the bullet.
    // A null enemy still destroys the bullet; the caller has already marked it consumed.
    void ApplyHit(Enemy enemy, Vector2 point, Vector2 normal)
    {
        if (enemy != null)
        {
            // Read the shield state before the damage lands, since this hit may be the one that drops it.
            var shielded = enemy.isShielded;

            SpawnHitSpark(point, normal, enemy.color);

            if (shielded)
                enemy.RegisterShieldHit();

            if (GameAudio.instance != null)
            {
                if (shielded)
                    GameAudio.instance.PlayShieldHit();
                else
                    GameAudio.instance.PlayHit();
            }

            enemy.TakeDamage(m_Damage);
        }

        Destroy(gameObject);
    }

    // The spark cone emits along its +Z, so aim that along the given direction.
    void SpawnHitSpark(Vector2 position, Vector2 direction, Color color)
    {
        if (m_HitSparkPrefab == null)
            return;

        // A cast that starts already overlapping its target reports a zero normal; fall back to spraying back along the bullet's travel.
        if (direction.sqrMagnitude < 1e-6f)
            direction = -(Vector2)transform.right;

        var rotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, 0f), Vector3.forward);
        var spark = Instantiate(m_HitSparkPrefab, position, rotation);
        spark.gameObject.hideFlags = HideFlags.HideInHierarchy;

        var main = spark.main;
        main.startColor = color;

        ParticleSettings.Apply(spark);
    }

    #region Internal

    // How the bullet moves and detects hits.
    [SerializeField] MovementMode m_Mode = MovementMode.DynamicBody;

    // Damage dealt to the enemy this bullet hits.
    [SerializeField] float m_Damage = 1f;

    // Small spark burst spawned at the impact point, tinted with the hit enemy's wave color.
    [SerializeField] ParticleSystem m_HitSparkPrefab;

    // The maximum time the bullet can live, a backstop for anything the sweep culling or kill border misses.
    [SerializeField] float m_MaxLifetime = 5f;

    // The bullet sprite drawn through the instanced sprite system, and its world scale.
    [SerializeField] Sprite m_Sprite;
    [SerializeField] Vector2 m_SpriteScale = Vector2.one;

    SpriteRenderSystem.SpriteInstance m_SpriteInstance;
    PhysicsPose m_Pose;
    PhysicsAreaCircle m_Area;
    float m_Lifetime;
    int m_MovementIndex = -1;
    bool m_Consumed;

    #endregion
}
