using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomCollider2D_Logo : MonoBehaviour
{
    private HingeJoint2D m_Hinge;
    private CustomCollider2D m_CustomCollider;
    readonly PhysicsShapeGroup2D m_ShapeGroup = new PhysicsShapeGroup2D();
    
    // A set of paths that define the Unity logo.
    // NOTE: Paths define the outer edge and the three cut-outs.
    readonly List<Vector2>[] m_UnityLogo = {
        new List<Vector2> { new Vector2(1.693262f, -2.305816f), new Vector2(2.196873f, -0.4644678f), new Vector2(1.944872f, -0.0019431f), new Vector2(2.198257f, 0.4264483f), new Vector2(1.683161f, 2.265592f), new Vector2(-0.2132433f, 1.764162f), new Vector2(-0.4694808f, 1.32f), new Vector2(-1.02455f, 1.315517f), new Vector2(-2.377048f, -0.0425735f), new Vector2(-1.013685f, -1.36f), new Vector2(-0.4553221f, -1.367743f), new Vector2(-0.1637628f, -1.841559f), new Vector2(1.693262f, -2.305816f), },
        new List<Vector2> { new Vector2(-1.341954f, 0.2576635f), new Vector2(-0.385737f, 1.200969f), new Vector2(0.8813718f, 1.488948f), new Vector2(0.904642f, 1.45706f), new Vector2(0.1709107f, 0.2299421f), new Vector2(-1.326493f, 0.2204046f), new Vector2(-1.341954f, 0.2576635f), },
        new List<Vector2> { new Vector2(1.38413f, -1.289442f), new Vector2(0.6348394f, -0.0018103f), new Vector2(1.384505f, 1.266855f), new Vector2(1.423719f, 1.2618f), new Vector2(1.778864f, 0.005874f), new Vector2(1.423529f, -1.284214f), new Vector2(1.38413f, -1.289442f), },
        new List<Vector2> { new Vector2(0.8818537f, -1.529058f), new Vector2(-0.3930915f, -1.23697f), new Vector2(-1.35149f, -0.2977395f), new Vector2(-1.336082f, -0.2603412f), new Vector2(0.1946112f, -0.2779137f), new Vector2(0.9051427f, -1.49734f), new Vector2(0.8818537f, -1.529058f), }
    };
    
    void Start()
    {
        // Fetch our references.
        m_Hinge = GetComponent<HingeJoint2D>();
        m_CustomCollider = GetComponent<CustomCollider2D>();
        
        // Start with no shapes.
        m_ShapeGroup.Clear();

        // Iterate all the paths for our logo.
        foreach (var path in m_UnityLogo)
        {
            // Add the whole path as a set of edges.
            m_ShapeGroup.AddEdges(path, 0.01f);

            // Iterate each vertex in the path adding a Circle to each to
            // demonstrate the compound nature of the PhysicsShapeGroup2D and CustomCollider2D i.e. mixed shape types.
            foreach (var vertex in path)
            {
                m_ShapeGroup.AddCircle(vertex, 0.1f);
            }
        }

        // Assign the shape group to the CustomCollider2D.
        m_CustomCollider.SetCustomShapes(m_ShapeGroup);
        
        // Periodically perform a random update.
        InvokeRepeating(nameof(SetRandomMotor), 1f, 3f);
    }

    //
    void SetRandomMotor()
    {
        // Choose a random motor speed.
        m_Hinge.motor = new JointMotor2D
        {
            maxMotorTorque = 10000f,
            motorSpeed = Random.Range(-100f, 100f)
        };
    }
}
