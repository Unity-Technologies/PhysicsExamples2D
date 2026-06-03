using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates the basics of creating and destroying a shape.
/// See the comments for more information.
/// </summary>
public class CreatePhysicsUserData : MonoBehaviour
{
    // User data is exactly that, a small struct containing useful fields to store a variety of data.
    // User data allows you to specify an object, a 64-bit PhysicsMask, a float, an int and a bool.
    // You can encode a variety of meaning to this data for your own purposes.
    // The data is always available to read, even when working off the main-thread.
    // The physics system never uses this data but allows you to set/get it to most objects including PhysicsWorld, PhysicsBody, PhysicsShape, PhysicsChain and all PhysicJoint.
    public PhysicsUserData UserData;
    
    private PhysicsBody m_PhysicsBody;
    private PhysicsShape m_PhysicsShape;
    
    private void OnEnable()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
      
        // Create a body and a shape.
        m_PhysicsBody = world.CreateBody();
        m_PhysicsShape = m_PhysicsBody.CreateShape(CircleGeometry.defaultGeometry);

        // Assign the user data.
        // NOTE: This is not an "object", it's a struct so is always a copy.
        m_PhysicsBody.userData = UserData;
        m_PhysicsShape.userData = UserData;
    }
    
    private void OnDisable()
    {
        // Destroy the body.
        m_PhysicsBody.Destroy();
    }
}