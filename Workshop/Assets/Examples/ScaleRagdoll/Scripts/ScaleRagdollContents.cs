using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Drops one ragdoll into a closed room and resizes it while it moves, to show how the solver copes when every limb is suddenly forced to overlap its neighbors.
/// The ragdoll is the shared ragdoll prefab of Physics Pose, Physics Area and Physics Constraint Hinge components, resized through those components rather than rebuilt.
/// </summary>
/// <remarks>
/// The doll is built at the starting scale, with its joint friction scaled in proportion to that size.
/// Resizing it afterwards moves every bone toward or away from the hip, scales each joint's anchors, resizes each shape, and scales the joint friction with the cube of the change since it was built.
/// </remarks>
public sealed class ScaleRagdollContents : MonoBehaviour
{
    /// <summary>
    /// How large the ragdoll is compared with its authored size.
    /// Changing this resizes the ragdoll where it is, without rebuilding it.
    /// </summary>
    public float ragdollScale
    {
        get => m_RagdollScale;
        set
        {
            m_RagdollScale = value;
            Rescale(m_RagdollScale);
        }
    }

    private void Start()
    {
        if (m_RagdollPrefab == null)
            return;

        var spawned = Instantiate(m_RagdollPrefab, SpawnPosition, Quaternion.identity);

        // A doll's limbs rest against each other, so its shapes share a negative group, which stops them touching and fighting the hinges.
        foreach (var area in spawned.GetComponentsInChildren<PhysicsArea>(true))
        {
            var definition = area.definition;
            var contactFilter = definition.contactFilter;
            contactFilter.groupIndex = GroupIndex;
            definition.contactFilter = contactFilter;
            area.definition = definition;
        }

        // The doll is built at the starting scale while it is still inactive, so every bone, joint and shape is created at that size rather than resized afterwards.
        // Every bone is kept with its authored joint anchors and friction, which each resize is measured from.
        m_StartScale = m_CurrentScale = m_RagdollScale;

        foreach (var pose in spawned.GetComponentsInChildren<PhysicsPose>(true))
        {
            var bone = new Bone { pose = pose, hinge = pose.GetComponent<PhysicsConstraintHinge>(), areas = pose.GetComponents<PhysicsArea>() };

            pose.transform.localPosition *= m_StartScale;

            // The joint friction is scaled in proportion to the starting size.
            if (bone.hinge != null)
            {
                var definition = bone.hinge.definition;
                bone.anchorA = definition.localAnchorA.position;
                bone.anchorB = definition.localAnchorB.position;
                bone.maxMotorTorque = definition.maxMotorTorque;

                definition.localAnchorA = new PhysicsTransform(bone.anchorA * m_StartScale, definition.localAnchorA.rotation);
                definition.localAnchorB = new PhysicsTransform(bone.anchorB * m_StartScale, definition.localAnchorB.rotation);
                definition.maxMotorTorque = bone.maxMotorTorque * m_StartScale;
                bone.hinge.definition = definition;
            }

            foreach (var area in bone.areas)
            {
                var transformation = area.transformation;
                transformation.scale = m_StartScale;
                area.transformation = transformation;
            }

            if (pose.name == HipName)
                m_Hip = pose;

            if (pose.name == TorsoName)
                m_Torso = pose;

            m_Bones.Add(bone);
        }

        spawned.SetActive(true);

        // The torso is started spinning so the doll lands in a heap rather than on its feet.
        if (m_Torso != null)
            m_Torso.body.ApplyAngularImpulse(AngularImpulse * m_RagdollScale);
    }

    // Resizes the running ragdoll about its hip, forcing its limbs to overlap or pull apart before the solver separates them again.
    private void Rescale(float newScale)
    {
        if (m_Hip == null || newScale <= 0f || m_CurrentScale <= 0f)
            return;

        // Positions move by the change since the last resize, anchors and shapes are set from their authored size, and friction follows the cube of the change since the doll was built.
        var scaleRatio = newScale / m_CurrentScale;
        var startRatio = newScale / m_StartScale;
        var origin = m_Hip.body.position;

        foreach (var bone in m_Bones)
        {
            var body = bone.pose.body;

            // The hip stays where it is and every other bone moves toward or away from it, keeping its angle.
            if (bone.pose != m_Hip)
            {
                var transform = body.transform;
                transform.position = origin + (transform.position - origin) * scaleRatio;
                body.transform = transform;
            }

            if (bone.hinge != null)
            {
                var definition = bone.hinge.definition;
                definition.localAnchorA = new PhysicsTransform(bone.anchorA * newScale, definition.localAnchorA.rotation);
                definition.localAnchorB = new PhysicsTransform(bone.anchorB * newScale, definition.localAnchorB.rotation);
                definition.maxMotorTorque = bone.maxMotorTorque * startRatio * startRatio * startRatio;
                bone.hinge.definition = definition;

                bone.hinge.ApplyDefinition();
            }

            foreach (var area in bone.areas)
            {
                var transformation = area.transformation;
                transformation.scale = newScale;
                area.transformation = transformation;

                area.ApplyGeometry();
            }

            body.ApplyMassFromShapes();
        }

        m_CurrentScale = newScale;
    }

    #region Internal

    // One bone of the ragdoll, with the joint anchors and friction it was authored with.
    struct Bone
    {
        public PhysicsPose pose;
        public PhysicsConstraintHinge hinge;
        public PhysicsArea[] areas;
        public Vector2 anchorA;
        public Vector2 anchorB;
        public float maxMotorTorque;
    }

    // The bones the resize is centered on and the starting spin is given to, found by their names in the prefab.
    const string HipName = "Hip";
    const string TorsoName = "Torso";

    // Where the doll starts, the spin given to its torso at a scale of one, and the group its shapes share.
    static readonly Vector3 SpawnPosition = new(0f, 5f, 0f);
    const float AngularImpulse = 10f;
    const int GroupIndex = -1;

    [SerializeField] GameObject m_RagdollPrefab;
    [SerializeField, Range(0.5f, 10f)] float m_RagdollScale = 3f;

    // The bones of the doll, the two found by name, the scale it was built at, and the scale it is at now.
    readonly List<Bone> m_Bones = new();
    PhysicsPose m_Hip;
    PhysicsPose m_Torso;
    float m_StartScale;
    float m_CurrentScale;

    #endregion
}
