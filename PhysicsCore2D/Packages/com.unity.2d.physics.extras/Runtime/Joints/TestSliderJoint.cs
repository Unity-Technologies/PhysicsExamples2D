using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.U2D.Physics.Extras
{
    [AddComponentMenu("Physics 2D/CoreExamples/Test Slider Joint", 40)]
    [Icon(IconUtility.IconPath + "TestSliderJoint.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace)]
    public sealed class TestSliderJoint : TestJointBase, IWorldDrawable
    {
        public PhysicsSliderJointDefinition JointDefinition = PhysicsSliderJointDefinition.defaultDefinition;

        private PhysicsSliderJoint m_Joint;

        protected override void CreateJoint()
        {
            DestroyJoint();

            // Validate.
            if (!BodyA || !BodyA.body.isValid || !BodyB || !BodyB.body.isValid)
                return;

            // Fetch the joint definition.
            // NOTE: We do this as we don't want to modify the user authored definition.
            var jointDef = JointDefinition;
            
            // Set the definition.
            jointDef.bodyA = BodyA.body;
            jointDef.bodyB = BodyB.body;

            // Calculate auto anchors.
            {
                var autoAnchorTransform = new AutoAnchorTransform
                {
                    bodyTransformA = jointDef.bodyA.transform,
                    bodyTransformB = jointDef.bodyB.transform,
                    localAnchorA = jointDef.localAnchorA,
                    localAnchorB = jointDef.localAnchorB
                };
                CalculateAutoAnchors(ref autoAnchorTransform);
                jointDef.localAnchorA = autoAnchorTransform.localAnchorA;
                jointDef.localAnchorB = autoAnchorTransform.localAnchorB;
            }

            // Clamp the limits.
            if (jointDef.lowerTranslationLimit > jointDef.upperTranslationLimit)
                jointDef.lowerTranslationLimit = jointDef.upperTranslationLimit;

            // Create the joint.
            m_Joint = PhysicsSliderJoint.Create(BodyA.body.world, jointDef);
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