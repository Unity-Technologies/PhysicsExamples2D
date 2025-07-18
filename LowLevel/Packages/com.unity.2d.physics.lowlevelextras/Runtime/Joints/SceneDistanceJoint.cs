﻿using UnityEngine.LowLevelPhysics2D;

namespace UnityEngine.U2D.Physics.LowLevelExtras
{
    [AddComponentMenu("Physics 2D (LowLevel)/Joints/Scene DistanceJoint")]
    [Icon(IconUtility.IconPath + "SceneDistanceJoint.png")]
    public sealed class SceneDistanceJoint : SceneJointBase, IWorldSceneDrawable
    {
        public PhysicsDistanceJointDefinition JointDefinition = PhysicsDistanceJointDefinition.defaultDefinition;
        
        private PhysicsDistanceJoint m_Joint;

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
            JointDefinition.bodyA = BodyA.Body;
            JointDefinition.bodyB = BodyB.Body;
            
            // Clamp the limits.
            if (JointDefinition.minLengthLimit > JointDefinition.maxLengthLimit)
                JointDefinition.minLengthLimit = JointDefinition.maxLengthLimit;
            
            // Create the joint.
            m_Joint = PhysicsDistanceJoint.Create(world, JointDefinition);
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