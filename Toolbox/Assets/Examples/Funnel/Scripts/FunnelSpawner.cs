using System;

using Unity.U2D.Physics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Drops a steady stream of objects into the funnel, and removes each one again when it reaches the trigger at the bottom.
/// Everything spawned is built from Physics Pose and Physics Area components, with the jointed objects instantiated whole from a prefab.
/// </summary>
/// <remarks>
/// Gravity is scaled up so objects fall through the funnel quickly enough to keep it busy, and the spawn interval is divided by the square root of that scale so a stronger pull does not simply thin the stream out.
/// The world's own gravity is put back when the example unloads.
/// </remarks>
public sealed class FunnelSpawner : MonoBehaviour, PhysicsAreaCallbacks.ITriggerCallback
{
    /// <summary>
    /// What the funnel is fed with.
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// A single circle.
        /// </summary>
        Circle,

        /// <summary>
        /// A single capsule.
        /// </summary>
        Capsule,

        /// <summary>
        /// A single random polygon.
        /// </summary>
        Polygon,

        /// <summary>
        /// One body carrying two polygons.
        /// </summary>
        Compound,

        /// <summary>
        /// Eleven bodies held together by hinges.
        /// </summary>
        Ragdoll,

        /// <summary>
        /// A ring of seven capsules held together by fixed joints, which squashes as it lands.
        /// </summary>
        Softbody,

        /// <summary>
        /// A different one of the above each time.
        /// </summary>
        Random
    }

    /// <summary>
    /// What the funnel is fed with from the next spawn onward.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    /// <summary>
    /// How large each spawned object is, where one is roughly a meter across.
    /// </summary>
    public float objectScale
    {
        get => m_ObjectScale;
        set => m_ObjectScale = value;
    }

    /// <summary>
    /// How long to wait between spawns, in seconds, before the gravity scale is taken into account.
    /// </summary>
    public float spawnPeriod
    {
        get => m_SpawnPeriod;
        set => m_SpawnPeriod = value;
    }

    /// <summary>
    /// How much harder than normal gravity pulls while this example runs.
    /// </summary>
    public float gravityScale
    {
        get => m_GravityScale;
        set
        {
            m_GravityScale = value;

            var world = PhysicsWorld.defaultWorld;
            world.gravity = m_WorldGravity * m_GravityScale;
        }
    }

    // Destroys whatever reached the trigger at the bottom of the funnel.
    // The event names the area that arrived, so the whole object it belongs to is removed, which for a ragdoll or a softbody means the entire jointed assembly rather than the one limb that happened to arrive first.
    void PhysicsAreaCallbacks.ITriggerCallback.OnTriggerBegin2D(PhysicsAreaCallbacks.TriggerBeginEvent beginEvent)
    {
        var visitorArea = beginEvent.visitorArea;

        if (visitorArea == null)
            return;

        // The funnel walls sit on the same body as the trigger, and a body reports its own shapes overlapping each other, so anything static is not something that fell in.
        var visitorPose = visitorArea.pose;

        if (visitorPose == null || visitorPose.definition.type == PhysicsBody.BodyType.Static)
            return;

        Destroy(visitorPose.transform.root.gameObject);
    }

    void PhysicsAreaCallbacks.ITriggerCallback.OnTriggerEnd2D(PhysicsAreaCallbacks.TriggerEndEvent endEvent)
    {
    }

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);
        m_SpawnTime = 0f;

        var world = PhysicsWorld.defaultWorld;
        m_WorldGravity = world.gravity;
        world.gravity = m_WorldGravity * m_GravityScale;
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay scaled up into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;
    }

    private void Update()
    {
        if (PhysicsWorld.defaultWorld.paused)
            return;

        m_SpawnTime -= Time.deltaTime;

        if (m_SpawnTime > 0f)
            return;

        // Stronger gravity gets an object out of the way sooner, so the wait shortens with it and the funnel stays as full as it was.
        m_SpawnTime = m_SpawnPeriod / Mathf.Sqrt(m_GravityScale);

        Spawn();
    }

    // Creates one object above the funnel's mouth, at a random point across it.
    private void Spawn()
    {
        var position = new Vector3(m_Random.NextFloat(-SpawnHalfWidth, SpawnHalfWidth), SpawnHeight, 0f);

        var resolvedType = m_ObjectType;

        if (resolvedType == ObjectType.Random)
        {
            // One of the seven draws spawns nothing at all, which is what thins the stream rather than any of the shapes appearing less often than the others.
            var choice = m_Random.NextInt(0, 7);

            if (choice == 3)
                return;

            resolvedType = choice switch
            {
                0 => ObjectType.Circle,
                1 => ObjectType.Capsule,
                2 => ObjectType.Polygon,
                4 => ObjectType.Compound,
                5 => ObjectType.Ragdoll,
                _ => ObjectType.Softbody
            };
        }

        switch (resolvedType)
        {
            case ObjectType.Circle:
            {
                var spawned = Instantiate(m_CirclePrefab, position, Quaternion.identity);
                spawned.GetComponent<PhysicsAreaCircle>().geometry = new CircleGeometry { center = Vector2.zero, radius = m_ObjectScale * 0.5f };
                spawned.SetActive(true);

                return;
            }

            case ObjectType.Capsule:
            {
                var capsuleScale = m_ObjectScale * 0.5f;

                var spawned = Instantiate(m_CapsulePrefab, position, Quaternion.identity);
                spawned.GetComponent<PhysicsAreaCapsule>().geometry = new CapsuleGeometry
                {
                    center1 = new Vector2(0f, -0.5f * capsuleScale),
                    center2 = new Vector2(0f, 0.5f * capsuleScale),
                    radius = 0.5f * capsuleScale
                };
                spawned.SetActive(true);

                return;
            }

            case ObjectType.Polygon:
            {
                var spawned = Instantiate(m_PolygonPrefab, position, Quaternion.identity);
                spawned.GetComponent<PhysicsAreaPolygon>().geometry = ToolboxUtility.CreateRandomPolygon(extent: m_ObjectScale * 0.5f, radius: m_ObjectScale * 0.25f, ref m_Random);
                spawned.SetActive(true);

                return;
            }

            case ObjectType.Compound:
            {
                var scale = Matrix4x4.Scale(Vector3.one * (m_ObjectScale * 0.5f));

                var spawned = Instantiate(m_CompoundPrefab, position, Quaternion.identity);
                var areas = spawned.GetComponents<PhysicsAreaPolygon>();
                areas[0].geometry = PolygonGeometry.Create(vertices: LeftCompoundVertices.AsSpan()).Transform(scale, false);
                areas[1].geometry = PolygonGeometry.Create(vertices: RightCompoundVertices.AsSpan()).Transform(scale, false);
                spawned.SetActive(true);

                return;
            }

            case ObjectType.Ragdoll:
            {
                var spawned = Instantiate(m_RagdollPrefab, position, Quaternion.identity);
                SetScale(spawned, m_ObjectScale * RagdollScale);
                SetRagdollJoints(spawned);
                SetAreas(spawned, SharedGroupIndex);
                spawned.SetActive(true);

                return;
            }

            case ObjectType.Softbody:
            {
                var spawned = Instantiate(m_DonutPrefab, position, Quaternion.identity);
                SetScale(spawned, m_ObjectScale * 0.5f);
                SetAreas(spawned, SharedGroupIndex);
                spawned.SetActive(true);

                return;
            }
        }
    }

    // Sizes a jointed object, which is authored at a scale of one.
    // Scaling the root carries the shapes, because an area builds its geometry through the transform, but a joint anchor is a body-local value the transform never reaches, so every anchor is scaled here to match.
    private static void SetScale(GameObject spawned, float scale)
    {
        spawned.transform.localScale = new Vector3(scale, scale, 1f);

        foreach (var constraint in spawned.GetComponentsInChildren<PhysicsConstraintHinge>(true))
        {
            var definition = constraint.definition;

            var anchorA = definition.localAnchorA;
            var anchorB = definition.localAnchorB;
            anchorA.position *= scale;
            anchorB.position *= scale;

            definition.localAnchorA = anchorA;
            definition.localAnchorB = anchorB;

            constraint.definition = definition;
        }

        foreach (var constraint in spawned.GetComponentsInChildren<PhysicsConstraintFixed>(true))
        {
            var definition = constraint.definition;

            var anchorA = definition.localAnchorA;
            var anchorB = definition.localAnchorB;
            anchorA.position *= scale;
            anchorB.position *= scale;

            definition.localAnchorA = anchorA;
            definition.localAnchorB = anchorB;

            constraint.definition = definition;
        }
    }

    // Replaces the ragdoll's joint settings with the looser ones this example uses, where a limb swings freely rather than resisting.
    private static void SetRagdollJoints(GameObject spawned)
    {
        foreach (var constraint in spawned.GetComponentsInChildren<PhysicsConstraintHinge>(true))
        {
            var definition = constraint.definition;
            definition.springFrequency = RagdollJointFrequency;
            definition.springDamping = RagdollJointDamping;
            definition.maxMotorTorque = 0f;
            constraint.definition = definition;

            constraint.ApplyDefinition();
        }
    }

    // Tells every shape on a spawned object to report reaching the trigger, and puts them all in one group so their own overlapping parts never push each other apart.
    private static void SetAreas(GameObject spawned, int groupIndex)
    {
        foreach (var area in spawned.GetComponentsInChildren<PhysicsArea>(true))
        {
            var definition = area.definition;
            definition.triggerEvents = true;

            var contactFilter = definition.contactFilter;
            contactFilter.groupIndex = groupIndex;
            definition.contactFilter = contactFilter;

            area.definition = definition;
        }
    }

    #region Internal

    // Where objects come in: how far above the funnel's mouth, and how far either side of the middle.
    const float SpawnHeight = 35f;
    const float SpawnHalfWidth = 15f;

    // How a ragdoll is sized against the other objects, and the joints it hangs on here, which are far looser than the ones holding a ragdoll upright.
    const float RagdollScale = 1.25f;
    const float RagdollJointFrequency = 1f;
    const float RagdollJointDamping = 0.1f;

    // The one group every jointed object shares, negative so the parts of an assembly never touch each other.
    const int SharedGroupIndex = -1;

    const uint RandomSeed = 0x32628473;

    // The two triangles making up a compound object, before it is scaled.
    static readonly Vector2[] LeftCompoundVertices = { new(-1.0f, 0f), new(0.5f, 1f), new(0f, 2f) };
    static readonly Vector2[] RightCompoundVertices = { new(1.0f, 0f), new(-0.5f, 1f), new(0f, 2f) };

    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField] GameObject m_CompoundPrefab;
    [SerializeField] GameObject m_RagdollPrefab;
    [SerializeField] GameObject m_DonutPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Random;
    [SerializeField, Range(1f, 3f)] float m_ObjectScale = 2.5f;
    [SerializeField, Range(0.1f, 1f)] float m_SpawnPeriod = 0.5f;
    [SerializeField, Range(1f, 10f)] float m_GravityScale = 5f;

    Random m_Random;
    Vector2 m_WorldGravity;
    float m_SpawnTime;

    #endregion
}
