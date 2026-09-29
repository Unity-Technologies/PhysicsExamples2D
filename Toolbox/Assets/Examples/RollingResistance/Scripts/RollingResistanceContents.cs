using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Rolls twenty identical balls down twenty ramps stacked one above the other, with each ball's rolling resistance a little higher than the one below it.
/// Every ramp is three Physics Area Primitive segments on one static body, and every ball is a Physics Pose with a Physics Area Circle, given its own rolling resistance and starting spin.
/// </summary>
/// <remarks>
/// Rolling resistance slows a shape's spin as it rolls, the way real friction between a wheel and the ground does, so the lowest ball keeps rolling long after the highest one has stopped.
/// The ramp itself can be tilted uphill, kept flat, or tilted downhill, which changes how far gravity alone can carry a ball regardless of its resistance.
/// </remarks>
public sealed class RollingResistanceContents : MonoBehaviour
{
    /// <summary>
    /// The three ramp tilts this example can show.
    /// </summary>
    public enum SlopeType
    {
        Uphill,
        Flat,
        Downhill
    }

    /// <summary>
    /// Destroys every ramp and ball and builds them again with the current slope.
    /// </summary>
    public void Rebuild()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();

        if (m_RampPrefab == null || m_BallPrefab == null)
            return;

        var slopeHeight = m_SlopeType switch
        {
            SlopeType.Uphill => 5f,
            SlopeType.Downhill => -5f,
            _ => 0f
        };

        for (var n = 0; n < RampCount; ++n)
        {
            var baseHeight = 2f * n;

            var ramp = Instantiate(m_RampPrefab, Vector3.zero, Quaternion.identity);
            var segments = ramp.GetComponents<PhysicsAreaPrimitive>();
            segments[0].segmentGeometry = new SegmentGeometry { point1 = new Vector2(-40f, baseHeight), point2 = new Vector2(-40f, baseHeight + 1.5f) };
            segments[1].segmentGeometry = new SegmentGeometry { point1 = new Vector2(-40f, baseHeight), point2 = new Vector2(40f, baseHeight + slopeHeight) };
            segments[2].segmentGeometry = new SegmentGeometry { point1 = new Vector2(40f, baseHeight + slopeHeight), point2 = new Vector2(40f, baseHeight + slopeHeight + 1.5f) };
            ramp.SetActive(true);
            m_Spawned.Add(ramp);

            var ball = Instantiate(m_BallPrefab, new Vector3(-39.5f, baseHeight + 0.75f, 0f), Quaternion.identity);
            var pose = ball.GetComponent<PhysicsPose>();
            var bodyDefinition = pose.definition;
            bodyDefinition.linearVelocity = new Vector2(5f, 0f);
            bodyDefinition.angularVelocity = -10f * Mathf.Rad2Deg;
            pose.definition = bodyDefinition;

            var area = ball.GetComponent<PhysicsArea>();
            var shapeDefinition = area.definition;
            var surfaceMaterial = shapeDefinition.surfaceMaterial;
            surfaceMaterial.rollingResistance = RollingResistanceStep * n;
            shapeDefinition.surfaceMaterial = surfaceMaterial;
            area.definition = shapeDefinition;

            ball.SetActive(true);
            m_Spawned.Add(ball);
        }
    }

    /// <summary>
    /// The ramp tilt every ball rolls down.
    /// Changing this does not affect ramps already built until <see cref="Rebuild"/> is called.
    /// </summary>
    public SlopeType slopeType
    {
        get => m_SlopeType;
        set => m_SlopeType = value;
    }

    private void Start() => Rebuild();

    #region Internal

    // How many ramps are stacked, and the rolling resistance step from one ball to the next.
    const int RampCount = 20;
    const float RollingResistanceStep = 0.02f;

    [SerializeField] GameObject m_RampPrefab;
    [SerializeField] GameObject m_BallPrefab;
    [SerializeField] SlopeType m_SlopeType = SlopeType.Flat;

    readonly List<GameObject> m_Spawned = new();

    #endregion
}
