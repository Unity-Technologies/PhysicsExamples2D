using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;
using Random = Unity.Mathematics.Random;

public class ParallelSimulateSpawner : MonoBehaviour
{
    [Range(0.1f, 1f)] public float Delay = 0.1f;
    [Range(1, 1000)] public int SpawnCount = 10;
    [Range(0, 1000)] public int Total = 1000;
    [Range(0f, 360f)] public float Spread = 30f;
    public Vector2 Radius = new (0.1f, 0.5f);
    public Vector2 Speed = new (5f, 10f);
    public Vector2 Direction = Vector2.up;
    
    public PhysicsBodyDefinition BodyDef = PhysicsBodyDefinition.defaultDefinition;
    public PhysicsShapeDefinition ShapeDef = PhysicsShapeDefinition.defaultDefinition;

    private float m_DelayTime;
    private int m_Count;
    private Random m_Random;
    private SceneWorld m_SceneWorld;

    private SandboxManager m_SandboxManager;

    public void ResetScene()
    {
        m_DelayTime = 0f;
        m_Count = 0;
        m_Random = new Random(0x01234567);
        
        m_SceneWorld = SceneWorld.FindSceneWorld(gameObject);
        
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
    }
    
    private void OnEnable()
    {
        ResetScene();
    }

    private void Update()
    {
        if (m_Count >= Total)
            return;
        
        m_DelayTime += Time.deltaTime;
        if (m_DelayTime < Delay) return;
        
        m_DelayTime = 0f;

        CreateFountainProjectiles();
    }
    
    private void CreateFountainProjectiles()
    {
        var spawnCount = math.min(SpawnCount, Total - m_Count);
        if (spawnCount == 0)
            return;

        m_Count += spawnCount;
        
        var circleGeometry = new CircleGeometry()
        {
            center = Vector2.zero,
            radius = m_Random.NextFloat(Radius.x, Radius.y)
        };
        
        var definitions = new NativeArray<PhysicsBodyDefinition>(spawnCount, Allocator.Temp);

        var directionAngle = new PhysicsRotate(Direction).angle;
        Vector2 position = transform.position;
        
        // Fire all the projectiles.
        for (var i = 0; i < spawnCount; ++i)
        {
            // Calculate the fire spread.
            var halfSpread = Spread * 0.5f;
            var direction = new PhysicsRotate(math.radians(m_Random.NextFloat(-halfSpread, halfSpread)) + directionAngle).direction;
            var speed = m_Random.NextFloat(Speed.x, Speed.y);

            // Create the projectile body.
            BodyDef.position = position;
            BodyDef.rotation = new PhysicsRotate(m_Random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI));
            BodyDef.linearVelocity = direction * speed;
            
            definitions[i] = BodyDef;
        }

        // Create the bodies.
        using var bodyBatch = m_SceneWorld.World.CreateBodyBatch(definitions);

        // Add the bodies to the sandbox manager.
        var bodies = m_SandboxManager.Bodies;
        foreach (var body in bodyBatch)
            bodies.Add(body);
        
        // Create the projectiles.
        for (var i = 0; i < spawnCount; ++i)
        {
            // Create the projectile shape.
            ShapeDef.surfaceMaterial.customColor = new Color32((byte)m_Random.NextFloat(0, 256), (byte)m_Random.NextFloat(0, 256), (byte)m_Random.NextFloat(0, 256), 255);
            bodyBatch[i].CreateShape(circleGeometry, ShapeDef);
        }

        // Dispose.
        definitions.Dispose();
    }
}
