using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Serialization;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [AddComponentMenu("Physics 2D (LowLevel)/Joints/Scene IgnoreJoint")]
    [Icon(IconUtility.IconPath + "SceneIgnoreJoint.png")]
    public sealed class SceneIgnoreJoint : SceneJointBase, IWorldSceneDrawable
    {
        public PhysicsIgnoreJointDefinition Definition = PhysicsIgnoreJointDefinition.defaultDefinition;
        
        private PhysicsIgnoreJoint m_Joint;

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
            Definition.bodyA = BodyA.Body;
            Definition.bodyB = BodyB.Body;
            
            // Create the joint.
            m_Joint = PhysicsIgnoreJoint.Create(world, Definition);
            if (m_Joint.isValid)
                m_OwnerKey = m_Joint.SetOwner(this);
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