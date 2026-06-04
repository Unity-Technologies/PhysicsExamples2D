using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

public class TestShadowRegion : MonoBehaviour
{
    public Vector2 Size = Vector2.one;
    public PhysicsQuery.QueryFilter QueryFilter = PhysicsQuery.QueryFilter.Everything; 
    public Color GizmoColor = Color.aliceBlue;

    public bool Overlap(Bounds bounds)
    {
        var polygonGeometry = PolygonGeometry.CreateBox(Size);
        
        var world = PhysicsWorld.defaultWorld;
        var physicsTransform = PhysicsMath.ToPhysicsTransform(transform, world.transformPlane);
        var aabb = polygonGeometry.CalculateAABB(physicsTransform);
        
        var otherAABB = new PhysicsAABB { lowerBound = bounds.min, upperBound = bounds.max };
        return aabb.Overlap(otherAABB);
    }

    public NativeArray<PhysicsQuery.WorldOverlapResult> GetOverlaps(Allocator allocator = Allocator.Temp)
    {
        var world = PhysicsWorld.defaultWorld;
        var physicsTransform = PhysicsMath.ToPhysicsTransform(transform, world.transformPlane);
        var polygonGeometry = PolygonGeometry.CreateBox(Size).Transform(physicsTransform);
        
        return world.OverlapGeometry(polygonGeometry, QueryFilter, allocator);
    }
    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;
        
        var world = PhysicsWorld.defaultWorld;
        var physicsTransform = PhysicsMath.ToPhysicsTransform(transform, world.transformPlane);
        world.DrawBox(physicsTransform, Size, 0f, GizmoColor);
    }

    private void OnValidate()
    {
        if (Size.x < 0.001f)
            Size.x = 0.001f;

        if (Size.y < 0.001f)
            Size.y = 0.001f;
    }
}
