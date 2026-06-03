using UnityEngine;

/// <summary>
/// Controls the BuoyanceEffector2D surface level property via a slider.
/// </summary>
[RequireComponent(typeof(BuoyancyEffector2D), typeof(Collider2D))]
public class BuoyancyEffector2D_SurfaceLevel : MonoBehaviour
{
    private BuoyancyEffector2D m_Effector;
    private Bounds m_EffectorBounds;
    private Rect m_SliderBounds;

    private float m_SurfaceLevel = 0.5f;

    void Start()
    {
        m_Effector = GetComponent<BuoyancyEffector2D>();
        m_EffectorBounds = GetComponent<Collider2D>().bounds;

        // Calculate the slider bounds.
        var camera = Camera.main;
        var min = camera.WorldToScreenPoint(m_EffectorBounds.min);
        var max = camera.WorldToScreenPoint(m_EffectorBounds.max);
        m_SliderBounds.Set(20.0f, 20.0f, 30.0f, max.y - min.y);
    }

    void OnGUI()
    {
        m_SurfaceLevel = GUI.VerticalSlider(m_SliderBounds, m_SurfaceLevel, 0.5f, -0.6f);
    }

    void Update()
    {
        if (m_Effector)
        {
            m_Effector.surfaceLevel = m_SurfaceLevel;

            var min = m_EffectorBounds.min;
            var max = m_EffectorBounds.max;           
            var boundsLevel = Mathf.LerpUnclamped(min.y, max.y, m_SurfaceLevel + 0.5f);
            var start = new Vector3(min.x, boundsLevel, 0.0f);
            var end = new Vector3(max.x, boundsLevel, 0.0f);
            Debug.DrawLine(start, end, Color.yellow);
        }
    }
}
