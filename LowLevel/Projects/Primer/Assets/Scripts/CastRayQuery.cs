using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

/// <summary>
/// Demonstrates the performing a simple CastRay Query in the world.
/// See the comments for more information.
/// </summary>
public class CastRayQuery : MonoBehaviour
{
    private PhysicsWorld m_PhysicsWorld;
    
    private void OnEnable()
    {
        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create();

        // Create a body with a Circle shape.
        m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { position = Vector2.up * 4f }).
            CreateShape(CircleGeometry.defaultGeometry);
    }

    private void Update()
    {
        // Perform a cast ray query.
        // NOTE: We use "using" here so the results are disposed. We also use the default TEMP allocator.
        using var results = m_PhysicsWorld.CastRay(
            new PhysicsQuery.CastRayInput { origin = Vector2.zero, translation = Vector2.up * 10f },
            PhysicsQuery.QueryFilter.Everything);
    
        // Finish if nothing was detected.
        if (results.Length == 0)
            return;
        
        // Fetch the result.
        var worldCastResult = results[0];
        var hitPoint = worldCastResult.point;
        
        // Draw the result.
        m_PhysicsWorld.DrawLine(Vector2.zero, hitPoint, Color.cornflowerBlue);
        m_PhysicsWorld.DrawPoint(Vector2.zero, 10f, Color.lawnGreen);
        m_PhysicsWorld.DrawPoint(results[0].point, 10f, Color.orange);
    }

    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        m_PhysicsWorld.Destroy();
    }
}