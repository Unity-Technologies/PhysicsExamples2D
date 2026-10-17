using UnityEngine;

/// <summary>
/// A subtle positional camera shake with quadratic falloff.
/// Lives on the camera; anything can request a shake and overlapping requests keep the strongest remaining one.
/// </summary>
public sealed class CameraShake : MonoBehaviour
{
    /// <summary>
    /// Starts (or refreshes) a shake of the given strength, decaying to nothing over the duration.
    /// </summary>
    public void Shake(float amplitude, float duration)
    {
        m_Amplitude = Mathf.Max(amplitude, currentAmplitude);
        m_Duration = Mathf.Max(0.01f, duration);
        m_Until = Time.time + m_Duration;
    }

    void OnEnable() => m_BasePosition = transform.localPosition;

    void Update()
    {
        if (m_Until <= 0f)
            return;

        if (Time.time >= m_Until)
        {
            transform.localPosition = m_BasePosition;
            m_Until = 0f;
            return;
        }

        var offset = Random.insideUnitCircle * currentAmplitude;

        transform.localPosition = m_BasePosition + new Vector3(offset.x, offset.y, 0f);
    }

    // The strength right now: the requested amplitude scaled by the squared remaining fraction, so it dies off quickly.
    float currentAmplitude
    {
        get
        {
            if (m_Until <= 0f || Time.time >= m_Until)
                return 0f;

            var falloff = (m_Until - Time.time) / m_Duration;

            return m_Amplitude * falloff * falloff;
        }
    }

    #region Internal

    Vector3 m_BasePosition;
    float m_Amplitude;
    float m_Duration;
    float m_Until;

    #endregion
}
