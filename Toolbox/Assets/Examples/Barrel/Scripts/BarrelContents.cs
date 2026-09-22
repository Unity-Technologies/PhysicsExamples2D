using System.Collections.Generic;

using UnityEngine;
using Unity.U2D.Physics;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Fills the barrel with a large grid of falling objects, stress-testing the solver.
/// The objects are ordinary Physics Pose and Physics Area components, instantiated from one prefab per object type.
/// </summary>
public sealed class BarrelContents : MonoBehaviour
{
    /// <summary>
    /// The kinds of object the barrel can be filled with.
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// Every object is a circle of a random size.
        /// </summary>
        Circle,

        /// <summary>
        /// Every object is a capsule of a random length and radius.
        /// </summary>
        Capsule,

        /// <summary>
        /// Every object is a random polygon.
        /// </summary>
        Polygon,

        /// <summary>
        /// The columns cycle through circles, capsules and polygons.
        /// </summary>
        Mix,

        /// <summary>
        /// Every object carries two polygons, so each body has more than one shape.
        /// </summary>
        Compound,

        /// <summary>
        /// Every object is a ragdoll of eleven bodies held together by hinges, at a random size.
        /// </summary>
        Ragdoll
    }

    /// <summary>
    /// Destroys whatever is in the barrel and fills it again from the current settings.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        var random = new Random(RandomSeed);

        var columnCount = m_ObjectType switch
        {
            ObjectType.Compound => CompoundColumns,
            ObjectType.Ragdoll => RagdollColumns,
            _ => MaxColumns
        };

        var rowCount = m_ObjectType == ObjectType.Ragdoll ? RagdollRows : MaxRows;

        var shift = m_ObjectType switch
        {
            ObjectType.Compound => CompoundShift,
            ObjectType.Ragdoll => RagdollShift,
            _ => Shift
        };

        var extraY = m_ObjectType switch
        {
            ObjectType.Compound => CompoundExtraY,
            ObjectType.Ragdoll => RagdollExtraY,
            _ => ExtraY
        };

        var side = m_ObjectType switch
        {
            ObjectType.Compound => CompoundSide,
            ObjectType.Ragdoll => RagdollSide,
            _ => Side
        };

        // Centered on the barrel: the last column sits as far from the right wall as the first does from the left.
        var centerX = shift * (columnCount - 1) * 0.5f;

        var centerY = shift * 0.5f;
        var startHeight = m_ObjectType == ObjectType.Ragdoll ? RagdollStartHeight : StartHeight;

        m_Spawned.Capacity = Mathf.Max(m_Spawned.Capacity, columnCount * rowCount);

        for (var column = 0; column < columnCount; ++column)
        {
            var x = column * shift - centerX;

            for (var row = 0; row < rowCount; ++row)
            {
                var y = row * (shift + extraY) + centerY + startHeight;

                Spawn(column, new Vector2(x + side, y), ref random);

                side = -side;
            }
        }
    }

    /// <summary>
    /// Destroys every object currently in the barrel, leaving the barrel itself alone.
    /// </summary>
    public void Clear()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
    }

    /// <summary>
    /// The kind of object the barrel is filled with.
    /// Changing this does not refill the barrel until <see cref="Rebuild"/> is called.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    /// <summary>
    /// How fast two objects must approach before the contact counts as a collision, in meters per second.
    /// Changing this does not refill the barrel until <see cref="Rebuild"/> is called.
    /// </summary>
    public float collisionThreshold
    {
        get => m_CollisionThreshold;
        set => m_CollisionThreshold = value;
    }

    private void Start() => Rebuild();

    // Creates one object of the type this column calls for.
    // The prefabs are stored inactive, so the instance is configured before it is enabled: its body and shapes are then built once, already correct, rather than built and immediately rebuilt.
    // Each object is also its own root, because one hierarchy holding thousands of transforms is a single indivisible unit of transform work that the physics write-back cannot spread across threads.
    private void Spawn(int column, Vector2 position, ref Random random)
    {
        var resolvedType = m_ObjectType == ObjectType.Mix
            ? (column % 3) switch
            {
                0 => ObjectType.Circle,
                1 => ObjectType.Capsule,
                _ => ObjectType.Polygon
            }
            : m_ObjectType;

        var prefab = resolvedType switch
        {
            ObjectType.Circle => m_CirclePrefab,
            ObjectType.Capsule => m_CapsulePrefab,
            ObjectType.Polygon => m_PolygonPrefab,
            ObjectType.Compound => m_CompoundPrefab,
            ObjectType.Ragdoll => m_RagdollPrefab,
            _ => m_CirclePrefab
        };

        if (prefab == null)
            return;

        var spawned = Instantiate(prefab, new Vector3(position.x, position.y, 0f), Quaternion.identity);

        if (resolvedType == ObjectType.Ragdoll)
        {
            SetRagdollScale(spawned, random.NextFloat(RagdollMinScale, RagdollMaxScale), m_Spawned.Count);
            SetRagdollArmPose(spawned, ref random);
        }
        else
        {
            SetBody(spawned);
            SetGeometry(spawned, resolvedType, ref random);
        }

        spawned.SetActive(true);

        m_Spawned.Add(spawned);
    }

    // Swings both arms forward, one further than the other, so a ragdoll starts in a pose that reads as a body rather than as a standing figure.
    // Swinging them opposite ways instead just crosses them through each other, since both hang from the same place.
    private static void SetRagdollArmPose(GameObject spawned, ref Random random)
    {
        var spread = random.NextFloat(ArmSpreadMinDegrees, ArmSpreadMaxDegrees);

        SetArmAngle(spawned, "UpperLeftArm", "LowerLeftArm", spread);
        SetArmAngle(spawned, "UpperRightArm", "LowerRightArm", spread * ArmSpreadFarFactor);
    }

    // Rotates a whole arm about the shoulder rather than about each bone's own center.
    // The joint anchors are body-local points aimed at the shoulder and elbow, so turning the arm rigidly about the shoulder leaves both pairs of anchors still meeting in the same place.
    private static void SetArmAngle(GameObject spawned, string upperName, string lowerName, float degrees)
    {
        var rotation = Quaternion.Euler(0f, 0f, degrees);

        SetBoneAngle(spawned.transform.Find(upperName), rotation, degrees);
        SetBoneAngle(spawned.transform.Find(lowerName), rotation, degrees);
    }

    // Turns one bone about the shoulder, keeping its distance from it.
    private static void SetBoneAngle(Transform bone, Quaternion rotation, float degrees)
    {
        if (bone == null)
            return;

        var shoulder = new Vector3(0f, ShoulderHeight, 0f);

        bone.localPosition = shoulder + rotation * (bone.localPosition - shoulder);
        bone.localRotation = Quaternion.Euler(0f, 0f, degrees);
    }

    // Sizes a ragdoll, which is authored at a scale of one, and stops it colliding with itself.
    // Scaling the root carries the bone placements and their shapes, because an area builds its geometry through the transform, but a joint anchor is a body-local value the transform never reaches, so every anchor is scaled here to match.
    // Joint friction scales with the doll, so a large one resists at its joints as much as a small one does in proportion.
    private void SetRagdollScale(GameObject spawned, float scale, int dollIndex)
    {
        spawned.transform.localScale = new Vector3(scale, scale, 1f);

        foreach (var hinge in spawned.GetComponentsInChildren<PhysicsConstraintHinge>(true))
        {
            var definition = hinge.definition;

            var anchorA = definition.localAnchorA;
            var anchorB = definition.localAnchorB;
            anchorA.position *= scale;
            anchorB.position *= scale;

            definition.localAnchorA = anchorA;
            definition.localAnchorB = anchorB;

            // The prefab carries each joint's own share of the friction, so scaling it here keeps the proportions between the joints.
            definition.maxMotorTorque *= scale;

            hinge.definition = definition;
        }

        // A doll's limbs rest against each other, so without this they shove each other apart and fight the hinges.
        // The group is negative, which means shapes sharing it never touch, and unique per doll so dolls still collide with one another.
        var groupIndex = -(dollIndex + 1);

        foreach (var area in spawned.GetComponentsInChildren<PhysicsArea>(true))
        {
            var definition = area.definition;
            var contactFilter = definition.contactFilter;
            contactFilter.groupIndex = groupIndex;
            definition.contactFilter = contactFilter;
            area.definition = definition;
        }
    }

    // Sets the body values that come from the menu rather than from the prefab: the collision threshold, and the angular damping a mixed barrel uses.
    private void SetBody(GameObject spawned)
    {
        var pose = spawned.GetComponent<PhysicsPose>();

        if (pose == null)
            return;

        var definition = pose.definition;
        definition.collisionThreshold = m_CollisionThreshold;
        definition.angularDamping = m_ObjectType == ObjectType.Mix ? MixAngularDamping : 0f;
        pose.definition = definition;
    }

    // Sizes the object, since every circle, capsule and polygon in the barrel is a different size.
    // A compound object keeps the two polygons authored on its prefab.
    private static void SetGeometry(GameObject spawned, ObjectType resolvedType, ref Random random)
    {
        switch (resolvedType)
        {
            case ObjectType.Circle:
            {
                var area = spawned.GetComponent<PhysicsAreaCircle>();
                area.geometry = new CircleGeometry { center = Vector2.zero, radius = random.NextFloat(0.25f, 0.75f) };

                return;
            }

            case ObjectType.Capsule:
            {
                var length = random.NextFloat(0.25f, 1.0f);
                var area = spawned.GetComponent<PhysicsAreaCapsule>();
                area.geometry = new CapsuleGeometry
                {
                    center1 = new Vector2(0f, -0.5f * length),
                    center2 = new Vector2(0f, 0.5f * length),
                    radius = random.NextFloat(0.25f, 0.5f)
                };

                return;
            }

            case ObjectType.Polygon:
            {
                var radius = 0.25f * random.NextFloat(0f, 1.0f);
                var area = spawned.GetComponent<PhysicsAreaPolygon>();
                area.geometry = ToolboxUtility.CreateRandomPolygon(extent: 0.75f, radius: radius, ref random);

                return;
            }
        }
    }

    #region Internal

    // The grid layout, matching the Sandbox example this reproduces.
    // A compound barrel uses fewer, wider spaced columns because each object is larger.
    const int MaxRows = 150;
    const int MaxColumns = 26;
    const int CompoundColumns = 20;
    const float Shift = 1.15f;
    const float CompoundShift = 2.0f;
    const float ExtraY = 0.5f;
    const float CompoundExtraY = 0.25f;
    const float Side = -0.1f;
    const float CompoundSide = 0.25f;
    const float StartHeight = 120f;

    // A ragdoll barrel uses far fewer, far wider spaced columns and starts much lower, because each doll is five to nine times the size of a single object.
    // Spaced so that even the largest ragdoll, 3.96 wide and 23.65 tall, clears its neighbors on both axes.
    // The side alternation brings neighboring columns to within a shift of two, so the shift carries that as well as the width.
    const int RagdollRows = 15;
    const int RagdollColumns = 9;
    const float RagdollShift = 8f;
    const float RagdollExtraY = 16f;
    const float RagdollSide = 1.0f;
    const float RagdollStartHeight = 25f;
    const float RagdollMinScale = 10f;
    const float RagdollMaxScale = 18f;

    // The shoulder each arm turns about, in the ragdoll's own unscaled space, and how far forward the two arms start.
    // The far arm swings twice as far as the near one, so the pair reads as two arms rather than one.
    const float ShoulderHeight = 1.35f;
    const float ArmSpreadMinDegrees = 10f;
    const float ArmSpreadMaxDegrees = 15f;
    const float ArmSpreadFarFactor = 2f;
    const float MixAngularDamping = 0.3f;
    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField] GameObject m_CompoundPrefab;
    [SerializeField] GameObject m_RagdollPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Circle;
    [SerializeField, Range(0f, 1f)] float m_CollisionThreshold = 0.5f;

    readonly List<GameObject> m_Spawned = new();

    #endregion
}
