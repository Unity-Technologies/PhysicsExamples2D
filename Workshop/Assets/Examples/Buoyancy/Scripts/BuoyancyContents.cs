using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

using Random = Unity.Mathematics.Random;

/// <summary>
/// Drops random shapes into a container of liquid and applies buoyancy to every shape in it before each simulation step, so light shapes float, heavy ones sink, and a flow can push them sideways.
/// The container and the liquid are authored in the scene as Physics Pose and Physics Area components, and every dropped shape is one Physics Pose with a Physics Area from the prefab for its type.
/// </summary>
/// <remarks>
/// The liquid is a trigger area, so it shows the region without anything colliding with it; the buoyancy itself is applied by script, because no component provides it.
/// The shape density, the surface level and every liquid setting act where they are; the shape type, count and scale rebuild the shapes.
/// </remarks>
public sealed class BuoyancyContents : MonoBehaviour
{
    /// <summary>
    /// The shapes that are dropped into the liquid.
    /// </summary>
    public enum ObjectType
    {
        Circle,
        Capsule,
        Polygon,
        Mix
    }

    /// <summary>
    /// Destroys the current shapes and drops a new set with the current type, count and scale.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        var random = new Random(RandomSeed);

        for (var n = 0; n < m_SpawnCount; ++n)
        {
            var position = new Vector3(random.NextFloat(-InnerHalfWidth + 1f, InnerHalfWidth - 1f), random.NextFloat(m_SurfaceLevel + 2f, m_SurfaceLevel + 30f), 0f);
            var rotation = Quaternion.Euler(0f, 0f, random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI) * Mathf.Rad2Deg);

            // A mix cycles through the three shape types in turn.
            var objectType = m_ObjectType == ObjectType.Mix ? (ObjectType)(n % 3) : m_ObjectType;
            var prefab = objectType switch
            {
                ObjectType.Circle => m_CirclePrefab,
                ObjectType.Capsule => m_CapsulePrefab,
                _ => m_PolygonPrefab
            };

            if (prefab == null)
                continue;

            var spawned = Instantiate(prefab, position, rotation);
            SetGeometry(spawned, objectType, ref random);

            var area = spawned.GetComponent<PhysicsArea>();
            var definition = area.definition;
            definition.density = m_ShapeDensity;
            area.definition = definition;

            spawned.SetActive(true);
            m_Spawned.Add(spawned);
        }
    }

    /// <summary>
    /// Destroys every dropped shape, leaving the container and the liquid alone.
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
    /// The shapes that are dropped into the liquid.
    /// Changing this does not rebuild the shapes until <see cref="Rebuild"/> is called.
    /// </summary>
    public ObjectType objectType
    {
        get => m_ObjectType;
        set => m_ObjectType = value;
    }

    /// <summary>
    /// How many shapes are dropped.
    /// Changing this does not rebuild the shapes until <see cref="Rebuild"/> is called.
    /// </summary>
    public int spawnCount
    {
        get => m_SpawnCount;
        set => m_SpawnCount = value;
    }

    /// <summary>
    /// How large the dropped shapes are compared with their normal size.
    /// Changing this does not rebuild the shapes until <see cref="Rebuild"/> is called.
    /// </summary>
    public float shapeScale
    {
        get => m_ShapeScale;
        set => m_ShapeScale = value;
    }

    /// <summary>
    /// The density of every dropped shape, which against the liquid density decides whether a shape floats or sinks.
    /// Changing this applies to every shape where it is, updating each body's mass.
    /// </summary>
    public float shapeDensity
    {
        get => m_ShapeDensity;
        set
        {
            m_ShapeDensity = value;

            foreach (var spawned in m_Spawned)
            {
                if (spawned == null)
                    continue;

                var area = spawned.GetComponent<PhysicsArea>();
                var definition = area.definition;
                definition.density = m_ShapeDensity;
                area.definition = definition;

                area.ApplyDefinition();
                spawned.GetComponent<PhysicsPose>().body.ApplyMassFromShapes();
            }
        }
    }

    /// <summary>
    /// The height of the liquid's surface above the container floor, in meters.
    /// Changing this resizes the liquid where it is.
    /// </summary>
    public float surfaceLevel
    {
        get => m_SurfaceLevel;
        set
        {
            m_SurfaceLevel = value;
            UpdateLiquid();
        }
    }

    /// <summary>
    /// The density of the liquid, which against a shape's density decides whether it floats or sinks.
    /// </summary>
    public float liquidDensity
    {
        get => m_LiquidDensity;
        set => m_LiquidDensity = value;
    }

    /// <summary>
    /// The direction the liquid flows in, in degrees counter-clockwise from the right.
    /// </summary>
    public float flowDirection
    {
        get => m_FlowDirection;
        set => m_FlowDirection = value;
    }

    /// <summary>
    /// How fast the liquid flows, in meters per second, where a negative speed flows the opposite way.
    /// </summary>
    public float flowSpeed
    {
        get => m_FlowSpeed;
        set => m_FlowSpeed = value;
    }

    /// <summary>
    /// How strongly the liquid slows a shape moving through it.
    /// </summary>
    public float linearDamping
    {
        get => m_LinearDamping;
        set => m_LinearDamping = value;
    }

    /// <summary>
    /// How strongly the liquid slows a shape spinning in it.
    /// </summary>
    public float angularDamping
    {
        get => m_AngularDamping;
        set => m_AngularDamping = value;
    }

    private void OnEnable() => PhysicsEvents.PreSimulate += ApplyBuoyancy;

    private void OnDisable() => PhysicsEvents.PreSimulate -= ApplyBuoyancy;

    private void Start()
    {
        UpdateLiquid();
        Rebuild();
    }

    // Sizes a dropped shape, since every circle, capsule and polygon is a different random size.
    private void SetGeometry(GameObject spawned, ObjectType objectType, ref Random random)
    {
        switch (objectType)
        {
            case ObjectType.Circle:
            {
                spawned.GetComponent<PhysicsAreaCircle>().geometry = new CircleGeometry { center = Vector2.zero, radius = Mathf.Max(MinShapeExtent, m_ShapeScale * random.NextFloat(0.25f, 0.75f)) };
                return;
            }

            case ObjectType.Capsule:
            {
                var capsuleLength = m_ShapeScale * random.NextFloat(0.25f, 1f);
                spawned.GetComponent<PhysicsAreaCapsule>().geometry = new CapsuleGeometry
                {
                    center1 = new Vector2(0f, -0.5f * capsuleLength),
                    center2 = new Vector2(0f, 0.5f * capsuleLength),
                    radius = Mathf.Max(MinShapeExtent, m_ShapeScale * random.NextFloat(0.25f, 0.5f))
                };
                return;
            }

            default:
            {
                var radius = m_ShapeScale * 0.25f * random.NextFloat(0f, 1f);
                spawned.GetComponent<PhysicsAreaPolygon>().geometry = WorkshopUtility.CreateRandomPolygon(extent: Mathf.Max(MinShapeExtent, m_ShapeScale * 0.75f), radius: radius, ref random);
                return;
            }
        }
    }

    // Resizes the liquid to span the container's width from the floor up to the surface level.
    private void UpdateLiquid()
    {
        if (m_Liquid == null)
            return;

        m_Liquid.geometry = PolygonGeometry.CreateBox(new Vector2(2f * InnerHalfWidth, m_SurfaceLevel), 0f, new PhysicsTransform(new Vector2(0f, 0.5f * m_SurfaceLevel), PhysicsRotate.identity));
        m_Liquid.ApplyGeometry();
    }

    // Applies buoyancy to every dynamic shape overlapping the liquid, whose surface is the top of the region.
    private void ApplyBuoyancy(PhysicsWorld world, float deltaTime)
    {
        if (world != PhysicsWorld.defaultWorld)
            return;

        var liquidRegion = new PhysicsAABB
        {
            lowerBound = new Vector2(-InnerHalfWidth, 0f),
            upperBound = new Vector2(InnerHalfWidth, m_SurfaceLevel)
        };

        var buoyancyInput = new PhysicsBody.BuoyancyInput
        {
            surfacePosition = new Vector2(0f, m_SurfaceLevel),
            surfaceNormal = Vector2.up,
            density = m_LiquidDensity,
            flowDirection = PhysicsRotate.FromDegrees(m_FlowDirection),
            flowSpeed = m_FlowSpeed,
            linearDamping = m_LinearDamping,
            angularDamping = m_AngularDamping,
            useTriggers = false
        };

        PhysicsBody.ApplyBuoyancy(world, liquidRegion, buoyancyInput, deltaTime);
    }

    #region Internal

    // Half the width inside the container, which the liquid spans and the shapes are dropped within.
    const float InnerHalfWidth = 10f;

    // The smallest extent a scaled shape can reach, keeping low scales from producing degenerate shapes.
    const float MinShapeExtent = 0.1f;

    const uint RandomSeed = 0x32628473;

    [SerializeField] PhysicsAreaPolygon m_Liquid;
    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField] ObjectType m_ObjectType = ObjectType.Mix;
    [SerializeField, Range(1, 1000)] int m_SpawnCount = 100;
    [SerializeField, Range(0.1f, 2f)] float m_ShapeScale = 1f;
    [SerializeField, Range(0.1f, 5f)] float m_ShapeDensity = 1f;
    [SerializeField, Range(0.1f, 75f)] float m_SurfaceLevel = 20f;
    [SerializeField, Range(0.1f, 10f)] float m_LiquidDensity = 2f;
    [SerializeField, Range(0f, 359f)] float m_FlowDirection;
    [SerializeField, Range(-20f, 20f)] float m_FlowSpeed;
    [SerializeField, Range(0f, 10f)] float m_LinearDamping = 1f;
    [SerializeField, Range(0f, 10f)] float m_AngularDamping = 1f;

    readonly List<GameObject> m_Spawned = new();

    #endregion
}
