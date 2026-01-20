using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Demonstrates the basics of creating a all shape types with a custom color.
/// Press "Play" to see all the shapes.
/// See the comments for more information.
/// </summary>
public class PhysicsShapeTypes : MonoBehaviour
{
    private PhysicsWorld m_PhysicsWorld;
    
    private void OnEnable()
    {
        // Create a world.
        m_PhysicsWorld = PhysicsWorld.Create();
      
       // Create a body with a Circle shape.
       var body1 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { position = new Vector2(-4f, 0f) });
       body1.CreateShape(
           CircleGeometry.defaultGeometry,
           new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.red } }
       );
       
       // Create a body with a Capsule shape.
       var body2 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { position = new Vector2(-2f, 0f) });
       body2.CreateShape(
           CapsuleGeometry.defaultGeometry,
           new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.lawnGreen } }
       );

       // Create a body with a Polygon shape.
       var body3 = m_PhysicsWorld.CreateBody();
       body3.CreateShape(
           PolygonGeometry.defaultGeometry,
           new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.deepSkyBlue } }
           );
       
       // Create a body with a Segment shape.
       var body4 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { position = new Vector2(2f, 0f) });
       body4.CreateShape(
           SegmentGeometry.defaultGeometry,
           new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.softYellow } }
           );
       
       // Create a body with a Chain (Chain Segment shapes).
       var body5 = m_PhysicsWorld.CreateBody(new PhysicsBodyDefinition { position = new Vector2(4f, 0f) });
       var points = new[]
       {
           new Vector2(-1.0f, -0.5f),
           new Vector2(-0.5f, 1.0f),
           new Vector2(0f, 1.0f),
           new Vector2(0.5f, 0.5f),
           new Vector2(1.0f, -0.5f)
       };
       body5.CreateChain(
           new ChainGeometry(points),
           new PhysicsChainDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.darkOrange }, isLoop = true });
    }
    
    private void OnDisable()
    {
        // Destroying a world will destroy all its contents.
        m_PhysicsWorld.Destroy();
    }
}