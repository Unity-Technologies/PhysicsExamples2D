// Minimal PhysicsCore2D component: creates a body in OnEnable, destroys in OnDisable.
using UnityEngine;
using Unity.U2D.Physics;

public class BasicPhysicsComponent : MonoBehaviour
{
    private PhysicsBody m_PhysicsBody;

    private void OnEnable()
    {
        // Get the main physics world.
        var world = PhysicsWorld.defaultWorld;

        // Create a body.
        m_PhysicsBody = world.CreateBody();
    }

    private void OnDisable()
    {
        // Destroy the body.
        if (m_PhysicsBody.isValid)
            m_PhysicsBody.Destroy();
    }
}
