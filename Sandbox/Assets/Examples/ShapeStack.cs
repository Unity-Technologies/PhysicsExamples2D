using System;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

// Run Tools > 2D > Physics > Rebuild Sandbox Registry after adding or renaming this class.
[ExampleScene("Shapes", "Checking the stability of stacking shapes.")]
public sealed class ShapeStack : SandboxExampleBehaviour
{
    private enum ObjectType
    {
        Circle = 0,
        Capsule = 1,
        Box = 2,
        Mix = 3
    }

    private ObjectType m_ObjectType;
    private int m_StackHeight;
    private float m_ContactFrequency;
    private float m_ContactDampingRatio;
    private float m_ContactSpeed;
    private float m_GravityScale;

    private float m_OldContactFrequency;
    private float m_OldContactDamping;
    private float m_OldContactSpeed;
    private Vector2 m_OldGravity;

    protected override float CameraSize => 6f;
    protected override Vector2 CameraPosition => new(0f, 5f);

    protected override void OnExampleEnable()
    {
        m_ObjectType = ObjectType.Circle;
        m_StackHeight = 8;

        var world = World;
        m_OldContactFrequency = world.contactFrequency;
        m_OldContactDamping = world.contactDamping;
        m_OldContactSpeed = world.contactSpeed;
        m_OldGravity = world.gravity;

        m_ContactFrequency = m_OldContactFrequency;
        m_ContactDampingRatio = m_OldContactDamping;
        m_ContactSpeed = m_OldContactSpeed;
        m_GravityScale = 1f;
    }

    protected override void OnExampleDisable()
    {
        var world = World;
        world.contactFrequency = m_OldContactFrequency;
        world.contactDamping = m_OldContactDamping;
        world.contactSpeed = m_OldContactSpeed;
        world.gravity = m_OldGravity;
    }

    protected override void SetupOptions()
    {
        // The contact/gravity sliders apply live to this captured world handle.
        var world = World;

        // Object Type.
        AddEnum("Object Type", m_ObjectType, v =>
        {
            m_ObjectType = v;
            RebuildScene();
        });

        // Stack Height.
        AddSliderInt("Stack Height", m_StackHeight, 2, 20, v =>
        {
            m_StackHeight = v;
            RebuildScene();
        });

        // Contact Frequency.
        AddSlider("Contact Frequency ", m_ContactFrequency, 0f, 120f, v =>
        {
            m_ContactFrequency = v;
            world.contactFrequency = m_ContactFrequency;
        });

        // Contact Damping.
        AddSlider("Contact Damping", m_ContactDampingRatio, 0f, 100f, v =>
        {
            m_ContactDampingRatio = v;
            world.contactDamping = m_ContactDampingRatio;
        });

        // Contact Speed.
        AddSlider("Contact Speed", m_ContactSpeed, 0f, 10f, v =>
        {
            m_ContactSpeed = v;
            world.contactSpeed = m_ContactSpeed;
        });

        // Gravity Scale.
        AddSlider("Gravity Scale", m_GravityScale, 0f, 20f, v =>
        {
            m_GravityScale = v;
            world.gravity = m_OldGravity * m_GravityScale;
        });
    }

    protected override void SetupScene()
    {
        var world = World;

        // Ground.
        {
            var groundBody = world.CreateBody();
            groundBody.CreateShape(new SegmentGeometry { point1 = new Vector2(-100f, 0f), point2 = new Vector2(100f, 0f) }, PhysicsShapeDefinition.defaultDefinition);
        }

        // Stack.
        {
            CreateStack(new Vector2(0f, 0.55f));
        }
    }

    private void CreateStack(Vector2 position)
    {
        var world = World;

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.3f } };

        for (var i = 0; i < m_StackHeight; ++i)
        {
            bodyDef.position = position + new Vector2(0f, i * 1.2f);
            var body = world.CreateBody(bodyDef);

            shapeDef.surfaceMaterial.customColor = ShapeColor;

            switch (m_ObjectType)
            {
                case ObjectType.Circle:
                {
                    CreateCircle(body, shapeDef);
                    continue;
                }

                case ObjectType.Capsule:
                {
                    CreateCapsule(body, shapeDef);
                    continue;
                }

                case ObjectType.Box:
                {
                    CreateBox(body, shapeDef);
                    continue;
                }

                case ObjectType.Mix:
                {
                    switch (i % 4)
                    {
                        case 0:
                        {
                            CreateCircle(body, shapeDef);
                            continue;
                        }

                        case 1:
                        {
                            CreateCapsule(body, shapeDef);
                            continue;
                        }

                        case 2:
                        {
                            CreateBox(body, shapeDef);
                            continue;
                        }

                        default:
                            continue;
                    }
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void CreateCircle(PhysicsBody body, PhysicsShapeDefinition shapeDef)
    {
        var circleGeometry = new CircleGeometry { center = Vector2.zero, radius = 0.5f };
        body.CreateShape(circleGeometry, shapeDef);
    }

    private static void CreateCapsule(PhysicsBody body, PhysicsShapeDefinition shapeDef)
    {
        var capsuleGeometry = new CapsuleGeometry
        {
            center1 = new Vector2(0f, -0.25f),
            center2 = new Vector2(0f, 0.25f),
            radius = 0.25f
        };
        body.CreateShape(capsuleGeometry, shapeDef);
    }

    private static void CreateBox(PhysicsBody body, PhysicsShapeDefinition shapeDef)
    {
        var boxSize = new Vector2(1f, 1f);
        const float boxRadius = 0f;
        var polygonGeometry = PolygonGeometry.CreateBox(size: boxSize, radius: boxRadius);
        body.CreateShape(polygonGeometry, shapeDef);
    }
}
