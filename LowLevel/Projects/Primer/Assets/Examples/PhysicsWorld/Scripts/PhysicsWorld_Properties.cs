using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates the basics of changing a property on a world although this applies to all objects.
/// </summary>
public class PhysicsWorld_Properties : MonoBehaviour
{
    private void Start()
    {
        // Create a world.
        // While the "world" struct returned only contains a handle to the actual world instance created,
        // it can be used in an object-orientated way and has properties and methods which can be used directly.
        var world = PhysicsWorld.Create();

        // Log the default gravity.
        Debug.Log(world.gravity);
        
        // Change the default gravity.
        // While we're simply showing a property of the world we created being changed, it's also the first instance of bad practice.
        // If we wanted the gravity to be this for the world lifetime, we should specify that in the world definition when we created it.
        // This is the same for all objects that are created using definitions. In many cases, using definitions has a huge positive impact on performance!
        world.gravity = Vector2.down * 5f;

        // Log the changed gravity.
        Debug.Log(world.gravity);
        
        // Destroy the world.
        world.Destroy();
    }
}