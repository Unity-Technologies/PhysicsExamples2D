using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

[ExampleScene("Shapes", "Demonstrates the use of geometry fragmenting.")]
public sealed class Fragmenting : SandboxExampleBehaviour, PhysicsCallbacks.IContactCallback
{
    private bool m_OldAutoContactCallbacks;
    private PhysicsWorld.DrawFillOptions m_OldDrawFillOptions;

    private ControlsMenu.CustomButton m_LeftButton;
    private ControlsMenu.CustomButton m_RightButton;
    private ControlsMenu.CustomButton m_FireButton;

    private readonly PhysicsMask m_ObstacleMask = new(1);
    private readonly PhysicsMask m_GroundMask = new(2);
    private readonly PhysicsMask m_DestructibleMask = new(3);
    private readonly PhysicsMask m_DebrisMask = new(4);
    private readonly PhysicsMask m_ProjectileMask = new(5);

    private Color m_DestructibleColor;
    private PhysicsShape.ContactFilter m_DestructibleContactFilter;
    private PhysicsShape.SurfaceMaterial m_DestructibleSurfaceMaterial;

    private const float ProjectileRadius = 0.2f;
    private const float PlayerSpeed = 60f;
    private const float ProjectileSpeed = 40f;
    private Vector2 m_PlayerPosition;
    private CapsuleGeometry m_PlayerGeometry;
    private CircleGeometry m_FireGeometry;
    private PolygonGeometry m_DestructGeometry;
    private NativeArray<PolygonGeometry> m_FragmentGeometryMask;

    private enum FragmentColors
    {
        Off,
        White,
        Group,
        Individual
    };

    private float m_FragmentRadius;
    private int m_FragmentCount;
    private FragmentColors m_FragmentColors;
    private bool m_FragmentExplode;

    protected override float CameraSize => 28f;
    protected override Vector2 CameraPosition => new(0f, 14f);

    protected override void OnExampleEnable()
    {
        // Get the default world.
        var world = World;

        // Turn-on auto contact callbacks.
        m_OldAutoContactCallbacks = world.autoContactCallbacks;
        world.autoContactCallbacks = true;

        // Turn on interior drawing only.
        m_OldDrawFillOptions = world.drawFillOptions;
        world.drawFillOptions = PhysicsWorld.DrawFillOptions.Interior;

        // Set controls.
        {
            m_LeftButton = SandboxManager.ControlsMenu[2];
            m_RightButton = SandboxManager.ControlsMenu[1];
            m_FireButton = SandboxManager.ControlsMenu[0];

            m_LeftButton.Set("Left [←]");
            m_RightButton.Set("Right [→]");
            m_FireButton.Set("Fire [Spc]");
            m_FireButton.button.clickable.clicked += FirePressed;
        }

        // Set Overrides.
        SandboxManager.SetOverrideColorShapeState(false);

        m_DestructibleColor = Color.seaGreen;
        m_DestructibleContactFilter = new PhysicsShape.ContactFilter { categories = m_DestructibleMask, contacts = m_ProjectileMask | m_DebrisMask };
        m_DestructibleSurfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f, bounciness = 0.4f, tangentSpeed = 0.1f, customColor = m_DestructibleColor };

        m_PlayerGeometry = new CapsuleGeometry { center1 = Vector2.zero, center2 = Vector2.up, radius = 0.5f };
        m_DestructGeometry = PolygonGeometry.CreateBox(new Vector2(50f, 20f));

        m_PlayerPosition = new Vector2(0f, -8f);

        m_FragmentRadius = 2f;
        m_FragmentCount = 100;
        m_FragmentColors = FragmentColors.Individual;
        m_FragmentExplode = false;

        UpdateFragmentGeometry();
    }

    protected override void OnExampleDisable()
    {
        // Get the default world.
        var world = World;

        // Reset the callbacks.
        world.autoContactCallbacks = m_OldAutoContactCallbacks;

        // Reset the draw fill options.
        world.drawFillOptions = m_OldDrawFillOptions;

        // Unregister.
        m_FireButton.button.clickable.clicked -= FirePressed;

        // Dispose.
        if (m_FragmentGeometryMask.IsCreated)
            m_FragmentGeometryMask.Dispose();
    }

    protected override void SetupOptions()
    {
        // Fragment Radius.
        AddSlider("Fragment  Radius", m_FragmentRadius, 0.5f, 5f, v =>
        {
            m_FragmentRadius = v;
            UpdateFragmentGeometry();
        });

        // Fragment Count.
        AddSliderInt("Fragment Count", m_FragmentCount, 1, 300, v => m_FragmentCount = v);

        // Fragment Colors.
        AddEnum("Fragment Colors", m_FragmentColors, v => m_FragmentColors = v);

        // Fragment Explode.
        AddToggle("Fragment Explode", m_FragmentExplode, v => m_FragmentExplode = v);
    }

    protected override void SetupScene()
    {
        // Get the default world.
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_GroundMask, contacts = m_ProjectileMask | m_DebrisMask },
            };
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(500f, 50f), radius: 0f, new PhysicsTransform(Vector2.down * 40f)), shapeDef);
            groundBody.CreateShape(PolygonGeometry.CreateBox(new Vector2(500f, 50f), radius: 0f, new PhysicsTransform(Vector2.up * 68f)), shapeDef);
        }

        // Obstacles.
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(-23f, 1f) });
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = new PhysicsShape.ContactFilter { categories = m_ObstacleMask, contacts = m_DebrisMask },
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0f, bounciness = 0.4f, tangentSpeed = 0.5f, customColor = Color.gray2}
            };

            const float radius = 1.5f;
            for (var n = 0; n < 7; ++n)
            {
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, -8f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f - 4f, -5f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, -2f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f - 4f, 1f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, 4f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f - 4f, 7f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, 10f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f - 4f, 13f), radius = radius }, shapeDef);
                body.CreateShape(new CircleGeometry { center = new Vector2(n * 8f, 16f), radius = radius }, shapeDef);
            }
        }

        // Destructible Geometry.
        {
            var body = world.CreateBody(new PhysicsBodyDefinition { position = Vector2.up * 30f });
            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = m_DestructibleContactFilter,
                surfaceMaterial = m_DestructibleSurfaceMaterial
            };
            body.CreateShape(m_DestructGeometry, shapeDef);
        }
    }

    private void Update()
    {
        // Get the default world.
        var world = World;

        // Only allow player movement/fire if the world is not paused.
        if (!SandboxManager.WorldPaused)
        {
            // Fetch input.
            var currentKeyboard = Keyboard.current;
            var leftPressed = m_LeftButton.isPressed || currentKeyboard.leftArrowKey.isPressed;
            var rightPressed = m_RightButton.isPressed || currentKeyboard.rightArrowKey.isPressed;
            var firePressed = currentKeyboard.spaceKey.wasPressedThisFrame;

            // Movement.
            {
                var movement = PlayerSpeed * Time.deltaTime;

                if (leftPressed)
                    m_PlayerPosition.x -= movement;

                if (rightPressed)
                    m_PlayerPosition.x += movement;

                // Clamp movement.
                m_PlayerPosition.x = Mathf.Clamp(m_PlayerPosition.x, -26f, 26f);
            }

            // Fire.
            if (firePressed)
                FirePressed();
        }

        // Draw the Player.
        world.DrawGeometry(m_PlayerGeometry, m_PlayerPosition, Color.azure);
    }

    private void FirePressed()
    {
        // Finish if the world is paused.
        if (SandboxManager.WorldPaused)
            return;

        // Get the default world.
        var world = World;

        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            gravityScale = 0f,
            fastCollisionsAllowed = true,
            position = m_PlayerPosition + Vector2.up * (ProjectileRadius + 1.5f),
            linearVelocity = Vector2.up * ProjectileSpeed
        };
        var body = world.CreateBody(bodyDef);
        body.callbackTarget = this;

        var shapeDef = new PhysicsShapeDefinition
        {
            contactFilter = new PhysicsShape.ContactFilter { categories = m_ProjectileMask, contacts = m_DestructibleMask | m_GroundMask },
            contactEvents = true,
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.ghostWhite }
        };
        var shape = body.CreateShape(new CircleGeometry { radius = ProjectileRadius }, shapeDef);
        shape.callbackTarget = this;
    }

    private void UpdateFragmentGeometry()
    {
        // Create the fragment geometry mask using the composer.
        {
            m_FireGeometry = new CircleGeometry { radius = m_FragmentRadius };

            // Dispose of any existing fire geometry mask.
            if (m_FragmentGeometryMask.IsCreated)
                m_FragmentGeometryMask.Dispose();

            var composer = PhysicsComposer.Create();
            composer.AddLayer(m_FireGeometry, PhysicsTransform.identity);
            m_FragmentGeometryMask = composer.CreatePolygonGeometry(vertexScale: Vector2.one, Allocator.Persistent);
            composer.Destroy();
        }
    }

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        var shapeA = beginEvent.shapeA;
        var shapeB = beginEvent.shapeB;

        // Finish if either shape is invalid.
        // NOTE: We might've destroyed one in a previous callback.
        if (!shapeA.isValid || !shapeB.isValid)
            return;

        var categoryA = shapeA.contactFilter.categories;
        var categoryB = shapeB.contactFilter.categories;

        // Did we hit the ground?
        if (categoryA == m_GroundMask || categoryB == m_GroundMask)
        {
            // Yes, so destroy whatever hit the ground.
            var destroyShape = categoryA == m_GroundMask ? shapeB : shapeA;
            destroyShape.body.Destroy();
            return;
        }

        // Did we hit a destructable?
        if (categoryA == m_DestructibleMask || categoryB == m_DestructibleMask)
        {
            // Yes, so finish if it wasn't the projectile.
            // NOTE: It should only be but always be defensive.
            if (categoryA != m_ProjectileMask && categoryB != m_ProjectileMask)
                return;

            // Fetch the contact.
            var contact = beginEvent.contactId.contact;
            var hitPosition = contact.manifold.points[0].point;

            // Get the default world.
            var world = World;

            // Draw the fragment radius.
            world.DrawCircle(hitPosition, m_FragmentRadius, Color.ivory, 2f / 60f, PhysicsWorld.DrawFillOptions.Outline);

            // Fetch the destructible shapes.
            var destructibleShape = categoryA == m_DestructibleMask ? shapeA : shapeB;
            var destructibleBody = destructibleShape.body;
            using var destructibleShapes = destructibleBody.GetShapes();

            // Get the polygon geometry from the shapes.
            // NOTE: We know it's always polygon geometry so we don't have to convert it.
            var targetPolygons = new NativeList<PolygonGeometry>(initialCapacity: destructibleShapes.Length, Allocator.Temp);
            foreach (var shape in destructibleShapes)
            {
                if (shape.shapeType == PhysicsShape.ShapeType.Polygon)
                    targetPolygons.Add(shape.polygonGeometry);
            }

            // Create the target fragment geometry.
            var targetGeometry = new PhysicsDestructor.FragmentGeometry(destructibleBody.transform, targetPolygons.AsReadOnly());
            targetPolygons.Dispose();

            // Create the fragment points.
            ref var random = ref Random;
            var fragmentPoints = new NativeArray<Vector2>(m_FragmentCount, Allocator.Temp);
            fragmentPoints[0] = hitPosition;
            for (var i = 1; i < m_FragmentCount; ++i)
            {
                var rotate = new PhysicsRotate(random.NextFloat(0f, PhysicsMath.PI));
                var radius = random.NextFloat(0.05f, m_FragmentRadius);
                fragmentPoints[i] = hitPosition + rotate.direction * radius;
            }

            // Fragment the target geometry with the mask and fragment the results with the fragment points.
            // NOTE: This is the most complex operation, you don't have to mask the target geometry.
            var maskGeometry = new PhysicsDestructor.FragmentGeometry(hitPosition, m_FragmentGeometryMask);
            using var fragmentResults = PhysicsDestructor.Fragment(targetGeometry, maskGeometry, fragmentPoints, Allocator.Temp);

            // Dispose of the fragment points.
            fragmentPoints.Dispose();

            // Destroy both the destructible and the projectile.
            shapeA.body.Destroy();
            shapeB.body.Destroy();

            var fragmentTransform = fragmentResults.transform;

            // Create the remaining destructible geometry from the "unbroken" geometry (if we have any).
            if (fragmentResults.unbrokenGeometry.Length > 0)
            {
                var body = world.CreateBody(new PhysicsBodyDefinition { position = fragmentTransform.position, rotation = fragmentTransform.rotation });
                var shapeDef = new PhysicsShapeDefinition
                {
                    contactFilter = m_DestructibleContactFilter,
                    surfaceMaterial = m_DestructibleSurfaceMaterial
                };

                foreach (var geometry in fragmentResults.unbrokenGeometry)
                    body.CreateShape(geometry, shapeDef);
            }

            // Create the debris destructible geometry from the "broken" geometry (if we have any).
            {
                var brokenCount = fragmentResults.brokenGeometry.Length;

                // Create a batch of bodies.
                var bodyDef = new PhysicsBodyDefinition
                {
                    type = PhysicsBody.BodyType.Dynamic,
                    fastCollisionsAllowed = true,
                    position = fragmentTransform.position, rotation = fragmentTransform.rotation,
                    gravityScale = 4f
                };
                using var bodies = world.CreateBodyBatch(bodyDef, brokenCount);

                var shapeColor = m_FragmentColors switch
                {
                    FragmentColors.Off or FragmentColors.Individual => m_DestructibleColor,
                    FragmentColors.White => Color.ivory,
                    FragmentColors.Group => ShapeColor,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var shapeDef = new PhysicsShapeDefinition
                {
                    contactFilter = new PhysicsShape.ContactFilter { categories = m_DebrisMask, contacts = m_GroundMask | m_DestructibleMask | m_ObstacleMask },
                    contactEvents = true,
                    surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = shapeColor }
                };

                // Add a shape for each body.
                for (var i = 0; i < brokenCount; ++i)
                {
                    var body = bodies[i];
                    var geometry = fragmentResults.brokenGeometry[i];

                    if (m_FragmentColors == FragmentColors.Individual)
                        shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = ShapeColor };

                    var shape = body.CreateShape(geometry, shapeDef);
                    shape.callbackTarget = this;
                }
            }

            // Explode the debris.
            if (m_FragmentExplode)
            {
                world.Explode(new PhysicsWorld.ExplosionDefinition
                {
                    position = hitPosition + Vector2.up * m_FragmentRadius,
                    hitCategories = m_DebrisMask,
                    impulsePerLength = 4f,
                    radius = m_FragmentRadius * 3f
                });
            }
        }
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent) { }
}
