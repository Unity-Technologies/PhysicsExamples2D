﻿using UnityEngine.LowLevelPhysics2D;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [AddComponentMenu("Physics 2D (LowLevel)/Joints/Scene WheelJoint", 6)]
    [Icon(IconUtility.IconPath + "SceneWheelJoint.png")]
    public sealed class SceneWheelJoint : SceneJointBase, IWorldSceneDrawable
    {
        public PhysicsWheelJointDefinition JointDefinition = PhysicsWheelJointDefinition.defaultDefinition;
        
        private PhysicsWheelJoint m_Joint;

        protected override  void CreateJoint()
        {
            DestroyJoint();
            
            // Validate.
            if (!BodyA || !BodyA.Body.isValid || !BodyB || !BodyB.Body.isValid)
                return;
            
            // Set the definition.
            JointDefinition.bodyA = BodyA.Body;
            JointDefinition.bodyB = BodyB.Body;
            
            // Clamp the limits.
            if (JointDefinition.lowerTranslationLimit > JointDefinition.upperTranslationLimit)
                JointDefinition.lowerTranslationLimit = JointDefinition.upperTranslationLimit;
            
            // Create the joint.
            m_Joint = PhysicsWheelJoint.Create(BodyA.Body.world, JointDefinition);
            if (m_Joint.isValid)
            {
                m_Joint.userData = UserData;
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