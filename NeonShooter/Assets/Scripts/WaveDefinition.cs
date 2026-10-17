using UnityEngine;

/// <summary>
/// Describes one enemy wave: which enemy to spawn, how many, how fast they arrive, and how they move toward the player.
/// A level is an ordered list of these assets.
/// </summary>
[CreateAssetMenu(menuName = "Shooter/Wave Definition", fileName = "New WaveDefinition")]
public sealed class WaveDefinition : ScriptableObject
{
    /// <summary>
    /// How enemies in this wave travel their inward line toward the player.
    /// Lateral patterns oscillate on the tangent of that line; inward progress is constant for all of them.
    /// </summary>
    public enum MovementPattern
    {
        /// <summary>
        /// Straight toward the player with no lateral motion.
        /// </summary>
        Direct,

        /// <summary>
        /// A triangle-wave zig-zag across the inward line.
        /// </summary>
        Sawtooth,

        /// <summary>
        /// A smooth sine weave across the inward line.
        /// </summary>
        Sine
    }

    /// <summary>
    /// The enemy prefab this wave spawns.
    /// </summary>
    public Enemy enemyPrefab;

    /// <summary>
    /// The movement pattern for every enemy in the wave.
    /// </summary>
    public MovementPattern pattern = MovementPattern.Direct;

    /// <summary>
    /// Inward speed toward the player, in units per second.
    /// </summary>
    public float speed = 1.5f;

    /// <summary>
    /// Lateral oscillation half-width, in units. Ignored by the Direct pattern.
    /// </summary>
    public float amplitude = 1f;

    /// <summary>
    /// Lateral oscillation rate, in cycles per second. Ignored by the Direct pattern.
    /// </summary>
    public float frequency = 0.5f;

    /// <summary>
    /// Constant spin applied to every enemy in the wave, in degrees per second.
    /// Zero means enemies face their travel direction instead.
    /// </summary>
    public float spinDegreesPerSecond;

    /// <summary>
    /// How many enemies the wave spawns in total.
    /// </summary>
    public int count = 20;

    /// <summary>
    /// Seconds between consecutive spawns.
    /// </summary>
    public float spawnInterval = 0.4f;

    /// <summary>
    /// The HDR tint identifying this wave; applied to enemy sprites and their kill particles.
    /// </summary>
    [ColorUsage(true, true)]
    public Color color = Color.red;

    /// <summary>
    /// Hits an enemy of this wave survives before dying.
    /// </summary>
    public float health = 3f;
}
