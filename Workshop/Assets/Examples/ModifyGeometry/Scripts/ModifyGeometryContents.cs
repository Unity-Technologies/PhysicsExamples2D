using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Changes the type, size and body type of one shape while it runs, with a box resting on top of it to show the change is felt straight away.
/// The shape is one Physics Area Primitive, which holds a geometry for every shape type and converts its existing shape to whichever type is chosen.
/// </summary>
/// <remarks>
/// Every geometry is authored at a scale of one and sized by the area's scale, which scales the radius too so a circle or capsule grows evenly.
/// Changing the type or scale modifies the existing shape in place rather than destroying it and creating another.
/// </remarks>
public sealed class ModifyGeometryContents : MonoBehaviour
{
    /// <summary>
    /// The shape types the changer can take.
    /// </summary>
    public enum GeometryType
    {
        Circle,
        Capsule,
        Segment,
        Polygon
    }

    /// <summary>
    /// How the changer's body moves: Static never moves, Kinematic moves only as it is told to, and Dynamic falls and is pushed around.
    /// </summary>
    public PhysicsBody.BodyType bodyType
    {
        get => m_BodyType;
        set
        {
            m_BodyType = value;

            if (m_Changer == null)
                return;

            var pose = m_Changer.pose;
            var definition = pose.definition;
            definition.type = m_BodyType;
            pose.definition = definition;

            pose.ApplyDefinition();
        }
    }

    /// <summary>
    /// The shape type the changer uses.
    /// </summary>
    public GeometryType geometryType
    {
        get => m_GeometryType;
        set
        {
            m_GeometryType = value;
            UpdateShape();
        }
    }

    /// <summary>
    /// How large the changer is compared to its authored geometry.
    /// </summary>
    public float geometryScale
    {
        get => m_GeometryScale;
        set
        {
            m_GeometryScale = value;
            UpdateShape();
        }
    }

    // Writes the current type and scale onto the area and applies them to its existing shape.
    private void UpdateShape()
    {
        if (m_Changer == null)
            return;

        m_Changer.shapeType = m_GeometryType switch
        {
            GeometryType.Circle => PhysicsShape.ShapeType.Circle,
            GeometryType.Capsule => PhysicsShape.ShapeType.Capsule,
            GeometryType.Segment => PhysicsShape.ShapeType.Segment,
            _ => PhysicsShape.ShapeType.Polygon
        };

        var transformation = m_Changer.transformation;
        transformation.scale = m_GeometryScale;
        m_Changer.transformation = transformation;

        m_Changer.ApplyGeometry();
    }

    #region Internal

    [SerializeField] PhysicsAreaPrimitive m_Changer;
    [SerializeField] PhysicsBody.BodyType m_BodyType = PhysicsBody.BodyType.Kinematic;
    [SerializeField] GeometryType m_GeometryType = GeometryType.Circle;
    [SerializeField, Range(0.5f, 10f)] float m_GeometryScale = 1f;

    #endregion
}
