using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates the very common usage of the default world.
/// See the comments for more information.
/// </summary>
public class UseDefaultPhysicsWorld : MonoBehaviour
{
    PhysicsWorld m_PhysicsWorld;
    
    private void Start()
    {
        // Create a world.
        // While the "world" struct returned only contains a handle to the actual world instance created,
        // it can be used in an object-orientated way and has properties and methods which can be used directly.
        m_PhysicsWorld = PhysicsWorld.defaultWorld;
        
        // Log the world ID.
        Debug.Log(m_PhysicsWorld);
    }
}