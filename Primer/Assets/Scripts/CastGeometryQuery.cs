using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates the performing a simple CastGeometry Query in the world.
/// See the comments for more information.
/// </summary>
public class CastGeometryQuery : MonoBehaviour
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
        // Create some geometry (a unit polygon box).
        var geometry = PolygonGeometry.CreateBox(Vector2.one);

        // How we want to cast the geometry.
        var castTranslation = Vector2.up * 10f;
        
        // Perform a cast geometry query.
        // NOTE: We use "using" here so the results are disposed. We also use the default TEMP allocator.
        using var results = m_PhysicsWorld.CastGeometry(
            geometry, castTranslation,
            PhysicsQuery.QueryFilter.Everything);
    
        // Finish if nothing was detected.
        if (results.Length == 0)
            return;
        
        // Fetch the result.
        var worldCastResult = results[0];
        var hitPoint = worldCastResult.point;
        var hitCentroid = Vector2.Lerp(Vector2.zero, castTranslation, worldCastResult.fraction);
        
        // Draw the result.
        m_PhysicsWorld.DrawLine(Vector2.zero, hitPoint, Color.cornflowerBlue);
        m_PhysicsWorld.DrawPoint(Vector2.zero, 10f, Color.lawnGreen);
        m_PhysicsWorld.DrawPoint(hitPoint, 10f, Color.softYellow);
        m_PhysicsWorld.DrawGeometry(geometry, Vector2.zero, Color.lawnGreen);
        m_PhysicsWorld.DrawGeometry(geometry, hitCentroid, Color.softYellow);
    }

    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        m_PhysicsWorld.Destroy();
    }
}