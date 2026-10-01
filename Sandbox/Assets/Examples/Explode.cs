using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Demonstrates an explosion that pushes shapes away from a point, or pulls them in when the impulse is negative.")]
public sealed class Explode : SandboxExampleBehaviour
{
    private const int BatchSize = 10;
    private const float BatchInterval = 0.25f;
    private const float RampCenterX = 10f;
    private const float RampLength = 24f;
    private const float WallThickness = 0.4f;
    private const float CeilingY = 20.1f;
    private const float SpawnY = 19f;
    private const float FloorY = -17f;
    private const float ShapeGravityScale = 2f;
    private const float SpawnMargin = 1f;

    private static readonly Vector2 ExplosionCenter = new(0f, FloorY + 3.5f);
    private static readonly Color RadiusColor = Color.yellow;
    private static readonly Color FalloffColor = new(0.5f, 0.5f, 0f);
    private static readonly float RampAngle = 0.15f * PhysicsMath.PI;

    private int m_ShapeCount;
    private float m_Radius;
    private float m_Falloff;
    private float m_Impulse;

    private int m_Spawned;
    private float m_SpawnHalfWidth;
    private float m_SpawnTime;
    private bool m_ButtonWasPressed;
    private ControlsMenu.CustomButton m_ExplodeButton;

    protected override float CameraSize => 21f;
    protected override Vector2 CameraPosition => new(0f, -0.2f);

    protected override void OnExampleEnable()
    {
        m_ShapeCount = 500;
        m_Radius = 10f;
        m_Falloff = 2f;
        m_Impulse = 40f;

        // Set controls.
        m_ExplodeButton = SandboxManager.ControlsMenu[0];
        m_ExplodeButton.Set("Explode [Spc]");
    }

    protected override void SetupOptions()
    {
        // Shape Count.
        AddSliderInt("Shape Count", m_ShapeCount, 50, 1000, v => m_ShapeCount = v, rebuild: true);

        // Radius.
        AddSlider("Radius", m_Radius, 1f, 20f, v => m_Radius = v);

        // Falloff.
        AddSlider("Falloff", m_Falloff, 0f, 10f, v => m_Falloff = v);

        // Impulse.
        AddSlider("Impulse", m_Impulse, -60f, 60f, v => m_Impulse = v);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        m_Spawned = 0;
        m_SpawnTime = BatchInterval;

        // A valley with side walls that run from the ceiling down to its sloped sides.
        {
            var groundBody = world.CreateBody();

            var shapeDef = new PhysicsShapeDefinition();
            shapeDef.surfaceMaterial.friction = 0.1f;

            shapeDef.surfaceMaterial.bounciness = 0.4f;

            // The walls run from the ceiling, which spans them, down to where the valley starts.
            PhysicsMath.CosSin(RampAngle, out var rampCosine, out var rampSine);
            var rampOuterX = RampCenterX + RampLength * 0.5f * rampCosine;
            var wallX = rampOuterX - WallThickness * 0.5f;
            m_SpawnHalfWidth = wallX - WallThickness * 0.5f - SpawnMargin;
            var wallBottom = FloorY + wallX * rampSine / rampCosine;
            var wallTop = CeilingY + WallThickness * 0.5f;
            var wallHeight = wallTop - wallBottom;
            var wallCenterY = wallBottom + wallHeight * 0.5f;
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(WallThickness, wallHeight), 0f, new PhysicsTransform(new Vector2(wallX, wallCenterY), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(WallThickness, wallHeight), 0f, new PhysicsTransform(new Vector2(-wallX, wallCenterY), PhysicsRotate.identity)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(wallX * 2f + WallThickness, WallThickness), 0f, new PhysicsTransform(new Vector2(0f, CeilingY), PhysicsRotate.identity)), shapeDef);

            // The valley slopes from the bottom of each wall to a point in the middle.
            var slopeLength = wallX / rampCosine + WallThickness;
            var slopeCenterY = (wallBottom + FloorY) * 0.5f;
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(slopeLength, WallThickness), 0f, new PhysicsTransform(new Vector2(-wallX * 0.5f, slopeCenterY), PhysicsRotate.FromRadians(-RampAngle))), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(slopeLength, WallThickness), 0f, new PhysicsTransform(new Vector2(wallX * 0.5f, slopeCenterY), PhysicsRotate.FromRadians(RampAngle))), shapeDef);
        }
    }

    private void Update()
    {
        // Get the default world.
        var world = World;

        // Show the region the explosion pushes on, and the distance beyond it that the push fades out over.
        world.DrawCircle(ExplosionCenter, m_Radius, RadiusColor);
        world.DrawCircle(ExplosionCenter, m_Radius + m_Falloff, FalloffColor);

        // Finish if the world is paused.
        if (SandboxManager.WorldPaused)
            return;

        // Explode once each time the button or the space key is pressed.
        var currentKeyboard = Keyboard.current;
        var buttonPressed = m_ExplodeButton.isPressed;
        if ((buttonPressed && !m_ButtonWasPressed) || (currentKeyboard != null && currentKeyboard.spaceKey.wasPressedThisFrame))
        {
            world.Explode(new PhysicsWorld.ExplosionDefinition { position = ExplosionCenter, radius = m_Radius, falloff = m_Falloff, impulsePerLength = m_Impulse });
        }

        m_ButtonWasPressed = buttonPressed;

        // Drop the shapes in batches until there are as many as chosen.
        if (m_Spawned >= m_ShapeCount)
            return;

        m_SpawnTime += Time.deltaTime;
        if (m_SpawnTime < BatchInterval)
            return;

        m_SpawnTime = 0f;
        SpawnBatch(world);
    }

    // Drops a batch of shapes at random places along the top between the walls, cycling through a capsule, a circle, a square and a random rounded polygon.
    private void SpawnBatch(PhysicsWorld world)
    {
        ref var random = ref Random;

        var capsule = new CapsuleGeometry { center1 = new Vector2(-0.25f, 0f), center2 = new Vector2(0.25f, 0f), radius = 0.25f };
        var circle = new CircleGeometry { radius = 0.35f };
        var square = PolygonGeometry.CreateBox(new Vector2(0.7f, 0.7f));

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, gravityScale = ShapeGravityScale };
        var shapeDef = new PhysicsShapeDefinition();

        for (var i = 0; i < BatchSize && m_Spawned < m_ShapeCount; ++i)
        {
            bodyDef.position = new Vector2(random.NextFloat(-m_SpawnHalfWidth, m_SpawnHalfWidth), SpawnY);
            var body = world.CreateBody(bodyDef);

            switch (m_Spawned % 4)
            {
                case 0:
                    body.CreateShape(capsule, shapeDef);
                    break;

                case 1:
                    body.CreateShape(circle, shapeDef);
                    break;

                case 2:
                    body.CreateShape(square, shapeDef);
                    break;

                default:
                    body.CreateShape(SandboxUtility.CreateRandomPolygon(extent: 0.75f, radius: 0.1f, ref random), shapeDef);
                    break;
            }

            ++m_Spawned;
        }
    }
}
