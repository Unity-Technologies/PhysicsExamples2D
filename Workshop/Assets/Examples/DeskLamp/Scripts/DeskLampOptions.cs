using UnityEngine;

/// <summary>
/// Adds the desk lamp's controls to the Workshop menu: the two spring frequencies, how much the springs are damped, the friction at the post and elbow, and the angle of the shade.
/// Every change acts on the running lamp straight away.
/// </summary>
public sealed class DeskLampOptions : WorkshopOptionsProvider
{
    protected override void SetupOptions()
    {
        if (m_Contents == null)
            return;

        AddSlider("Arm Spring", m_Contents.armSpring, 0f, 20f, value => m_Contents.armSpring = value, immediate: true);
        AddSlider("Head Spring", m_Contents.headSpring, 0f, 20f, value => m_Contents.headSpring = value, immediate: true);
        AddSlider("Spring Damping", m_Contents.springDamping, 0f, 5f, value => m_Contents.springDamping = value, immediate: true);
        AddSlider("Post Friction", m_Contents.postFriction, 0f, 200f, value => m_Contents.postFriction = value, immediate: true);
        AddSlider("Elbow Friction", m_Contents.elbowFriction, 0f, 200f, value => m_Contents.elbowFriction = value, immediate: true);
        AddSlider("Head", m_Contents.headAngle, -68.75f, 68.75f, value => m_Contents.headAngle = value, immediate: true);
    }

    #region Internal

    [SerializeField] DeskLampContents m_Contents;

    #endregion
}
