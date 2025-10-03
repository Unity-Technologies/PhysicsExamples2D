# Physics Destructor

The [PhysicsDestructor](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsDestructor.html) can break down (decompose) input geometries using two methods: [Fragment](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsDestructor.Fragment.html) and [Slice](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsDestructor.Slice.html).

The `PhysicsDestructor` always operates on [PolygonGeometry](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PolygonGeometry.html).
However, converting other geometry types to `PolygonGeometry` is straightforward (see [PhysicsComposer](PhysicsComposer.md) for details).

The output of a `PhysicsDestructor` is always in the form of `PolygonGeometry` as well.

## Slicing

Slicing geometry essentially divides it into two groups along a specified ray, functioning like a 2D plane intersection. The result is geometry separated into "left" and "right" sides relative to the ray.

In the following example, we start by providing a concave or convex outline as a set of vertices and convert them into an array of `PolygonGeometry`.
We then slice this geometry along a ray and create both the left and right portions as separate shapes, each attached to its own body:

```csharp
void Run(ReadOnlySpan<Vector2> outlineVertices)
{
    // Create polygon geometries from a concave/convex vertex outline 
    // NOTE: We don't transform or scale it here.
    using var polygonGeometries = PolygonGeometry.CreatePolygons(outlineVertices, PhysicsTransform.identity, Vector2.one);

    // Create the target geometry we want to operate on.
    var targetGeometry = new PhysicsDestructor.FragmentGeometry(PhysicsTransform.identity, polygonGeometries);
    
    // Define the ray we want to slice with.
    // NOTE: The ray extends to infinity in both ray directions acting like a 2D plane.
    var rayOrigin = Vector2.zero;
    var rayTranslation = new Vector2(20f, 10f);

    // Slice the target geometry.
    var sliceResult = PhysicsDestructor.Slice(targetGeometry, rayOrigin, rayTranslation, Allocator.Temp);

    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;
    
    // If we have left geometry then add it to a body.
    PhysicsBody leftBody = default;
    if (sliceResult.leftGeometry.IsCreated)
    {
        // Create a body.
        leftBody = world.CreateBody();
        
        // Create the shapes from the geometry.
        using var shapes = leftBody.CreateShapeBatch(sliceResult.leftGeometry, PhysicsShapeDefinition.defaultDefinition);
    }
    
    // If we have right geometry then add it to a body.
    PhysicsBody rightBody = default;
    if (sliceResult.leftGeometry.IsCreated)
    {
        // Create a body.
        rightBody = world.CreateBody();
        
        // Create the shapes from the geometry.
        using var shapes = rightBody.CreateShapeBatch(sliceResult.rightGeometry, PhysicsShapeDefinition.defaultDefinition);
    }
    
    // Dispose of the slice results.
    sliceResult.Dispose();

    // Destroy the body.
    // NOTE: This makes the code useless but is an example so this destroys the body and all its shapes.
    if (leftBody.isValid)
        leftBody.Destroy();
    
    // Destroy the body.
    // NOTE: This makes the code useless but is an example so this destroys the body and all its shapes.
    if (rightBody.isValid)
        rightBody.Destroy();
}
```

## Fragmenting

Fragmenting geometry involves adding fragment points, which define fragment islands—wherever a fragment point overlaps the input geometry, a fragment is produced. When fragmentation is performed, any geometry overlapping the fragment points is returned as "broken" geometry, while any geometry that does not overlap the fragment points is returned as "unbroken" geometry.

Additionally, fragmentation can include a "carving" step, where a geometry "mask" is used to remove specific areas from the input geometry before fragmentation occurs.
Any part of the input geometry not overlapped by the mask is returned as "unbroken" geometry. The fragment points are then applied to the masked area, and any resulting fragments are returned as "broken" geometry.

In the example below, we start by providing a concave or convex outline defined by a set of vertices and convert them into an array of `PolygonGeometry`.
We then fragment this geometry using a set of fragment points, and create both the "broken" and "unbroken" pieces as separate shapes, each attached to its own body:

```csharp
void Run(ReadOnlySpan<Vector2> outlineVertices)
{
    // Create polygon geometries from a concave/convex vertex outline 
    // NOTE: We don't transform or scale it here.
    using var polygonGeometries = PolygonGeometry.CreatePolygons(outlineVertices, PhysicsTransform.identity, Vector2.one);

    // Create the target geometry we want to operate on.
    var targetGeometry = new PhysicsDestructor.FragmentGeometry(PhysicsTransform.identity, polygonGeometries);
    
    // Fragment the target geometry.
    var fragmentResult = PhysicsDestructor.Fragment(targetGeometry, fragmentPoints, Allocator.Temp);

    // Get the main physics world.
    var world = PhysicsWorld.defaultWorld;
    
    // If we have left geometry then add it to a body.
    PhysicsBody unbrokenBody = default;
    if (fragmentResult.unbrokenGeometry.IsCreated)
    {
        // Create a body.
        unbrokenBody = world.CreateBody();
        
        // Create the shapes from the geometry.
        using var shapes = unbrokenBody.CreateShapeBatch(fragmentResult.unbrokenGeometry, PhysicsShapeDefinition.defaultDefinition);
    }
    
    // If we have right geometry then add it to a body.
    PhysicsBody brokenBody = default;
    if (fragmentResult.brokenGeometry.IsCreated)
    {
        // Create a body.
        brokenBody = world.CreateBody();
        
        // Create the shapes from the geometry.
        using var shapes = brokenBody.CreateShapeBatch(fragmentResult.brokenGeometry, PhysicsShapeDefinition.defaultDefinition);
    }
    
    // Dispose of the slice results.
    fragmentResult.Dispose();

    // Destroy the body.
    // NOTE: This makes the code useless but is an example so this destroys the body and all its shapes.
    if (unbrokenBody.isValid)
        unbrokenBody.Destroy();
    
    // Destroy the body.
    // NOTE: This makes the code useless but is an example so this destroys the body and all its shapes.
    if (brokenBody.isValid)
        brokenBody.Destroy();
}
```
