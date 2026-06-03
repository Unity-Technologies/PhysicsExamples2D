using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// World-level explosion impulse. A 5×5 grid of small dynamic bodies; press Space to invoke
/// `PhysicsWorld.Explode(ExplosionDefinition)` centred at `ExplosionCenter`. The explosion applies an outward
/// impulse to every shape inside `ExplosionRadius` whose category bits match `HitCategories`.
///
/// `impulsePerLength` scales the impulse by the shape's perimeter inside the blast — larger shapes get more push.
/// </summary>
public class Explosion : MonoBehaviour
{
    public Vector2 ExplosionCenter = Vector2.zero;
    public float ExplosionRadius = 4f;
    public float ImpulsePerLength = 4f;

    private NativeArray<PhysicsBody> m_Bodies;

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;

        // 5×5 grid of small Dynamic circles.
        const int gridX = 5;
        const int gridY = 5;
        m_Bodies = new NativeArray<PhysicsBody>(gridX * gridY, Allocator.Persistent);
        var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
        var circle = new CircleGeometry { radius = 0.25f };
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;

        var idx = 0;
        for (var x = -gridX / 2; x <= gridX / 2; ++x)
        {
            for (var y = 0; y < gridY; ++y)
            {
                bodyDef.position = new Vector2(x, y);
                var body = world.CreateBody(bodyDef);
                body.CreateShape(circle, shapeDef);
                m_Bodies[idx++] = body;
            }
        }
    }

    private void OnDisable()
    {
        if (!m_Bodies.IsCreated)
            return;
        for (var i = 0; i < m_Bodies.Length; ++i)
            if (m_Bodies[i].isValid)
                m_Bodies[i].Destroy();
        m_Bodies.Dispose();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TriggerExplosion();
    }

    private void TriggerExplosion()
    {
        var world = PhysicsWorld.defaultWorld;
        world.Explode(new PhysicsWorld.ExplosionDefinition
        {
            position = ExplosionCenter,
            radius = ExplosionRadius,
            hitCategories = PhysicsMask.All,
            impulsePerLength = ImpulsePerLength
        });
    }
}
