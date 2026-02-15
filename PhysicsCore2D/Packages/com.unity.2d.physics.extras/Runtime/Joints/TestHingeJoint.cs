using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [AddComponentMenu("Physics 2D/CoreExamples/Test Hinge Joint", 40)]
    [Icon(IconUtility.IconPath + "TestHingeJoint.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, "SceneHingeJoint")]
    public sealed class TestHingeJoint : TestJointBase, IWorldDrawable
    {
        public PhysicsHingeJointDefinition JointDefinition = PhysicsHingeJointDefinition.defaultDefinition;

        private PhysicsHingeJoint m_Joint;

        protected override void CreateJoint()
        {
            DestroyJoint();

            // Validate.
            if (!BodyA || !BodyA.body.isValid || !BodyB || !BodyB.body.isValid)
                return;

            // Set the definition.
            JointDefinition.bodyA = BodyA.body;
            JointDefinition.bodyB = BodyB.body;

            // Clamp the limits.
            if (JointDefinition.lowerAngleLimit > JointDefinition.upperAngleLimit)
                JointDefinition.lowerAngleLimit = JointDefinition.upperAngleLimit;

            // Create the joint.
            m_Joint = PhysicsHingeJoint.Create(BodyA.body.world, JointDefinition);
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