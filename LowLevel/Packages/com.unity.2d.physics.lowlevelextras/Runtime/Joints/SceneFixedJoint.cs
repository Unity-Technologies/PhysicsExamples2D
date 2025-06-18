using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Serialization;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [AddComponentMenu("Physics 2D (LowLevel)/Joints/Scene FixedJoint", 3)]
    [Icon(IconUtility.IconPath + "SceneFixedJoint.png")]
    public sealed class SceneFixedJoint : SceneJointBase, IWorldSceneDrawable
    {
        public PhysicsFixedJointDefinition Definition = PhysicsFixedJointDefinition.defaultDefinition;
        
        private PhysicsFixedJoint m_Joint;

        protected override void CreateJoint()
        {
            DestroyJoint();
            
            // Fetch the world.
            var world = SceneWorld == null ? PhysicsWorld.defaultWorld : SceneWorld.World;
            if (!world.isValid)
                return;
            
            // Validate.
            if (!BodyA || !BodyA.Body.isValid || !BodyB || !BodyB.Body.isValid)
                return;
            
            // Set the definition.
            Definition.bodyA = BodyA.Body;
            Definition.bodyB = BodyB.Body;
            
            // Create the joint.
            m_Joint = PhysicsFixedJoint.Create(world, Definition);
            if (m_Joint.isValid)
                m_OwnerKey = m_Joint.SetOwner(this);
        }

        protected override void DestroyJoint()
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