using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates the basics of using the PhysicsComposer to composer geometries.
/// Press "Play" to see the composer result. Edit the Geometries and their transforms and operation to see the results.
/// See the comments for more information.
/// </summary>
public class PhysicsComposerGeometry : MonoBehaviour
{
    // A test Circle geometry.
    public CircleGeometry MyCircleGeometry = CircleGeometry.defaultGeometry;
    
    // How to transform the Circle geometry.
    public PhysicsTransform CircleTransform = PhysicsTransform.identity;
    
    // A test Capsule geometry.
    public CapsuleGeometry MyCapsuleGeometry = CapsuleGeometry.defaultGeometry;
    
    // How to transform the Capsule Geometry.
    public PhysicsTransform CapsuleTransform = Vector2.right * 0.75f;

    // The operation we want to apply with the capsule against the circle geometry..
    public PhysicsComposer.Operation ComposerOperation = PhysicsComposer.Operation.OR;
    
    private PhysicsWorld m_PhysicsWorld;
    
    private void OnEnable()
    {
        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create();

        RunComposerExample();
    }
    
    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        if (m_PhysicsWorld.isValid)
            m_PhysicsWorld.Destroy();
    }

    private void RunComposerExample()
    {
        // Create a composer.
        // We use the default TEMP allocator here but as with anything in the API that can create multiple results, you can choose an appropriate allocator.
        var composer = PhysicsComposer.Create();
        
        // Add our test geometry as layers.
        // Layers can be added in any order which is the implicit order they will be processed but there are arguments when adding a layer that allows you to specify the order.
        // Additional arguments are the operation to perform, the curve-stride to use when converting curves to edges and if to generate a reverse winding of the geometry for this layer.
        // The first layer to be processed (whether implicit or explicit) will always uses a "PhysicsComposer.Operation.OR" operation (even if something else is specified) as it has noting to operate with and forms the base geometry for further layers/operations. 
        composer.AddLayer(MyCircleGeometry, CircleTransform);
        composer.AddLayer(MyCapsuleGeometry, CapsuleTransform, ComposerOperation);

        // Generate polygon geometry from the composer layers.
        // We can scale the geometry and specify how the results are allocated, in this case using the TEMP allocator.
        // We can also use "composer.CreateChainGeometry" to create a set of PhysicsChain (outlines).
        using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, allocator: Allocator.Temp);
        
        // We must always dispose of the allocator when we're finished.
        composer.Destroy();

        // We may have produced no polygons, depending on the operations we choose etc.
        if (polygons.Length == 0)
            return;
        
        // Create a body.
        var body = m_PhysicsWorld.CreateBody();
       
        // Create the shape(s) from the composed polygon geometry.
        // NOTE: We can have a single or multiple polygons here so we'll use the shape batch creation for convenience.
        // Batch creation returns the PhysicsShape created, although we're not interested in them in this example.
        using var shapes = body.CreateShapeBatch(polygons, PhysicsShapeDefinition.defaultDefinition);
    }

    private void OnValidate()
    {
        // Finish if there's no valid physics world.
        if (!m_PhysicsWorld.isValid)
            return;
        
        // Recreate any existing physics world.
        // NOTE: Using a physics world like this isn't good practice, it's just a simple way to "wipe the slate clean" for this demonstration.
        m_PhysicsWorld.Destroy();
        m_PhysicsWorld = PhysicsWorld.Create();

        RunComposerExample();
    }
}