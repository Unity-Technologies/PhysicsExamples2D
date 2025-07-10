using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Serialization;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [AddComponentMenu("Physics 2D (LowLevel)/Joints/Scene HingeJoint", 1)]
    [Icon(IconUtility.IconPath + "SceneHingeJoint.png")]
    public sealed class SceneHingeJoint : SceneJointBase, IWorldSceneDrawable
    {
        [FormerlySerializedAs("Definition")] public PhysicsHingeJointDefinition JointDefinition = PhysicsHingeJointDefinition.defaultDefinition;
        
        private PhysicsHingeJoint m_Joint;

        protected override  void CreateJoint()
        {
            DestroyJoint();
            
            // Fetch the world.
            var world  = SceneWorld == null ? PhysicsWorld.defaultWorld : SceneWorld.World;
            if (!world.isValid)
                return;
            
            // Validate.
            if (!BodyA || !BodyA.Body.isValid || !BodyB || !BodyB.Body.isValid)
                return;
            
            // Set the definition.
            JointDefinition.bodyA = BodyA.Body;
            JointDefinition.bodyB = BodyB.Body;

            // Clamp the limits.
            if (JointDefinition.lowerAngleLimit > JointDefinition.upperAngleLimit)
                JointDefinition.lowerAngleLimit = JointDefinition.upperAngleLimit;
            
            // Create the joint.
            m_Joint = PhysicsHingeJoint.Create(world, JointDefinition);
            if (m_Joint.isValid)
            {
                m_Joint.callbackTarget = CallbackTarget;
                m_OwnerKey = m_Joint.SetOwner(this);
            }
        }

        protected override  void DestroyJoint()
        {
            // Destroy the joint.
            if (m_Joint.isValid)
            {
                m_Joint.Destroy(m_OwnerKey);
                m_Joint = default;
                m_OwnerKey = 0;
            }
        }

        // Draw the joint.
        public void Draw()
        {
            // Draw if we're drawing selections.
            if (m_Joint.isValid &&
                m_Joint.world.drawOptions.HasFlag(PhysicsWorld.DrawOptions.SelectedJoints))
                m_Joint.Draw();
        }
    }

}