using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

using Random = Unity.Mathematics.Random;

/// <summary>
/// Demonstrates the basics of using the PhysicsDestructor to fragment geometries using fragment points to produce multiple fragments.
/// Press "Play" to see the destructor result. Edit the Slice ray to see the results.
/// See the comments for more information.
/// </summary>
public class PhysicsDestructorFragmentGeometry : MonoBehaviour
{
    [Range(1, 1000)] public int FragmentPointCount = 100;
    [Range(0.05f, 5)] public float FragmentPointRadius = 0.95f;
    [Range(0f, 1f)] public float GravityScale = 0f;
    public RigidbodyType2D FragmentBodyType = RigidbodyType2D.Dynamic;

    // Polygon Geometries.
    // NOTE: Polygon radius is NOT supported with slicing and will be ignored.
    public PolygonGeometry PolygonGeometry1 = PolygonGeometry.CreateBox(new Vector2(3f, 2f));
    public PolygonGeometry PolygonGeometry2 = PolygonGeometry.CreateBox(new Vector2(2f, 4f));

    private Random m_Random = new (0x12345678);
    private PhysicsWorld m_PhysicsWorld;
    private NativeArray<Vector2> m_FragmentedPoints;

    private void OnEnable() => RunDestructorExample();
    private void OnDisable() => CleanDestructorExample();

    private void OnValidate()
    {
        if (Application.isPlaying)
            RunDestructorExample();
    }
    
    private void Update()
    {
        if (!m_PhysicsWorld.isValid)
            return;

        // Draw the original geometries.
        m_PhysicsWorld.DrawGeometry(PolygonGeometry1, PhysicsTransform.identity, Color.gray4);
        m_PhysicsWorld.DrawGeometry(PolygonGeometry2, PhysicsTransform.identity, Color.gray4);
        
        // Draw the fragment radius.
        m_PhysicsWorld.DrawCircle(Vector2.zero, FragmentPointRadius, Color.gray5);

        // Draw the fragment points.
        if (m_FragmentedPoints.IsCreated)
            foreach (var point in m_FragmentedPoints)
                m_PhysicsWorld.DrawPoint(point, 3f, Color.gray7);
    }

    private void RunDestructorExample()
    {
        // Recreate any existing physics world.
        // NOTE: Using a physics world like this isn't good practice, it's just a simple way to "wipe the slate clean" for this demonstration.
        if (m_PhysicsWorld.isValid)
            m_PhysicsWorld.Destroy();

        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create();
        
        // Dispose of any existing fragment points.
        if (m_FragmentedPoints.IsCreated)
            m_FragmentedPoints.Dispose();
        
        // Create the fragment points.
        m_FragmentedPoints = new NativeArray<Vector2>(FragmentPointCount, Allocator.Persistent);
        for (var i = 0; i < FragmentPointCount; ++i)
            m_FragmentedPoints[i] = new PhysicsRotate(m_Random.NextFloat(0f, PhysicsMath.TAU)).direction * m_Random.NextFloat(0f, FragmentPointRadius);

        // Create a static area for the shapes to move around in.
        CreateArea();

        // Create a composer.
        // We use the default TEMP allocator here but as with anything in the API that can create multiple results, you can choose an appropriate allocator.
        var composer = PhysicsComposer.Create();
    
        // Add our test geometry as layers.
        // We default to OR operation here to simply merge the result.
        composer.AddLayer(PolygonGeometry1, PhysicsTransform.identity);
        composer.AddLayer(PolygonGeometry2, PhysicsTransform.identity);

        // Generate polygon geometry from the composer layers.
        // We can scale the geometry and specify how the results are allocated, in this case using the TEMP allocator.
        // We can also use "composer.CreateChainGeometry" to create a set of PhysicsChain (outlines).
        using var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, allocator: Allocator.Temp);
    
        // We must always dispose of the allocator when we're finished.
        composer.Destroy();

        // We may have produced no polygons, depending on the operations we choose etc.
        if (polygons.Length == 0)
            return;            

        // Create the target set of fragment geometry.
        // NOTE: This is NOT a copy operation so the source geometry provided here must exist until you have used the FragmentGeometry.
        // We have the option to also transform the geometry which, if coming from a specific PhysicsBody, would be the "PhysicsBody.transform".
        // This can come from any source that is able to provide a Span<PolygonGeometry>.
        // If you have anything other than PolygonGeometry, you can use the PhysicsComposer to quickly convert it.
        var target = new PhysicsDestructor.FragmentGeometry(PhysicsTransform.identity, polygons);

        // Perform the fragment operation.
        // Take the target geometry and fragment it with the specified fragment points, returning the fragments.
        // If none of the fragment points overlap the target geometry, all geometry is returned in "unbrokenGeometry".
        // If at least a single fragment point overlaps the target geometry, all geometry is returned in "brokenGeometry".
        // NOTE: Like anything in this API that returns multiple results, you must specify an allocator and dispose of it too.
        using var fragmentResult = PhysicsDestructor.Fragment(target, m_FragmentedPoints, Allocator.Temp);
        
        // Fetch the fragment geometry, no matter whether it's unbroken or not.
        var fragmentGeometry = fragmentResult.unbrokenGeometry.Length > 0 ? fragmentResult.unbrokenGeometry : fragmentResult.brokenGeometry;

        // Create a body definition for all fragments.
        var bodyDef = new PhysicsBodyDefinition
        {
            bodyType = FragmentBodyType,
            position = fragmentResult.transform.position,
            rotation = fragmentResult.transform.rotation,
            gravityScale = GravityScale
        };
        
        // Create fragments with their own bodies.
        foreach (var geometry in fragmentGeometry)
        {
            // Yes, so create a dynamic body at the fragment transform with movement away from the fragment center.
            bodyDef.linearVelocity = geometry.centroid.normalized * 0.25f;
            var body = m_PhysicsWorld.CreateBody(bodyDef);
            
            // Create a colourful shape definition (for fun).
            var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.HSVToRGB(m_Random.NextFloat(), 1f, m_Random.NextFloat(0.5f, 1f)) } };
            
            // Create the fragment shape.
            body.CreateShape(geometry, shapeDef);
        }
    }

    private void CleanDestructorExample()
    {
        // Dispose of the fragment points.
        if (m_FragmentedPoints.IsCreated)
            m_FragmentedPoints.Dispose();

        // Destroying a world will destroy all its contents.
        if (m_PhysicsWorld.isValid)
            m_PhysicsWorld.Destroy();
    }

    private void CreateArea()
    {
        // Ground Body. 
        var groundBody = m_PhysicsWorld.CreateBody();

        var extents = new Vector2(8f, 5f);
        using var extentPoints = new NativeList<Vector2>(Allocator.Temp)
        {
            new(-extents.x, extents.y),
            new(extents.x, extents.y),
            new(extents.x, -extents.y),
            new(-extents.x, -extents.y)
        };

        // Create a chain of line segments.
        groundBody.CreateChain(
            geometry: new ChainGeometry(extentPoints.AsArray()),
            definition: PhysicsChainDefinition.defaultDefinition);
    }
}