using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates the basics of creating and destroying a body.
/// See the comments for more information.
/// </summary>
public class CreatePhysicsBody : MonoBehaviour
{
    /// <summary>
    /// The definition contains the start-up state for our body.
    /// All types have a definition and are serializable.
    /// Selecting the "Example" GameObject, you can edit this field in the inspector.
    /// Pressing "Play" will create our body but by default we don't draw bodies so nothing will be visible.
    /// </summary>
    public PhysicsBodyDefinition BodyDefinition = PhysicsBodyDefinition.defaultDefinition;
    
    private PhysicsBody m_PhysicsBody;
    
    private void OnEnable()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        // Create the body using our body definition.
        m_PhysicsBody = world.CreateBody(BodyDefinition);
    }
    
    private void OnDisable()
    {
        // Destroy the body.
        m_PhysicsBody.Destroy();
    }
}