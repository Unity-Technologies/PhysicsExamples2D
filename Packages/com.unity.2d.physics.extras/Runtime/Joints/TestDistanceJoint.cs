using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif

namespace Unity.U2D.Physics.Extras
{
    [AddComponentMenu("Physics 2D/LowLevel/Joints/Test Distance Joint", 40)]
    [Icon(IconUtility.IconPath + "TestDistanceJoint.png")]
    [MovedFrom(autoUpdateAPI: APIUpdates.AutoUpdateAPI, sourceNamespace: APIUpdates.RuntimeSourceNamespace, sourceClassName: "SceneDistanceJoint")]
    public sealed class TestDistanceJoint : TestJointBase, IWorldDrawable
    {
        public PhysicsDistanceJointDefinition JointDefinition = PhysicsDistanceJointDefinition.defaultDefinition;
        public bool AutoDistance;
        
        public PhysicsDistanceJoint joint => m_Joint;
        private PhysicsDistanceJoint m_Joint;

        protected override void CreateJoint()
        {
            DestroyJoint();
            
            // Validate.
            if (!enabled || !BodyA || !BodyA.body.isValid || !BodyB || !BodyB.body.isValid)
                return;

            // Warning if the same body.
            if (BodyA == BodyB)
            {
                Debug.LogWarning($"{name}.{GetType().Name} has '{nameof(BodyA)}' and '{nameof(BodyB)}' both referring to the same body. No joint was created.");
                return;
            }

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
            if (jointDef.minDistanceLimit > jointDef.maxDistanceLimit)
                jointDef.minDistanceLimit = jointDef.maxDistanceLimit;

            if (jointDef.springLowerForce > jointDef.springUpperForce)
                jointDef.springLowerForce = jointDef.springUpperForce;

            // Calculate the automatic distance.
            if (AutoDistance)
                jointDef.distance = (jointDef.bodyA.transform.MultiplyTransform(jointDef.localAnchorA).position - jointDef.bodyB.transform.MultiplyTransform(jointDef.localAnchorB).position).magnitude;
            
            // Create the joint.
            m_Joint = PhysicsDistanceJoint.Create(BodyA.body.world, jointDef);
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