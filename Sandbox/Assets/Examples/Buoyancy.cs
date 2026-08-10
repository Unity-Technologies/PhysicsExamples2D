using System;
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using Random = Unity.Mathematics.Random;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrating buoyancy applied to random shapes dropped into a liquid region.")]
public sealed class Buoyancy : SandboxExampleBehaviour
{
    private enum ObjectType
    {
        Circle,
        Capsule,
        Polygon,
        Mix
    }

    private ObjectType m_ObjectType;
    private int m_SpawnCount;
    private float m_ShapeScale;
    private float m_ShapeDensity;
    private float m_SurfaceLevel;
    private float m_LiquidDensity;
    private float m_FlowDirection;
    private float m_FlowSpeed;
    private float m_LinearDamping;
    private float m_AngularDamping;

    private readonly Color m_LiquidColor = Color.dodgerBlue;

    // The spawned shapes, kept so the density option can be changed live without respawning.
    private NativeList<PhysicsShape> m_Shapes;

    // The liquid trigger shape, kept so the surface level option can resize it live.
    private PhysicsShape m_LiquidShape;

    // The U-shaped container: the inner width, the default liquid surface height and the wall height.
    // The walls extend far beyond the top of the screen so nothing can escape the container.
    private const float InnerHalfWidth = 10f;
    private const float FloorThickness = 1f;
    private const float SideWallThickness = 2f;
    private const float WallHeight = 400f;
    private const float DefaultSurfaceLevel = 20f;
    private const float MaxSurfaceLevel = 75f;

    // The smallest extent a scaled shape can reach, keeping low scales from producing degenerate shapes.
    private const float MinShapeExtent = 0.1f;

    protected override float CameraSize => 16f;
    protected override Vector2 CameraPosition => new(0f, 12f);

    protected override void OnExampleEnable()
    {
        // Register to apply buoyancy.
        PhysicsEvents.PreSimulate += ApplyBuoyancy;

        m_Shapes = new NativeList<PhysicsShape>(Allocator.Persistent);

        m_ObjectType = ObjectType.Mix;
        m_SpawnCount = 100;
        m_ShapeScale = 1f;
        m_ShapeDensity = 1f;
        m_SurfaceLevel = DefaultSurfaceLevel;
        m_LiquidDensity = 2f;
        m_FlowDirection = 0f;
        m_FlowSpeed = 0f;
        m_LinearDamping = 1f;
        m_AngularDamping = 1f;
    }

    protected override void OnExampleDisable()
    {
        // Unregister to apply buoyancy.
        PhysicsEvents.PreSimulate -= ApplyBuoyancy;

        if (m_Shapes.IsCreated)
            m_Shapes.Dispose();
    }

    protected override void SetupOptions()
    {
        // Object Type.
        AddEnum("Object Type", m_ObjectType, v => m_ObjectType = v, rebuild: true);

        // Spawn Count.
        AddSliderInt("Spawn Count", m_SpawnCount, 1, 1000, v => m_SpawnCount = v, rebuild: true);

        // Shape Scale.
        AddSlider("Shape Scale", m_ShapeScale, 0.1f, 2f, v => m_ShapeScale = v, rebuild: true);

        // Shape Density, applied live to all spawned shapes without respawning them.
        AddSlider("Shape Density", m_ShapeDensity, 0.1f, 5f, v =>
        {
            m_ShapeDensity = v;

            foreach (var physicsShape in m_Shapes)
                physicsShape.SetDensity(v, updateBodyMass: true);
        });

        // Surface Level, resizing the liquid trigger shape live without respawning anything.
        AddSlider("Surface Level", m_SurfaceLevel, 0.1f, MaxSurfaceLevel, v =>
        {
            m_SurfaceLevel = v;
            m_LiquidShape.polygonGeometry = CreateLiquidGeometry();
        });

        // Liquid Density.
        AddSlider("Liquid Density", m_LiquidDensity, 0.1f, 10f, v => m_LiquidDensity = v);

        // Flow Direction.
        AddSlider("Flow Direction", m_FlowDirection, 0f, 359f, v => m_FlowDirection = v);

        // Flow Speed.
        AddSlider("Flow Speed", m_FlowSpeed, -20f, 20f, v => m_FlowSpeed = v);

        // Linear Damping.
        AddSlider("Linear Damping", m_LinearDamping, 0f, 10f, v => m_LinearDamping = v);

        // Angular Damping.
        AddSlider("Angular Damping", m_AngularDamping, 0f, 10f, v => m_AngularDamping = v);
    }

    protected override void SetupScene()
    {
        CreateContainer();
        SpawnShapes();
    }

    // Creates the static U-shaped container and the liquid region within it.
    // The liquid is a trigger shape so it visualizes the region without colliding with anything.
    private void CreateContainer()
    {
        var world = World;
        var body = world.CreateBody();

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;

        // The container floor.
        {
            var boxTransform = new PhysicsTransform(new Vector2(0f, -0.5f * FloorThickness), PhysicsRotate.identity);
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(2f * InnerHalfWidth + 2f * SideWallThickness, FloorThickness), radius: 0f, transform: boxTransform);
            body.CreateShape(boxGeometry, shapeDef);
        }

        // The container walls, extending high above the screen.
        {
            var boxTransform = new PhysicsTransform(new Vector2(-InnerHalfWidth - 0.5f * SideWallThickness, 0.5f * WallHeight), PhysicsRotate.identity);
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(SideWallThickness, WallHeight), radius: 0f, transform: boxTransform);
            body.CreateShape(boxGeometry, shapeDef);
        }

        {
            var boxTransform = new PhysicsTransform(new Vector2(InnerHalfWidth + 0.5f * SideWallThickness, 0.5f * WallHeight), PhysicsRotate.identity);
            var boxGeometry = PolygonGeometry.CreateBox(new Vector2(SideWallThickness, WallHeight), radius: 0f, transform: boxTransform);
            body.CreateShape(boxGeometry, shapeDef);
        }

        // The liquid region as a trigger shape with a custom color.
        {
            var liquidShapeDef = new PhysicsShapeDefinition
            {
                isTrigger = true,
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = m_LiquidColor }
            };

            m_LiquidShape = body.CreateShape(CreateLiquidGeometry(), liquidShapeDef);
        }
    }

    // Creates the liquid region geometry, a box spanning the container width up to the surface level.
    private PolygonGeometry CreateLiquidGeometry()
    {
        var boxTransform = new PhysicsTransform(new Vector2(0f, 0.5f * m_SurfaceLevel), PhysicsRotate.identity);
        return PolygonGeometry.CreateBox(new Vector2(2f * InnerHalfWidth, m_SurfaceLevel), radius: 0f, transform: boxTransform);
    }

    // Spawns the dynamic shapes at random positions above the liquid.
    private void SpawnShapes()
    {
        var world = World;
        ref var random = ref Random;

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = new PhysicsShapeDefinition { density = m_ShapeDensity, surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.5f } };

        m_Shapes.Clear();

        for (var n = 0; n < m_SpawnCount; ++n)
        {
            bodyDef.position = new Vector2(random.NextFloat(-InnerHalfWidth + 1f, InnerHalfWidth - 1f), random.NextFloat(m_SurfaceLevel + 2f, m_SurfaceLevel + 30f));
            bodyDef.rotation = PhysicsRotate.FromRadians(random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI));
            var body = world.CreateBody(bodyDef);

            // Fetch the appropriate shape color.
            shapeDef.surfaceMaterial.customColor = ShapeColor;

            // Create the appropriate shape type.
            var objectType = m_ObjectType == ObjectType.Mix ? (ObjectType)(n % 3) : m_ObjectType;
            switch (objectType)
            {
                case ObjectType.Circle:
                {
                    var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = Mathf.Max(MinShapeExtent, m_ShapeScale * random.NextFloat(0.25f, 0.75f)) };
                    m_Shapes.Add(body.CreateShape(circleGeometry, shapeDef));
                    continue;
                }

                case ObjectType.Capsule:
                {
                    var capsuleLength = m_ShapeScale * random.NextFloat(0.25f, 1.0f);
                    var capsuleGeometry = new CapsuleGeometry
                    {
                        center1 = new Vector2(0f, -0.5f * capsuleLength),
                        center2 = new Vector2(0f, 0.5f * capsuleLength),
                        radius = Mathf.Max(MinShapeExtent, m_ShapeScale * random.NextFloat(0.25f, 0.5f))
                    };
                    m_Shapes.Add(body.CreateShape(capsuleGeometry, shapeDef));
                    continue;
                }

                case ObjectType.Polygon:
                {
                    var radius = m_ShapeScale * 0.25f * random.NextFloat(0f, 1.0f);
                    var polygonGeometry = SandboxUtility.CreateRandomPolygon(extent: Mathf.Max(MinShapeExtent, m_ShapeScale * 0.75f), radius: radius, ref random);
                    m_Shapes.Add(body.CreateShape(polygonGeometry, shapeDef));
                    continue;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    // Applies buoyancy to any dynamic shape overlapping the liquid region.
    // The region is the liquid trigger's bounds and the surface plane is the top of that region.
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
}
