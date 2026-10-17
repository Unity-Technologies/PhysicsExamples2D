using UnityEngine;

/// <summary>
/// The berserker's size pulse: the body sprite throbs around its authored scale.
/// Purely visual, driven through <see cref="Enemy.visualScale"/>; the physics shape and the movement-system spin are untouched.
/// </summary>
[RequireComponent(typeof(Enemy))]
public sealed class BerserkerVisuals : MonoBehaviour
{
    void Awake() => m_Enemy = GetComponent<Enemy>();

    void Update() => m_Enemy.visualScale = 1f + m_PulseAmount * Mathf.Sin(Mathf.PI * 2f * m_PulseRate * Time.time);

    #region Internal

    // How far the scale swings either side of the authored size, and how many throbs per second.
    [SerializeField]
    [Range(0f, 0.5f)]
    float m_PulseAmount = 0.15f;

    [SerializeField]
    float m_PulseRate = 3f;

    Enemy m_Enemy;

    #endregion
}
