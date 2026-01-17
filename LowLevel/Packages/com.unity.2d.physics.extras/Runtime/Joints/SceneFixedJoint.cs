using UnityEngine;
using Unity.U2D.Physics;

namespace Unity.U2D.Physics.Extras
{
    [AddComponentMenu("Physics 2D/LowLevel/Joints/Scene Fixed Joint", 40)]
    [Icon(IconUtility.IconPath + "SceneFixedJoint.png")]
    public sealed class SceneFixedJoint : SceneJointBase, IWorldSceneDrawable
    {
        public PhysicsFixedJointDefinition JointDefinition = PhysicsFixedJointDefinition.defaultDefinition;

        private PhysicsFixedJoint m_Joint;

        protected override void CreateJoint()
        {
            DestroyJoint();

            // Validate.
            if (!BodyA || !BodyA.Body.isValid || !BodyB || !BodyB.Body.isValid)
                return;

            // Set the definition.
            JointDefinition.bodyA = BodyA.Body;
            JointDefinition.bodyB = BodyB.Body;

            // Create the joint.
            m_Joint = PhysicsFixedJoint.Create(BodyA.Body.world, JointDefinition);
            if (m_Joint.isValid)
            {
                m_Joint.userData = UserData;
                m_Joint.callbackTarget = CallbackTarget;
                m_OwnerKey = m_Joint.SetOwner(this);
            }
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