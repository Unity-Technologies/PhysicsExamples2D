using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Fires a slicing ray from an orbiting player into a large destructible slab, cutting whatever it crosses into two new pieces.
/// The circular arena and the slab are Physics Pose and Physics Area components, and the slicing and reflecting off the arena wall are done here with the physics destructor.
/// </summary>
/// <remarks>
/// Every cut moves the two new pieces apart perpendicular to the cut, so a busy arena spreads itself out rather than piling fragments on top of each other.
/// The ray can reflect off the arena wall several times in one shot, each bounce scattered by a small random spread, slicing anything it crosses along the way.
/// Firing stops once the arena holds the maximum number of bodies, so the piece count never runs away.
/// The slab starts out green, and every piece cut from it gets its own random color, so a busy arena is easy to read at a glance.
/// </remarks>
public sealed class SlicingContents : MonoBehaviour
{
    /// <summary>
    /// Hands this component the controls menu buttons that move the player and fire, alongside the arrow keys and space bar.
    /// </summary>
    /// <param name="leftButton">The button that orbits the player counterclockwise.</param>
    /// <param name="rightButton">The button that orbits the player clockwise.</param>
    public void SetButtons(ControlsMenu.CustomButton leftButton, ControlsMenu.CustomButton rightButton)
    {
        m_LeftButton = leftButton;
        m_RightButton = rightButton;
    }

    /// <summary>
    /// Fires one slicing ray from the player, unless the world is paused or the arena is already full.
    /// </summary>
    public void Fire()
    {
        var world = PhysicsWorld.defaultWorld;

        if (world.paused || world.counters.bodyCount > m_MaximumFragments)
            return;

        var fireOrigin = m_FireTransform.position;
        var fireTranslation = -m_FireTransform.rotation.direction * ArenaRadius * 3f;

        // Slice bodies are collected per reflection, so the same body is never sliced twice by one bounce of the ray.
        using var destructibleBodies = new NativeHashSet<PhysicsBody>(4, Allocator.Temp);

        for (var n = 0; n <= m_ReflectionCount; ++n)
        {
            var rayInput = new PhysicsQuery.CastRayInput { origin = fireOrigin, translation = fireTranslation };
            using var results = world.CastRay(rayInput, new PhysicsQuery.QueryFilter { categories = PhysicsMask.All, hitCategories = DestructibleMask | GroundMask }, PhysicsQuery.WorldCastMode.AllSorted);

            if (results.Length == 0)
                break;

            destructibleBodies.Clear();

            foreach (var castHit in results)
            {
                var hitCategory = castHit.shape.contactFilter.categories;

                if (hitCategory == DestructibleMask)
                {
                    destructibleBodies.Add(castHit.shape.body);
                    continue;
                }

                if (hitCategory != GroundMask)
                    continue;

                // The next reflection starts just off the wall, heading in the reflected direction with a small random spread.
                fireOrigin = castHit.point + castHit.normal * ArenaInset;
                var reflectAngle = new PhysicsRotate(castHit.normal).radians + m_Random.NextFloat(-0.5f, 0.5f);
                fireTranslation = PhysicsRotate.FromRadians(reflectAngle).direction * ArenaRadius * 2f;

                world.DrawLine(rayInput.origin, castHit.point, Color.ghostWhite, HitDrawLifetime);
            }

            if (destructibleBodies.Count == 0)
                continue;

            foreach (var hitBody in destructibleBodies)
                Slice(hitBody, rayInput.origin, rayInput.translation);
        }
    }

    /// <summary>
    /// How many times a shot can reflect off the arena wall.
    /// </summary>
    public int reflectionCount
    {
        get => m_ReflectionCount;
        set => m_ReflectionCount = value;
    }

    /// <summary>
    /// The largest number of bodies the arena can hold before firing stops.
    /// </summary>
    public int maximumFragments
    {
        get => m_MaximumFragments;
        set => m_MaximumFragments = value;
    }

    private void OnEnable()
    {
        m_Random = new Random(RandomSeed);
        m_PlayerAngle = -0.5f * Mathf.PI;
        UpdatePlayerTransform();

        // Only the interiors are drawn, so the edges between the many sliced pieces do not clutter the arena.
        var world = PhysicsWorld.defaultWorld;
        m_WorldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;
    }

    // The fill options belong to the world rather than to this scene, so they would otherwise stay changed into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.drawFillOptions = m_WorldDrawFillOptions;
    }

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        if (!world.paused)
        {
            var currentKeyboard = Keyboard.current;
            var movement = PlayerSpeed * Time.deltaTime;

            if (IsPressed(m_LeftButton) || currentKeyboard.leftArrowKey.isPressed)
                m_PlayerAngle += movement;

            if (IsPressed(m_RightButton) || currentKeyboard.rightArrowKey.isPressed)
                m_PlayerAngle -= movement;

            m_PlayerAngle = PhysicsRotate.UnwindAngle(m_PlayerAngle);
            UpdatePlayerTransform();

            if (currentKeyboard.spaceKey.wasPressedThisFrame)
                Fire();
        }

        // The player is only drawn, never simulated, so nothing can touch it.
        world.DrawGeometry(PlayerGeometry, m_PlayerTransform, Color.azure);
    }

    // Returns whether a controls menu button is assigned and currently held down.
    private static bool IsPressed(ControlsMenu.CustomButton button) => button != null && button.isPressed;

    // Removes the object a body belongs to.
    // The body is owned by its Physics Pose component, so destroying the body directly is refused; destroying the GameObject releases it instead.
    private static void Remove(PhysicsBody body)
    {
        if (body.owner is not PhysicsPose pose)
            return;

        var target = pose.gameObject;
        target.SetActive(false);
        Destroy(target);
    }

    // Returns a random, evenly bright and saturated color, so every sliced piece is easy to tell apart from its neighbors.
    private Color RandomColor() => Color.HSVToRGB(m_Random.NextFloat(0f, 1f), m_Random.NextFloat(0.7f, 1f), m_Random.NextFloat(0.5f, 1f));

    // Places the player just outside the wall and the fire point just inside it, both facing the same way around the arena.
    private void UpdatePlayerTransform()
    {
        var rotation = PhysicsRotate.FromRadians(m_PlayerAngle);
        m_PlayerTransform = new PhysicsTransform(rotation.direction * (ArenaRadius + 2f), rotation);
        m_FireTransform = new PhysicsTransform(rotation.direction * (ArenaRadius - ArenaInset), rotation);
    }

    // Cuts every polygon on the hit body along the ray, replacing the body with one new piece per non-empty side of the cut.
    private void Slice(PhysicsBody hitBody, Vector2 origin, Vector2 translation)
    {
        using var targetShapes = hitBody.GetShapes();

        var targetPolygons = new NativeList<PolygonGeometry>(targetShapes.Length, Allocator.Temp);

        foreach (var shape in targetShapes)
        {
            if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
                targetPolygons.Add(shape.polygonGeometry);
        }

        if (targetPolygons.Length == 0)
        {
            targetPolygons.Dispose();
            return;
        }

        var targetGeometry = new PhysicsDestructor.FragmentGeometry(hitBody.transform, targetPolygons.AsArray());
        using var sliceResult = PhysicsDestructor.Slice(targetGeometry, origin, translation, Allocator.Temp);
        targetPolygons.Dispose();

        if (sliceResult.leftGeometry.Length == 0 && sliceResult.rightGeometry.Length == 0)
            return;

        Remove(hitBody);

        if (m_DestructiblePrefab == null)
            return;

        var sliceTransform = sliceResult.transform;
        var slicePosition = new Vector3(sliceTransform.position.x, sliceTransform.position.y, 0f);
        var sliceRotation = Quaternion.Euler(0f, 0f, sliceTransform.rotation.degrees);

        var sliceNormal = translation.normalized;
        var slicePerpendicular = Vector2.Perpendicular(sliceNormal);
        var originSide = Vector2.Dot(sliceNormal, origin - sliceTransform.position) > 0f;

        SpawnSlice(sliceResult.leftGeometry, slicePosition, sliceRotation, slicePerpendicular * (originSide ? SeparateSpeed : -SeparateSpeed));
        SpawnSlice(sliceResult.rightGeometry, slicePosition, sliceRotation, -slicePerpendicular * (originSide ? SeparateSpeed : -SeparateSpeed));
    }

    // Builds one new piece from a slice's polygons, one Physics Area Polygon per polygon, each given its own random color, and pushes the piece away from the cut.
    private void SpawnSlice(NativeArray<PolygonGeometry> polygons, Vector3 position, Quaternion rotation, Vector2 velocity)
    {
        if (polygons.Length == 0)
            return;

        var piece = Instantiate(m_DestructiblePrefab, position, rotation);

        var firstArea = piece.GetComponent<PhysicsAreaPolygon>();
        var shapeDefinition = firstArea.definition;

        for (var i = 0; i < polygons.Length; ++i)
        {
            var area = i == 0 ? firstArea : piece.AddComponent<PhysicsAreaPolygon>();

            var definition = shapeDefinition;
            var surfaceMaterial = definition.surfaceMaterial;
            surfaceMaterial.customColor = RandomColor();
            definition.surfaceMaterial = surfaceMaterial;

            area.definition = definition;
            area.geometry = polygons[i];
        }

        piece.SetActive(true);

        var body = piece.GetComponent<PhysicsPose>().body;
        body.ApplyMassFromShapes();
        body.linearVelocity = velocity;
    }

    #region Internal

    // The contact categories the arena wall and the slab are authored with, and the maximum reflections the doc UI clamps to.
    static readonly PhysicsMask GroundMask = new(2);
    static readonly PhysicsMask DestructibleMask = new(3);

    // The player's outline, how fast it orbits the arena, how fast a cut piece separates, the arena radius, and how far inside the wall a shot starts.
    static readonly CapsuleGeometry PlayerGeometry = new() { center1 = Vector2.left, center2 = Vector2.right, radius = 1f };
    const float PlayerSpeed = Mathf.PI;
    const float SeparateSpeed = 6f;
    const float ArenaRadius = 20f;
    const float ArenaInset = 0.1f;
    const float HitDrawLifetime = 30f / 60f;

    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_DestructiblePrefab;
    [SerializeField, Range(0, 10)] int m_ReflectionCount;
    [SerializeField, Range(1000, 5000)] int m_MaximumFragments = 5000;

    ControlsMenu.CustomButton m_LeftButton;
    ControlsMenu.CustomButton m_RightButton;
    PhysicsWorld.DrawFillOptions m_WorldDrawFillOptions;
    PhysicsTransform m_PlayerTransform;
    PhysicsTransform m_FireTransform;
    float m_PlayerAngle;
    Random m_Random;

    #endregion
}
