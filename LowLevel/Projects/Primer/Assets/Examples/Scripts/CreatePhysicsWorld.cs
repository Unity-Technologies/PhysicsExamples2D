using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates the basics of creating and destroying a world.
/// Pressing "Play" will result in multiple world IDs being shown in the console.
/// See the comments for more information.
/// </summary>
public class CreatePhysicsWorld : MonoBehaviour
{
    public PhysicsWorldDefinition WorldDefinition = PhysicsWorldDefinition.defaultDefinition;
    
    private void Start()
    {
        {
            // Create a world (using the default world definition).
            // This world exists in isolation from any other world you create.
            // Anything you create in this world is completely isolated from any other objects in other worlds.
            // The world can be simulated in parallel to other worlds too.
            //
            // All main physics objects use "definitions" that allow you to specify up-front what the settings are for that object.
            // This API pattern is much faster than creating an object then performing many changes to it while it's "live".
            // All definitions (as are all types in this API) are structs so can be reused and passed around, even used in C# Jobs.
            // All definitions have a default definition by accessing the static "defaultDefinition" property.
            var world = PhysicsWorld.Create(PhysicsWorldDefinition.defaultDefinition);

            // Log the world ID.
            // Everything you create is simply a "struct" that only contains a 64-bit ID.
            // When you log objects, their ID is output as that is the default implementation for their "ToString()".
            Debug.Log(world);

            // Destroy the world.
            // Destroying a world will also destroy any objects created in it.
            // Any objects (structs) that you still have a reference too will no longer function as their handle is not invalid.
            // Using an object with an invalid handle will produce a clear warning in the console that this is the case.
            // This is true for all objects created.
            // All objects that have handles have an "isValid" property to check this. 
            world.Destroy();
        }

        {
            // Create a world (implicitly using the default world definition).
            // All creation methods that accept a definition also have an overload that allows you to specify nothing which implicitly uses the default.
            // There is no functional difference, only convenience to implicitly mean to use its default definition.
            var world = PhysicsWorld.Create();

            // Log the world ID.
            Debug.Log(world);

            // Destroy the world.
            world.Destroy();
        }
        
        {
            // Get a copy of the default world definition.
            var myWorldDefinition = PhysicsWorldDefinition.defaultDefinition;
            
            // Change the definition to invert gravity.
            myWorldDefinition.gravity = Vector2.up * 10f;
            
            // Create the world using my modified definition.
            var world = PhysicsWorld.Create(myWorldDefinition);

            // Log the world ID.
            Debug.Log(world);

            // Destroy the world.
            world.Destroy();
        }
        
        {
            // Create a world using the public field above.
            // You can edit this world definition right in the inspector then hit play.
            var world = PhysicsWorld.Create(WorldDefinition);

            // Log the world ID.
            Debug.Log(world);

            // Destroy the world.
            world.Destroy();
        }
    }
}
