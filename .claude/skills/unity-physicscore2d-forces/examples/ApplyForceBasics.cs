// Demonstrates the four basic force/impulse/torque application methods on a PhysicsBody.
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Minimal force/impulse/torque demo. Creates one dynamic circle body and exposes the four primary force APIs:
/// - `ApplyForceToCenter` (continuous force at center of mass; press Up arrow)
/// - `ApplyLinearImpulseToCenter` (instantaneous velocity change; tap Space)
/// - `ApplyTorque` (continuous angular force; press Right arrow)
/// - `ApplyForce` (continuous force at an offset point; press Left arrow — this also generates torque)
/// </summary>
public class ApplyForceBasics : MonoBehaviour
{
    public Vector2 force = new Vector2(0f, 100f);
    public float torque = 50f;

    private PhysicsBody m_Body;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;

        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        m_Body = world.CreateBody(bodyDef);

        var circle = new CircleGeometry { center = Vector2.zero, radius = 0.5f };
        m_Body.CreateShape(circle, PhysicsShapeDefinition.defaultDefinition);
    }

    private void OnDisable()
    {
        if (m_Body.isValid)
            m_Body.Destroy();
    }

    private void Update()
    {
        if (!m_Body.isValid)
            return;

        // Continuous push toward "force" each frame the key is held.
        if (Input.GetKey(KeyCode.UpArrow))
            m_Body.ApplyForceToCenter(force, wake: true);

        // One-shot velocity change: large effect per call. Use for jumps, knockbacks.
        if (Input.GetKeyDown(KeyCode.Space))
            m_Body.ApplyLinearImpulseToCenter(force, wake: true);

        // Continuous angular force.
        if (Input.GetKey(KeyCode.RightArrow))
            m_Body.ApplyTorque(torque, wake: true);

        // Same magnitude force, but applied at an offset → produces torque as well as linear motion.
        if (Input.GetKey(KeyCode.LeftArrow))
            m_Body.ApplyForce(force, m_Body.position + new Vector2(0.5f, 0f), wake: true);
    }
}
