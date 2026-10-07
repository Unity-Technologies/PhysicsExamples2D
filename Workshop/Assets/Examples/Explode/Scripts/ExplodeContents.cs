using System.Collections.Generic;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Drops a pile of shapes into a deep valley and sets off an explosion at the bottom of it, which pushes the shapes away, or pulls them in when the impulse is negative.
/// The valley is authored from a static Physics Pose with Physics Area Polygons, and every shape is one Physics Pose with a Physics Area, instantiated from an inactive prefab.
/// </summary>
/// <remarks>
/// Pressing the Explode button or the space key sets off one explosion, and holding it does not repeat it.
/// The yellow circle is the radius, inside which the impulse is at full strength, and the dim yellow circle is where the impulse has faded to nothing.
/// The shapes are dropped in batches at random places along the top, cycling through a capsule, a circle, a square and a random rounded polygon.
/// Changing the shape count rebuilds the scene, while the radius, falloff and impulse act on the next explosion.
/// </remarks>
public sealed class ExplodeContents : MonoBehaviour
{
    /// <summary>
    /// Destroys every shape and starts dropping a new pile.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        m_Random = new Random(RandomSeed);
        m_Spawned = 0;
        m_SpawnTime = BatchInterval;
    }

    /// <summary>
    /// Destroys every shape, leaving the valley alone.
    /// </summary>
    public void Clear()
    {
        foreach (var spawned in m_Shapes)
        {
            if (spawned != null)
            {
                spawned.SetActive(false);
                Destroy(spawned);
            }
        }

        m_Shapes.Clear();
    }

    /// <summary>
    /// Sets the button that sets off an explosion when it is pressed.
    /// </summary>
    /// <param name="explodeButton">The button on the controls menu.</param>
    public void SetButton(ControlsMenu.CustomButton explodeButton) => m_ExplodeButton = explodeButton;

    /// <summary>
    /// How many shapes are dropped into the valley.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int shapeCount
    {
        get => m_ShapeCount;
        set => m_ShapeCount = value;
    }

    /// <summary>
    /// The radius of the explosion, inside which the impulse is at full strength.
    /// </summary>
    public float radius
    {
        get => m_Radius;
        set => m_Radius = value;
    }

    /// <summary>
    /// The distance beyond the radius over which the impulse fades to nothing.
    /// </summary>
    public float falloff
    {
        get => m_Falloff;
        set => m_Falloff = value;
    }

    /// <summary>
    /// The impulse of the explosion for each unit of length of a shape that faces it, which is negative to pull shapes in.
    /// </summary>
    public float impulse
    {
        get => m_Impulse;
        set => m_Impulse = value;
    }

    // Shapes are dropped from here on, and this is used rather than Awake because Unity calls it again after a script reload while playing.
    private void OnEnable() => Rebuild();

    private void OnDisable() => Clear();

    private void Update()
    {
        var world = PhysicsWorld.defaultWorld;

        // Show the region the explosion pushes on, and the distance beyond it that the push fades out over.
        world.DrawCircle(ExplosionCenter, m_Radius, RadiusColor);
        world.DrawCircle(ExplosionCenter, m_Radius + m_Falloff, FalloffColor);

        // Finish if the world is paused.
        if (world.paused)
            return;

        // Explode once each time the button or the space key is pressed.
        var currentKeyboard = Keyboard.current;
        var buttonPressed = m_ExplodeButton != null && m_ExplodeButton.isPressed;
        if ((buttonPressed && !m_ButtonWasPressed) || (currentKeyboard != null && currentKeyboard.spaceKey.wasPressedThisFrame))
            world.Explode(new PhysicsWorld.ExplosionDefinition { position = ExplosionCenter, radius = m_Radius, falloff = m_Falloff, impulsePerLength = m_Impulse });

        m_ButtonWasPressed = buttonPressed;

        // Drop the shapes in batches until there are as many as chosen.
        if (m_Spawned >= m_ShapeCount)
            return;

        m_SpawnTime += Time.deltaTime;
        if (m_SpawnTime < BatchInterval)
            return;

        m_SpawnTime = 0f;
        SpawnBatch();
    }

    // Drops a batch of shapes at random places along the top between the walls, cycling through a capsule, a circle, a square and a random rounded polygon.
    private void SpawnBatch()
    {
        for (var i = 0; i < BatchSize && m_Spawned < m_ShapeCount; ++i)
        {
            var prefab = (m_Spawned % 4) switch
            {
                0 => m_CapsulePrefab,
                1 => m_CirclePrefab,
                2 => m_SquarePrefab,
                _ => m_PolygonPrefab
            };

            ++m_Spawned;

            if (prefab == null)
                continue;

            var position = new Vector3(m_Random.NextFloat(-SpawnHalfWidth, SpawnHalfWidth), SpawnY, 0f);
            var spawned = Instantiate(prefab, position, Quaternion.identity);

            // The polygon is the only shape that is different every time, so it is given its geometry before it is switched on.
            if (prefab == m_PolygonPrefab)
                spawned.GetComponent<PhysicsAreaPolygon>().geometry = WorkshopUtility.CreateRandomPolygon(extent: 0.75f, radius: 0.1f, ref m_Random);

            spawned.SetActive(true);
            m_Shapes.Add(spawned);
        }
    }

    #region Internal

    // The point a little above the bottom of the valley that the explosion is centered on.
    static readonly Vector2 ExplosionCenter = new(0f, -13.5f);

    // The colors of the circle that shows the radius and the circle that shows where the falloff ends.
    static readonly Color RadiusColor = Color.yellow;
    static readonly Color FalloffColor = new(0.5f, 0.5f, 0f);

    // How many shapes are dropped in a batch and how long to wait between batches.
    const int BatchSize = 10;
    const float BatchInterval = 0.25f;

    // Shapes are dropped just below the ceiling, anywhere between the side walls with a margin from each.
    const float SpawnY = 19f;
    const float SpawnHalfWidth = 19.29f;

    // The seed the places the shapes are dropped from and their random polygons start from.
    const uint RandomSeed = 0x32628473;

    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_SquarePrefab;
    [SerializeField] GameObject m_PolygonPrefab;
    [SerializeField, Range(50, 1000)] int m_ShapeCount = 500;
    [SerializeField, Range(1f, 20f)] float m_Radius = 10f;
    [SerializeField, Range(0f, 10f)] float m_Falloff = 2f;
    [SerializeField, Range(-60f, 60f)] float m_Impulse = 40f;

    readonly List<GameObject> m_Shapes = new();
    ControlsMenu.CustomButton m_ExplodeButton;
    Random m_Random;
    int m_Spawned;
    float m_SpawnTime;
    bool m_ButtonWasPressed;

    #endregion
}
