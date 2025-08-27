using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

using Random = Unity.Mathematics.Random;

/// <summary>
/// Demonstrates the basics of using the PhysicsDestructor to fragment geometries using fragment points to produce multiple fragments.
/// Press "Play" to see the destructor result. Edit the Slice ray to see the results.
/// See the comments for more information.
/// </summary>
public class PhysicsDestructorFragmentMaskGeometry : MonoBehaviour
{
    public PhysicsTransform FragmentTransform = Vector2.down * 2f;
    [Range(0.05f, 5)] public float FragmentMaskRadius = 2f;
    [Range(1, 1000)] public int FragmentPointCount = 100;
    [Range(0f, 1f)] public float GravityScale = 1f;
    public RigidbodyType2D FragmentBodyType = RigidbodyType2D.Dynamic;

    // Polygon Geometries.
    // NOTE: Polygon radius is NOT supported with Fragmenting and will be ignored.
    public PolygonGeometry PolygonGeometry1 = PolygonGeometry.CreateBox(new Vector2(3f, 2f));
    public PolygonGeometry PolygonGeometry2 = PolygonGeometry.CreateBox(new Vector2(2f, 4f));

    private Random m_Random = new (0x12345678);
    private PhysicsWorld m_PhysicsWorld;
    private NativeArray<Vector2> m_FragmentedPoints;
    private NativeArray<PolygonGeometry> m_FragmentMask;

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
        m_PhysicsWorld.DrawCircle(FragmentTransform.position, FragmentMaskRadius, Color.gray5);

        // Draw the fragment points.
        if (m_FragmentedPoints.IsCreated)
            foreach (var point in m_FragmentedPoints)
                m_PhysicsWorld.DrawPoint(point, 3f, Color.gray3);
    }

    private void RunDestructorExample()
    {
        // Recreate any existing physics world.
        // NOTE: Using a physics world like this isn't good practice, it's just a simple way to "wipe the slate clean" for this demonstration.
        if (m_PhysicsWorld.isValid)
            m_PhysicsWorld.Destroy();

        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create();

        // Create a static area for the shapes to move around in.
        CreateArea();
        
        // Create the fragment points.
        CreateFragmentPoints();

        // Create the Target mask.
        CreateTargetMask();
        
        // Create the target polygons.
        using var polygons = CreateTargetPolygons();
        
        // We may have produced no polygons, depending on the operations we choose etc.
        if (polygons.Length == 0)
            return;           
        
        // Create the target set of fragment geometry.
        // NOTE: This is NOT a copy operation so the source geometry provided here must exist until you have used the FragmentGeometry.
        // We have the option to also transform the geometry which, if coming from a specific PhysicsBody, would be the "PhysicsBody.transform".
        // This can come from any source that is able to provide a Span<PolygonGeometry>.
        // If you have anything other than PolygonGeometry, you can use the PhysicsComposer to quickly convert it.
        var target = new PhysicsDestructor.FragmentGeometry(PhysicsTransform.identity, polygons);

        // Create the fragment mask.
        var mask = new PhysicsDestructor.FragmentGeometry(PhysicsTransform.identity, m_FragmentMask);
        
        // Perform the fragment operation.
        // Take the target geometry and split it using the mask.
        // The target geometry that doesn't overlap the mask geometry is placed into the "unbrokenGeometry".
        // The target geometry that overlaps the mask geometry and fragment it with the specified fragment points, returning the fragments, returning that in "brokenGeometry".
        // NOTE: Like anything in this API that returns multiple results, you must specify an allocator and dispose of it too.
        using var fragmentResult = PhysicsDestructor.Fragment(target, mask, m_FragmentedPoints, Allocator.Temp);
        
        // Create the "unbroken" geometry on a single static body.
        if (fragmentResult.unbrokenGeometry.Length > 0)
        {
            // Create a body definition for all fragments.
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Static,
                position = fragmentResult.transform.position,
                rotation = fragmentResult.transform.rotation
            };

            // Create the shapes.
            using var shapes = m_PhysicsWorld.CreateBody(bodyDef).CreateShapeBatch(fragmentResult.unbrokenGeometry, PhysicsShapeDefinition.defaultDefinition);
        }
        
        // Create the "unbroken" geometry on multiple bodies.
        if (fragmentResult.brokenGeometry.Length > 0)
        {
            // Create a body definition for all fragments.
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = FragmentBodyType,
                position = fragmentResult.transform.position,
                rotation = fragmentResult.transform.rotation,
                gravityScale = GravityScale
            };
        
            // Create broken fragments with their own bodies.
            foreach (var geometry in fragmentResult.brokenGeometry)
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
    }

    private void CleanDestructorExample()
    {
        // Dispose of the fragment points.
        if (m_FragmentedPoints.IsCreated)
            m_FragmentedPoints.Dispose();
        
        // Dispose of the fragment mask.
        if (m_FragmentMask.IsCreated)
            m_FragmentMask.Dispose();

        // Destroying a world will destroy all its contents.
        if (m_PhysicsWorld.isValid)
            m_PhysicsWorld.Destroy();
    }
    
    private void CreateArea()
    {
        // Ground Body. 
        m_PhysicsWorld.CreateBody().CreateShape(PolygonGeometry.CreateBox(new Vector2(100f, 4f), 0f, new PhysicsTransform(Vector2.down * 7f)));
    }
    
    private void CreateFragmentPoints()
    {
        // Dispose of any existing fragment points.
        if (m_FragmentedPoints.IsCreated)
            m_FragmentedPoints.Dispose();
        
        // Create the fragment points.
        m_FragmentedPoints = new NativeArray<Vector2>(FragmentPointCount, Allocator.Persistent);
        for (var i = 0; i < FragmentPointCount; ++i)
            m_FragmentedPoints[i] = FragmentTransform.position + new PhysicsRotate(m_Random.NextFloat(0f, PhysicsMath.TAU)).direction * m_Random.NextFloat(0f, FragmentMaskRadius);
    }
    
    private void CreateTargetMask()
    {
        // Dispose of the fragment mask.
        if (m_FragmentMask.IsCreated)
            m_FragmentMask.Dispose();
        
        // Create a Polygon geometry mask from a Circle.
        var composer = PhysicsComposer.Create();
        composer.AddLayer(new CircleGeometry { radius = FragmentMaskRadius }, FragmentTransform);
        m_FragmentMask = composer.CreatePolygonGeometry(vertexScale: Vector2.one, allocator: Allocator.Persistent);
        composer.Destroy();
    }
    
    private NativeArray<PolygonGeometry> CreateTargetPolygons()
    {
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
        var polygons = composer.CreatePolygonGeometry(vertexScale: Vector2.one, allocator: Allocator.Temp);
    
        // We must always dispose of the allocator when we're finished.
        composer.Destroy();

        return polygons;
    }
}