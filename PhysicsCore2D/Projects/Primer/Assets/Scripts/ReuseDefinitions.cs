using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates the basics of creating bodies/shapes and reusing definitions.
/// See the comments for more information.
/// </summary>
public class ReuseDefinitions : MonoBehaviour
{
    /// <summary>
    /// The definition contains the start-up state for our body.
    /// All types have their own definition
    /// Selecting the "Example" GameObject, you can edit this field in the inspector.
    /// Pressing "Play" will show all the created bodies/shapes.
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

    private PhysicsWorld m_PhysicsWorld;
    
    private void OnEnable()
    {
        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create();

        for (var n = 0; n < 10; n++)
        {
            // Set the body definition.
            // We want to keep all the properties we set but change the position.
            BodyDefinition.position = new Vector2(-4.5f + n, 0f);
            
            // Create the body using our body definition.
            var body = m_PhysicsWorld.CreateBody(BodyDefinition);

            // Set the circle geometry.
            MyCircleGeometry.radius = n * 0.2f + 0.2f;
            
            // Create the circle shape using our circle geometry and shape definition.
            body.CreateShape(MyCircleGeometry, ShapeDefinition);
        }
    }
    
    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        m_PhysicsWorld.Destroy();
    }
}