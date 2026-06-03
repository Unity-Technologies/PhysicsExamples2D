using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates the basics of creating and destroying a shape.
/// See the comments for more information.
/// </summary>
public class CreatePhysicsShape : MonoBehaviour
{
    /// <summary>
    /// The definition contains the start-up state for our body.
    /// All types have their own definition
    /// Selecting the "Example" GameObject, you can edit this field in the inspector.
    /// Pressing "Play" will create our body and shape and should be visible by default in the Game/Scene view.
    /// Note that the default body-type is "Static" so the body doesn't move by default, but you can configure that in the Inspector.
    /// </summary>
    public PhysicsBodyDefinition BodyDefinition = PhysicsBodyDefinition.defaultDefinition;

    /// <summary>
    /// The definition contains the start-up state for our shape.
    /// All shapes use a definition.
    /// Selecting the "Example" GameObject, you can edit this field in the inspector.
    /// </summary>
    public PhysicsShapeDefinition ShapeDefinition = PhysicsShapeDefinition.defaultDefinition;
    
    /// <summary>
    /// The geometry for our shape.
    /// There are several geometries available for Circle, Capsule, Polygon, Segment and Chain.
    /// Selecting the "Example" GameObject, you can edit this field in the inspector.
    /// </summary>
    public CircleGeometry MyCircleGeometry = CircleGeometry.defaultGeometry;
    
    private PhysicsBody m_PhysicsBody;
    
    private void OnEnable()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
      
        // Create the body using our body definition.
        m_PhysicsBody = world.CreateBody(BodyDefinition);
        
        // Create the circle shape using our circle geometry and shape definition.
        // NOTE: Shapes are always created against a body, they do not exist without one.
        m_PhysicsBody.CreateShape(MyCircleGeometry, ShapeDefinition);
    }
    
    private void OnDisable()
    {
        // Destroy the body.
        m_PhysicsBody.Destroy();
    }
}