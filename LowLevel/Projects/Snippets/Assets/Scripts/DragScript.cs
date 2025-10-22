using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;

public class DragScript : MonoBehaviour
{
    public bool UseDefaultWorld = true;
    public SceneWorld SceneWorld;
    public PhysicsQuery.QueryFilter QueryFilter = PhysicsQuery.QueryFilter.Everything;
    
    private Camera m_Camera;
    private PhysicsWorld m_World;
    private PhysicsBody m_DragGroundBody;
    private PhysicsRelativeJoint m_DragJoint;
    private DragMode m_DragMode = DragMode.Off;

    private enum DragMode
    {
        Off,
        Dragging
    }
    
    private void OnEnable()
    {
        m_Camera = Camera.main;
        m_World = UseDefaultWorld || SceneWorld == null ? PhysicsWorld.defaultWorld : SceneWorld.World; 
        m_DragGroundBody = m_World.CreateBody();
    }

    private void OnDisable()
    {
        if (m_DragGroundBody.isValid)
            m_DragGroundBody.Destroy();
    }

    private void Update()
    {
        var worldPosition = m_Camera.ScreenToWorldPoint(Mouse.current.position.value);
        
        if (m_DragMode == DragMode.Off && Mouse.current.leftButton.wasPressedThisFrame)
        {
            using var results = m_World.OverlapPoint(worldPosition, QueryFilter);
            if (results.Length == 0)
                return;

            var hitBody = results[0].shape.body;
            
            var relativeDefinition = new PhysicsRelativeJointDefinition
            {
                bodyA = m_DragGroundBody,
                bodyB = hitBody,
                localAnchorA = new PhysicsTransform(worldPosition),
                localAnchorB = hitBody.GetLocalPoint(worldPosition),
                springLinearFrequency = 7.5f,
                springLinearDamping = 0.7f,
                springMaxForce = 1000f * hitBody.mass * m_World.gravity.magnitude
            };

            m_DragJoint = m_World.CreateJoint(relativeDefinition);
            m_DragJoint.WakeBodies();

            m_DragMode = DragMode.Dragging;
            
            return;
        }

        if (m_DragMode == DragMode.Dragging)
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                m_DragJoint.Destroy();
                m_DragMode = DragMode.Off;
                return;
            }

            // Update the target.
            m_DragJoint.localAnchorA = new PhysicsTransform(worldPosition);
            var bodyB = m_DragJoint.bodyB;
            bodyB.awake = true;
        }
    }
}
