using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates the basics of using the PhysicsDestructor to slice geometries in half to produce two distinct fragment geometries of "left" and "right".
/// Press "Play" to see the destructor result. Edit the slice ray Geometries and their transforms and operation to see the results.
/// See the comments for more information.
/// </summary>
public class PhysicsDestructorSliceGeometry : MonoBehaviour
{
    // The slice vector.
    public Vector2 SliceOrigin = new(-3f, -3f);
    public Vector2 SliceTranslation = new(7f, 6f);

    // Polygon Geometries.
    // NOTE: Polygon radius is NOT supported with Slicing and will be ignored.
    public PolygonGeometry PolygonGeometry1 = PolygonGeometry.CreateBox(new Vector2(3f, 2f));
    public PolygonGeometry PolygonGeometry2 = PolygonGeometry.CreateBox(new Vector2(2f, 4f));

    private PhysicsWorld m_PhysicsWorld;
    private readonly PhysicsShapeDefinition m_ShapeDefLeft = new() { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.aquamarine } };
    private readonly PhysicsShapeDefinition m_ShapeDefRight = new() { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.coral } };

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

        // Draw the slice.
        m_PhysicsWorld.DrawPoint(SliceOrigin, 10f, Color.softBlue);
        m_PhysicsWorld.DrawPoint(SliceOrigin + SliceTranslation, 10f, Color.softYellow);
        m_PhysicsWorld.DrawLine(SliceOrigin, SliceOrigin + SliceTranslation, Color.ghostWhite);
    }

    private void RunDestructorExample()
    {
        // Recreate any existing physics world.
        // NOTE: Using a physics world like this isn't good practice, it's just a simple way to "wipe the slate clean" for this demonstration.
        if (m_PhysicsWorld.isValid)
            m_PhysicsWorld.Destroy();

        // Create a world.
        // NOTE: We don't want gravity in this world.
        m_PhysicsWorld = PhysicsWorld.Create(new PhysicsWorldDefinition { gravity = Vector2.zero });

        // Create a static area for the shapes to move around in.
        CreateArea();

        // Create an array of geometries.
        var geometries = new NativeArray<PolygonGeometry>(2, Allocator.Temp);
        geometries[0] = PolygonGeometry1;
        geometries[1] = PolygonGeometry2;

        // Create the target set of fragment geometry.
        // NOTE: This is NOT a copy operation so the source geometry provided here must exist until you have used the FragmentGeometry.
        // We have the option to also transform the geometry which, if coming from a specific PhysicsBody, would be the "PhysicsBody.transform".
        // This can come from any source that is able to provide a Span<PolygonGeometry>.
        // If you have anything other than PolygonGeometry, you can use the PhysicsComposer to quickly convert it.
        var target = new PhysicsDestructor.FragmentGeometry(PhysicsTransform.identity, geometries);

        // Perform the slice operation.
        // NOTE: Like anything in this API that returns multiple results, you must specify an allocator and dispose of it too.
        using var sliceResult = PhysicsDestructor.Slice(target, SliceOrigin, SliceTranslation, Allocator.Temp);

        // We can now dispose the geometries array now.
        geometries.Dispose();

        // The results are returned as geometry on the left or right of the slice.
        // Depending on the slice and geometry, you may get no results on one side.
        // If you only get results on one side then there was no intersection i.e. no slicing so the results on that side are identical to the geometry originally provided.
        var leftGeometry = sliceResult.leftGeometry;
        var rightGeometry = sliceResult.rightGeometry;

        // Fetch the slice position/rotation.
        var slicePosition = sliceResult.transform.position;
        var sliceRotation = sliceResult.transform.rotation;

        // Calculate a small linear velocity moving away from the slice.
        var linearVelocity = Vector2.Perpendicular(SliceTranslation).normalized * 0.25f;

        // Did we produce any geometry on the left side?
        if (leftGeometry.Length > 0)
        {
            // Yes, so create a dynamic body at the slice transform with movement away from the slice.
            var body = m_PhysicsWorld.CreateBody(
                new PhysicsBodyDefinition
                {
                    type = PhysicsBody.BodyType.Dynamic,
                    position = slicePosition,
                    rotation = sliceRotation,
                    linearVelocity = linearVelocity
                });

            // Create the shape(s) from the left polygon geometry.
            // NOTE: We can have a single or multiple polygons here so we'll use the shape batch creation for convenience.
            // Batch creation returns the PhysicsShape created, although we're not interested in them in this example.
            using var shapes = body.CreateShapeBatch(leftGeometry, m_ShapeDefLeft);
        }

        // Did we produce any geometry on the right side?
        if (rightGeometry.Length > 0)
        {
            // Yes, so create a dynamic body at the slice transform with movement away from the slice.
            var body = m_PhysicsWorld.CreateBody(
                new PhysicsBodyDefinition
                {
                    type = PhysicsBody.BodyType.Dynamic,
                    position = slicePosition,
                    rotation = sliceRotation,
                    linearVelocity = -linearVelocity
                });

            // Create the shape(s) from the right polygon geometry.
            // NOTE: We can have a single or multiple polygons here so we'll use the shape batch creation for convenience.
            // Batch creation returns the PhysicsShape created, although we're not interested in them in this example.
            using var shapes = body.CreateShapeBatch(rightGeometry, m_ShapeDefRight);
        }
    }

    private void CleanDestructorExample()
    {
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