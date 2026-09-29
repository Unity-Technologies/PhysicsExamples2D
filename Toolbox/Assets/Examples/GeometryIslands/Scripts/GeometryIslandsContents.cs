using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Fires projectiles down at a row of tall static slabs, breaking holes out of them until a part is cut free and falls.
/// The slabs, the projectiles and the debris are Physics Pose and Physics Area components, and the fragmenting itself is done here with the physics destructor.
/// </summary>
/// <remarks>
/// What is left of a slab after a hit is grouped into islands, each one a set of polygons that still touch each other, and every island becomes its own body.
/// An island still crossing the ground line drawn below the slabs stays static, while one cut free of it becomes dynamic and falls, so a slab only collapses once a hole is cut all the way through it.
/// The pieces broken out of each hole fall as debris, and anything that falls off the bottom of the world is destroyed.
/// Every slab and island of what is left of one stays the same green, while every fragment broken off gets its own random color so a scattered hole is easy to read at a glance.
/// </remarks>
public sealed class GeometryIslandsContents : MonoBehaviour, PhysicsAreaCallbacks.IContactCallback
{
    /// <summary>
    /// Hands this component the controls menu buttons that move the player and fire, alongside the arrow keys and space bar.
    /// </summary>
    /// <param name="leftButton">The button that moves the player left.</param>
    /// <param name="rightButton">The button that moves the player right.</param>
    /// <param name="fireButton">The button that fires while it is held.</param>
    public void SetButtons(ControlsMenu.CustomButton leftButton, ControlsMenu.CustomButton rightButton, ControlsMenu.CustomButton fireButton)
    {
        m_LeftButton = leftButton;
        m_RightButton = rightButton;
        m_FireButton = fireButton;
    }

    /// <summary>
    /// The radius of the hole a projectile breaks out of a slab, in meters.
    /// </summary>
    public float fragmentRadius
    {
        get => m_FragmentRadius;
        set
        {
            m_FragmentRadius = value;
            UpdateFragmentMask();
        }
    }

    /// <summary>
    /// Whether the pieces of each hole are blown outward as they are broken off, rather than simply falling.
    /// </summary>
    public bool fragmentExplode
    {
        get => m_FragmentExplode;
        set => m_FragmentExplode = value;
    }

    // Handles a contact by the categories of its two sides: anything reaching the ground is destroyed, and a projectile reaching a slab breaks a hole out of it.
    void PhysicsAreaCallbacks.IContactCallback.OnContactBegin2D(PhysicsAreaCallbacks.ContactBeginEvent beginEvent)
    {
        var engineEvent = beginEvent.beginEvent;
        var shapeA = engineEvent.shapeA;
        var shapeB = engineEvent.shapeB;

        // An earlier contact in the same step may already have destroyed one side.
        if (!shapeA.isValid || !shapeB.isValid)
            return;

        var categoryA = shapeA.contactFilter.categories;
        var categoryB = shapeB.contactFilter.categories;

        if (categoryA == GroundMask || categoryB == GroundMask)
        {
            Remove(categoryA == GroundMask ? beginEvent.areaB : beginEvent.areaA);
            return;
        }

        if (categoryA != DestructibleMask && categoryB != DestructibleMask)
            return;

        if (categoryA != ProjectileMask && categoryB != ProjectileMask)
            return;

        var hitPosition = engineEvent.contactId.contact.manifold.points[0].pointA;
        var destructibleArea = categoryA == DestructibleMask ? beginEvent.areaA : beginEvent.areaB;
        var projectileArea = categoryA == DestructibleMask ? beginEvent.areaB : beginEvent.areaA;

        Fragment(destructibleArea, hitPosition);
        Remove(projectileArea);
    }

    void PhysicsAreaCallbacks.IContactCallback.OnContactEnd2D(PhysicsAreaCallbacks.ContactEndEvent endEvent)
    {
    }

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);
        UpdateFragmentMask();

        // Only the interiors are drawn, so the edges between the many pieces of each slab do not clutter it.
        var world = PhysicsWorld.defaultWorld;
        m_WorldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;
    }

    // The fill options belong to the world rather than to this scene, so they would otherwise stay changed into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.drawFillOptions = m_WorldDrawFillOptions;

        if (m_FragmentMask.IsCreated)
            m_FragmentMask.Dispose();
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        if (!world.paused)
        {
            var currentKeyboard = Keyboard.current;
            var movement = PlayerSpeed * Time.deltaTime;

            if (IsPressed(m_LeftButton) || currentKeyboard.leftArrowKey.isPressed)
                m_PlayerPosition.x -= movement;

            if (IsPressed(m_RightButton) || currentKeyboard.rightArrowKey.isPressed)
                m_PlayerPosition.x += movement;

            m_PlayerPosition.x = Mathf.Clamp(m_PlayerPosition.x, -PlayerRange, PlayerRange);

            // Holding fire keeps firing, one projectile per interval.
            m_ProjectileTime += Time.deltaTime;

            if ((IsPressed(m_FireButton) || currentKeyboard.spaceKey.isPressed) && m_ProjectileTime >= ProjectileInterval)
            {
                m_ProjectileTime = 0f;
                Fire();
            }
        }

        // The player and the ground line are only drawn, never simulated, so nothing can touch them.
        world.DrawGeometry(PlayerGeometry, m_PlayerPosition, Color.azure);
        world.DrawGeometry(GroundLineGeometry, GroundLineTransform, Color.seaGreen);
    }

    // Returns whether a controls menu button is assigned and currently held down.
    private static bool IsPressed(ControlsMenu.CustomButton button) => button != null && button.isPressed;

    // Returns a random, evenly bright and saturated color, so every piece of debris is easy to tell apart from its neighbors.
    private Color RandomColor() => Color.HSVToRGB(m_Random.NextFloat(0f, 1f), m_Random.NextFloat(0.7f, 1f), m_Random.NextFloat(0.5f, 1f));

    // Fires one projectile straight down from the player.
    private void Fire()
    {
        if (m_ProjectilePrefab == null)
            return;

        var position = m_PlayerPosition + Vector2.down * (ProjectileRadius + 1.5f);
        var spawned = Instantiate(m_ProjectilePrefab, position, Quaternion.identity);

        var pose = spawned.GetComponent<PhysicsPose>();
        var definition = pose.definition;
        definition.linearVelocity = Vector2.down * ProjectileSpeed;
        pose.definition = definition;

        spawned.GetComponent<PhysicsArea>().callbackTarget = this;
        spawned.SetActive(true);
    }

    // Rebuilds the circle cut out of a slab at every hit, as polygons centered on the origin.
    private void UpdateFragmentMask()
    {
        if (m_FragmentMask.IsCreated)
            m_FragmentMask.Dispose();

        var composer = PhysicsComposer.Create();
        composer.AddLayer(new CircleGeometry { radius = m_FragmentRadius }, PhysicsTransform.identity);
        m_FragmentMask = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Persistent);
        composer.Destroy();
    }

    // Breaks a hole out of a slab at the hit position, replacing the slab with a body per island of what is left and adding one falling piece of debris for every piece broken off.
    private void Fragment(PhysicsArea destructibleArea, Vector2 hitPosition)
    {
        var world = PhysicsWorld.defaultWorld;
        world.DrawCircle(hitPosition, m_FragmentRadius, Color.ivory, HitDrawLifetime, PhysicsWorld.DrawFillOptions.Outline);

        // After earlier hits a slab is several areas on one body, so every polygon on the body is fragmented together.
        var destructibleBody = destructibleArea.pose.body;
        var destructibleIsStatic = destructibleBody.type == PhysicsBody.BodyType.Static;
        using var destructibleShapes = destructibleBody.GetShapes();

        var targetPolygons = new NativeList<PolygonGeometry>(destructibleShapes.Length, Allocator.Temp);

        foreach (var shape in destructibleShapes)
        {
            if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
                targetPolygons.Add(shape.polygonGeometry);
        }

        var targetGeometry = new PhysicsDestructor.FragmentGeometry(destructibleBody.transform, targetPolygons.AsReadOnly());
        targetPolygons.Dispose();

        // The hit position is always one of the points, and the rest are scattered at random across the hole.
        var fragmentPoints = new NativeArray<Vector2>(FragmentCount, Allocator.Temp);
        fragmentPoints[0] = hitPosition;

        for (var i = 1; i < FragmentCount; ++i)
        {
            var rotate = PhysicsRotate.FromRadians(m_Random.NextFloat(0f, PhysicsMath.PI));
            var radius = m_Random.NextFloat(0.05f, m_FragmentRadius);
            fragmentPoints[i] = hitPosition + rotate.direction * radius;
        }

        var maskGeometry = new PhysicsDestructor.FragmentGeometry(hitPosition, m_FragmentMask);
        using var fragmentResults = PhysicsDestructor.Fragment(targetGeometry, maskGeometry, fragmentPoints, Allocator.Temp);
        fragmentPoints.Dispose();

        Remove(destructibleArea);

        var fragmentTransform = fragmentResults.transform;
        var fragmentPosition = new Vector3(fragmentTransform.position.x, fragmentTransform.position.y, 0f);
        var fragmentRotation = Quaternion.Euler(0f, 0f, fragmentTransform.rotation.degrees);

        // Every island of what is left becomes its own body carrying an area per polygon, all sharing the slab's contact filter and surface.
        if (m_DestructiblePrefab != null)
        {
            var unbrokenGeometry = fragmentResults.unbrokenGeometry;

            foreach (var islandRange in fragmentResults.unbrokenGeometryIslands)
            {
                var islandGeometry = unbrokenGeometry.GetSubArray(islandRange.start, islandRange.length);
                var falls = !destructibleIsStatic || !CrossesGroundLine(islandGeometry, fragmentTransform);

                var island = Instantiate(m_DestructiblePrefab, fragmentPosition, fragmentRotation);

                // A falling island is dynamic, and reports its contacts here so it can be destroyed when it falls off the bottom of the world.
                if (falls)
                {
                    var pose = island.GetComponent<PhysicsPose>();
                    var bodyDefinition = pose.definition;
                    bodyDefinition.type = PhysicsBody.BodyType.Dynamic;
                    bodyDefinition.fastCollisionsAllowed = true;
                    bodyDefinition.gravityScale = FallingGravityScale;
                    pose.definition = bodyDefinition;
                }

                var firstArea = island.GetComponent<PhysicsAreaPolygon>();
                var shapeDefinition = firstArea.definition;

                for (var i = 0; i < islandGeometry.Length; ++i)
                {
                    var area = i == 0 ? firstArea : island.AddComponent<PhysicsAreaPolygon>();
                    area.definition = shapeDefinition;
                    area.geometry = islandGeometry[i];

                    if (!falls)
                        continue;

                    area.callbackSource = PhysicsArea.CallbackSourceType.Area;
                    area.callbackTarget = this;
                }

                island.SetActive(true);
            }
        }

        // Every broken piece becomes its own dynamic body, each given its own random color so a busy hole reads as a scatter of separate pieces.
        if (m_DebrisPrefab != null)
        {
            foreach (var geometry in fragmentResults.brokenGeometry)
            {
                var debris = Instantiate(m_DebrisPrefab, fragmentPosition, fragmentRotation);
                var area = debris.GetComponent<PhysicsAreaPolygon>();
                area.geometry = geometry;
                area.callbackTarget = this;

                var definition = area.definition;
                var surfaceMaterial = definition.surfaceMaterial;
                surfaceMaterial.customColor = RandomColor();
                definition.surfaceMaterial = surfaceMaterial;
                area.definition = definition;

                debris.SetActive(true);
            }
        }

        if (m_FragmentExplode)
        {
            world.Explode(new PhysicsWorld.ExplosionDefinition
            {
                position = hitPosition + Vector2.up * m_FragmentRadius,
                hitCategories = DebrisMask,
                impulsePerLength = 2f,
                radius = m_FragmentRadius * 3f
            });
        }
    }

    // Returns whether any polygon of an island crosses the ground line, which is what keeps the island standing.
    private static bool CrossesGroundLine(NativeArray<PolygonGeometry> islandGeometry, PhysicsTransform islandTransform)
    {
        foreach (var geometry in islandGeometry)
        {
            if (geometry.Intersect(islandTransform, GroundLineGeometry, GroundLineTransform).pointCount > 0)
                return true;
        }

        return false;
    }

    // Removes the object an area belongs to.
    // It is deactivated first, which destroys its body straight away, so later contacts in the same step see it as gone rather than acting on it a second time.
    private static void Remove(PhysicsArea area)
    {
        if (area == null)
            return;

        var target = area.pose.gameObject;
        target.SetActive(false);
        Destroy(target);
    }

    #region Internal

    // The contact categories every part is authored with, which decide what each one can touch.
    static readonly PhysicsMask GroundMask = new(2);
    static readonly PhysicsMask DestructibleMask = new(3);
    static readonly PhysicsMask DebrisMask = new(4);
    static readonly PhysicsMask ProjectileMask = new(5);

    // The line below the slabs that an island has to cross to stay standing.
    static readonly SegmentGeometry GroundLineGeometry = new() { point1 = new Vector2(-30f, 0f), point2 = new Vector2(25f, 0f) };
    static readonly PhysicsTransform GroundLineTransform = new(Vector2.down * 21.9f);

    // The player's outline, how fast it moves and how far either side of the middle it can go.
    static readonly CapsuleGeometry PlayerGeometry = new() { center1 = Vector2.zero, center2 = Vector2.up, radius = 0.5f };
    const float PlayerSpeed = 60f;
    const float PlayerRange = 28f;

    // How large and fast each projectile is, and how often one is fired while fire is held, in seconds.
    const float ProjectileRadius = 0.2f;
    const float ProjectileSpeed = 40f;
    const float ProjectileInterval = 0.02f;

    // How many pieces each hole is split into, how hard gravity pulls on an island cut free, and how long the outline of each hole stays drawn, in seconds.
    const int FragmentCount = 25;
    const float FallingGravityScale = 4f;
    const float HitDrawLifetime = 2f / 60f;

    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_DestructiblePrefab;
    [SerializeField] GameObject m_ProjectilePrefab;
    [SerializeField] GameObject m_DebrisPrefab;
    [SerializeField, Range(0.5f, 3f)] float m_FragmentRadius = 1f;
    [SerializeField] bool m_FragmentExplode;

    ControlsMenu.CustomButton m_LeftButton;
    ControlsMenu.CustomButton m_RightButton;
    ControlsMenu.CustomButton m_FireButton;
    NativeArray<PolygonGeometry> m_FragmentMask;
    PhysicsWorld.DrawFillOptions m_WorldDrawFillOptions;
    Vector2 m_PlayerPosition = new(0f, 24f);
    float m_ProjectileTime;
    Random m_Random;

    #endregion
}
