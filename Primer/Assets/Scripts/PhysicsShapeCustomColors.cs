using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates the basics of creating a shape with a custom color.
/// See the comments for more information.
/// </summary>
public class PhysicsShapeCustomColors : MonoBehaviour
{
    /// <summary>
    /// The definition contains the start-up state for our shape.
    /// All shapes use a definition.
    /// Selecting the "Example" GameObject, you can edit this field in the inspector.
    /// Expand "Shape Definition > Surface Material" and change the "Custom Color" field.
    /// </summary>
    public PhysicsShapeDefinition ShapeDefinition = PhysicsShapeDefinition.defaultDefinition;
    
    private PhysicsWorld m_PhysicsWorld;
    
    private void OnEnable()
    {
        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create();
      
        // Create the body.
        var body = m_PhysicsWorld.CreateBody();

        // Set this to true to override what you specified in the inspector.
#if false        
        ShapeDefinition.surfaceMaterial.customColor = Color.darkOrange;
#endif        
        var shape = body.CreateShape(CircleGeometry.defaultGeometry, ShapeDefinition);
        
        // Set this to true to override what the shape currently uses.
#if false 
        var surfaceMaterial = shape.surfaceMaterial;
        surfaceMaterial.customColor = Color.deepPink;
        shape.surfaceMaterial = surfaceMaterial;
#endif        
    }
    
    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        m_PhysicsWorld.Destroy();
    }
}