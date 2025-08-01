﻿using UnityEngine.LowLevelPhysics2D;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [AddComponentMenu("Physics 2D (LowLevel)/Joints/Scene RelativeJoint", 4)]
    [Icon(IconUtility.IconPath + "SceneRelativeJoint.png")]
    public sealed class SceneRelativeJoint : SceneJointBase, IWorldSceneDrawable
    {
        public PhysicsRelativeJointDefinition JointDefinition = PhysicsRelativeJointDefinition.defaultDefinition;
        
        private PhysicsRelativeJoint m_Joint;

        protected override  void CreateJoint()
        {
            DestroyJoint();
            
            // Validate.
            if (!BodyA || !BodyA.Body.isValid || !BodyB || !BodyB.Body.isValid)
                return;
            
            // Set the definition.
            JointDefinition.bodyA = BodyA.Body;
            JointDefinition.bodyB = BodyB.Body;
            
            // Create the joint.
            m_Joint = PhysicsRelativeJoint.Create(BodyA.Body.world, JointDefinition);
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