using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Detonates enemies that reach the player, after each simulation step.
/// One overlap query with the player's circle answers "which enemies are touching"; each one explodes, costing the player a chunk of health.
/// The player has no contact events at all.
/// </summary>
public sealed class PlayerHitSystem : MonoBehaviour
{
    void Awake() => GameSettings.RegisterSlider(this, GameSettings.GameTab, "Damage Per Hit", 0f, 100f, () => m_DamagePerDetonation, v => m_DamagePerDetonation = v);

    void OnDestroy() => GameSettings.Unregister(this);

    void OnEnable()
    {
        // The query belongs to every category so the enemies' own contact masks never filter it out; it detects only enemies.
        m_Filter = new PhysicsQuery.QueryFilter
        {
            categories = PhysicsMask.All,
            hitCategories = PhysicsLayers.GetLayerMask("Enemy")
        };

        if (m_Feedback == null)
            m_Feedback = FindAnyObjectByType<PlayerHitFeedback>();

        PhysicsEvents.PostSimulate += OnPostSimulate;
    }

    void OnDisable() => PhysicsEvents.PostSimulate -= OnPostSimulate;

    // Queries the freshly stepped world and detonates every enemy touching the player.
    // Detonation keeps running after game over so parked enemies never sit against the dead player; only the damage and hit feedback stop.
    void OnPostSimulate(PhysicsWorld world, float deltaTime)
    {
        if (!world.isDefaultWorld || m_Game == null)
            return;

        // The player sits at the origin with no rotation, so its local geometry is already world-space.
        using var hits = world.OverlapGeometry(m_PlayerArea.activeGeometry, m_Filter, Allocator.Temp);

        var anyDamage = false;

        for (var i = 0; i < hits.Length; ++i)
        {
            var enemy = Enemy.FromShape(hits[i].shape);

            if (enemy == null)
                continue;

            enemy.Detonate();

            if (!m_Game.isGameOver)
            {
                m_Game.Damage(m_DamagePerDetonation);
                anyDamage = true;
            }
        }

        // One feedback burst per step no matter how many enemies landed together; the killing blow still gets it.
        if (anyDamage && m_Feedback != null)
            m_Feedback.OnHit();
    }

    #region Internal

    // The player's circle area, queried in place each step.
    // The circle sits inset within the drawn hex (touching its edge midpoints), so hits never register past the visible rim.
    [SerializeField] PhysicsAreaCircle m_PlayerArea;

    [SerializeField] GameManager m_Game;

    // The player-hit emphasis (sound, shake, flicker); found on the player when not assigned.
    [SerializeField] PlayerHitFeedback m_Feedback;

    // Health the player loses for each enemy that detonates against it.
    [SerializeField] float m_DamagePerDetonation = 10f;

    PhysicsQuery.QueryFilter m_Filter;

    #endregion
}
