using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// PhysicsChain ground geometry. A long undulating ground built from ~20 control points using
/// `body.CreateChain(ChainGeometry(...), PhysicsChainDefinition)`. Bodies of various types spawn at the
/// upper-left and slide along the chain. Toggle `FastCollisionsAllowed` to compare CCD vs default.
/// </summary>
public class ChainShape : MonoBehaviour
{
    public enum ObjectType { Circle = 0, Capsule = 1, Box = 2 }

    public ObjectType Type = ObjectType.Box;
    public int ObjectCount = 100;
    public float GravityScale = 10f;
    public bool FastCollisionsAllowed = false;

    private const float Friction = 0.1f;
    private Vector2 m_OldGravity;
    private int m_ItemsSpawned;
    private const float SpawnPeriod = 1.75f;
    private float m_SpawnTime;

    private void OnEnable()
    {
        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        PhysicsWorld.defaultWorld.gravity = m_OldGravity * GravityScale;
        SetupScene();
    }

    private void OnDisable()
    {
        PhysicsWorld.defaultWorld.gravity = m_OldGravity;
    }

    private void Update()
    {
        if (m_ItemsSpawned >= ObjectCount)
            return;

        m_SpawnTime -= Time.deltaTime;
        if (m_SpawnTime > 0)
            return;

        m_SpawnTime = SpawnPeriod / math.sqrt(GravityScale);
        ++m_ItemsSpawned;

        var world = PhysicsWorld.defaultWorld;

        var startPosition = new Vector2(-55f, 13.5f);
        var startLinearVelocity = new Vector2(2f, -1f);

        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic, fastCollisionsAllowed = FastCollisionsAllowed, position = startPosition, linearVelocity = startLinearVelocity };
        var body = world.CreateBody(bodyDef);

        var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = Friction, bounciness = 0f } };

        switch (Type)
        {
            case ObjectType.Circle:
                body.CreateShape(new CircleGeometry { radius = 1f }, shapeDef);
                break;
            case ObjectType.Capsule:
                body.CreateShape(new CapsuleGeometry { center1 = new Vector2(-0.75f, 0f), center2 = new Vector2(0.75f, 0f), radius = 0.75f }, shapeDef);
                break;
            case ObjectType.Box:
                body.CreateShape(PolygonGeometry.CreateBox(new Vector2(2f, 2f)), shapeDef);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetupScene()
    {
        m_ItemsSpawned = 0;
        m_SpawnTime = 0f;

        var world = PhysicsWorld.defaultWorld;

        // Chain ground built from ~20 hand-picked control points. CreateChain handles the segment connectivity
        // so neighbouring segments share normal-direction continuity (no internal "ghost" collisions).
        var groundBody = world.CreateBody();

        using var points = new NativeList<Vector2>(Allocator.Temp)
        {
            new(-60.885498f, 12.8985004f), new(-60.885498f, 16.2057495f), new(60.885498f, 16.2057495f), new(60.885498f, -30.2057514f),
            new(51.5935059f, -30.2057514f), new(43.6559982f, -10.9139996f), new(35.7184982f, -10.9139996f), new(27.7809982f, -10.9139996f),
            new(21.1664963f, -14.2212505f), new(11.9059982f, -16.2057514f), new(0f, -16.2057514f), new(-10.5835037f, -14.8827496f),
            new(-17.1980019f, -13.5597477f), new(-21.1665001f, -12.2370014f), new(-25.1355019f, -9.5909977f), new(-31.75f, -3.63799858f),
            new(-38.3644981f, 5.0840004f), new(-42.3334999f, 8.59125137f), new(-47.625f, 10.5755005f), new(-60.885498f, 12.8985004f),
        };

        var chainGeometry = new ChainGeometry(points.AsArray());
        var chainDef = new PhysicsChainDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f, bounciness = 0f } };
        groundBody.CreateChain(chainGeometry, chainDef);
    }
}
