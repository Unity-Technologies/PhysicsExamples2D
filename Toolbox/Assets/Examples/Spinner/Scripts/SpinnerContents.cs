using System.Collections.Generic;

using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Spins a long paddle in the middle of a round room full of thousands of small debris pieces, to stress-test how the simulation copes with a great many bodies being churned at once.
/// The room is a Physics Area Contour circle of segments, and the paddle and every debris piece is one Physics Pose with a Physics Area, instantiated from a prefab.
/// </summary>
/// <remarks>
/// The paddle is either a kinematic body that is simply given an angular velocity, or a dynamic body held at its middle by a Physics Constraint Hinge whose motor turns it.
/// Only the debris settings and the choice of paddle rebuild the scene, while the motor speed, motor torque and gravity scale act on the running paddle and world.
/// The world's own gravity is put back when the example unloads.
/// </remarks>
public sealed class SpinnerContents : MonoBehaviour
{
    /// <summary>
    /// Destroys the paddle and every debris piece, then builds them again with the current settings.
    /// </summary>
    public void Rebuild()
    {
        Clear();

        SpawnPaddle();
        SpawnDebris();
    }

    /// <summary>
    /// Destroys the paddle and every debris piece, leaving the room alone.
    /// </summary>
    public void Clear()
    {
        foreach (var spawned in m_Spawned)
        {
            if (spawned != null)
                Destroy(spawned);
        }

        m_Spawned.Clear();
        m_Paddle = null;
        m_Hinge = null;
    }

    /// <summary>
    /// How fast the paddle turns, in degrees per second.
    /// Changing this turns the running paddle at the new speed without rebuilding the scene.
    /// </summary>
    public float motorSpeed
    {
        get => m_MotorSpeed;
        set
        {
            m_MotorSpeed = value;

            if (m_KinematicSpinner)
            {
                if (m_Paddle != null && m_Paddle.body.isValid)
                {
                    var body = m_Paddle.body;
                    body.angularVelocity = m_MotorSpeed;
                }
            }
            else
            {
                UpdateHinge();
            }
        }
    }

    /// <summary>
    /// The most torque the hinge motor can apply to turn a dynamic paddle.
    /// A kinematic paddle has no hinge, so this has no effect on it.
    /// </summary>
    public float maxMotorTorque
    {
        get => m_MaxMotorTorque;
        set
        {
            m_MaxMotorTorque = value;
            UpdateHinge();
        }
    }

    /// <summary>
    /// Whether the paddle is a kinematic body turned directly, instead of a dynamic body turned by a hinge motor.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public bool kinematicSpinner
    {
        get => m_KinematicSpinner;
        set => m_KinematicSpinner = value;
    }

    /// <summary>
    /// How many debris pieces are placed in the room.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public int debrisCount
    {
        get => m_DebrisCount;
        set => m_DebrisCount = value;
    }

    /// <summary>
    /// The friction of every debris piece.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float debrisFriction
    {
        get => m_DebrisFriction;
        set => m_DebrisFriction = value;
    }

    /// <summary>
    /// The bounciness of every debris piece, from zero for none to one for fully elastic.
    /// Changing this does not rebuild the scene until <see cref="Rebuild"/> is called.
    /// </summary>
    public float debrisBounciness
    {
        get => m_DebrisBounciness;
        set => m_DebrisBounciness = value;
    }

    /// <summary>
    /// How strong gravity is, as a multiple of the world's own gravity.
    /// Changing this scales gravity straight away without rebuilding the scene.
    /// </summary>
    public float gravityScale
    {
        get => m_GravityScale;
        set
        {
            m_GravityScale = value;

            var world = PhysicsWorld.defaultWorld;
            world.gravity = m_WorldGravity * m_GravityScale;
        }
    }

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;
        m_WorldGravity = world.gravity;
        world.gravity = m_WorldGravity * m_GravityScale;
    }

    // Gravity belongs to the world rather than to this scene, so it would otherwise stay scaled into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.gravity = m_WorldGravity;
    }

    private void Start() => Rebuild();

    // Creates the paddle in the middle of the room, left inactive until it is set up so its body is built with those settings.
    private void SpawnPaddle()
    {
        if (m_PaddlePrefab == null)
            return;

        var spawned = Instantiate(m_PaddlePrefab, PaddlePosition, Quaternion.identity);
        m_Paddle = spawned.GetComponent<PhysicsPose>();

        var bodyDefinition = m_Paddle.definition;
        bodyDefinition.type = m_KinematicSpinner ? PhysicsBody.BodyType.Kinematic : PhysicsBody.BodyType.Dynamic;
        bodyDefinition.angularVelocity = m_KinematicSpinner ? m_MotorSpeed : 0f;
        m_Paddle.definition = bodyDefinition;

        // A dynamic paddle is held by a hinge at its middle, whose motor turns it.
        if (!m_KinematicSpinner && m_Ground != null)
        {
            m_Hinge = spawned.AddComponent<PhysicsConstraintHinge>();
            m_Hinge.source = PhysicsConstraint.PoseSource.Custom;
            m_Hinge.poseA = m_Ground;
            m_Hinge.poseB = m_Paddle;

            var hingeDefinition = m_Hinge.definition;
            hingeDefinition.autoAnchorA = false;
            hingeDefinition.autoAnchorB = false;
            hingeDefinition.localAnchorA = new PhysicsTransform(new Vector2(PaddlePosition.x, PaddlePosition.y), PhysicsRotate.identity);
            hingeDefinition.localAnchorB = new PhysicsTransform(Vector2.zero, PhysicsRotate.identity);
            hingeDefinition.enableMotor = true;
            hingeDefinition.motorSpeed = m_MotorSpeed;
            hingeDefinition.maxMotorTorque = m_MaxMotorTorque;
            m_Hinge.definition = hingeDefinition;
        }

        spawned.SetActive(true);
        m_Spawned.Add(spawned);
    }

    // Fills the lower half of the room with debris, alternating capsules, circles and boxes in rows.
    private void SpawnDebris()
    {
        var x = -23f;
        var y = -30f;

        for (var i = 0; i < m_DebrisCount; ++i)
        {
            var prefab = (i % 3) switch
            {
                0 => m_CapsulePrefab,
                1 => m_CirclePrefab,
                _ => m_BoxPrefab
            };

            if (prefab != null)
            {
                var spawned = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity);

                var area = spawned.GetComponent<PhysicsArea>();
                var shapeDefinition = area.definition;
                var surfaceMaterial = shapeDefinition.surfaceMaterial;
                surfaceMaterial.friction = m_DebrisFriction;
                surfaceMaterial.bounciness = m_DebrisBounciness;
                shapeDefinition.surfaceMaterial = surfaceMaterial;
                area.definition = shapeDefinition;

                spawned.SetActive(true);
                m_Spawned.Add(spawned);
            }

            x += 0.5f;

            if (x >= 23f)
            {
                x = -23f;
                y += 0.5f;
            }
        }
    }

    // Writes the current motor speed and torque onto the running hinge, which wakes the paddle so it responds straight away.
    private void UpdateHinge()
    {
        if (m_Hinge == null)
            return;

        var definition = m_Hinge.definition;
        definition.motorSpeed = m_MotorSpeed;
        definition.maxMotorTorque = m_MaxMotorTorque;
        m_Hinge.definition = definition;

        m_Hinge.ApplyDefinition();
    }

    #region Internal

    // Where the middle of the paddle is, which is also where it is hinged.
    static readonly Vector2 PaddlePosition = new(0f, -20f);

    [SerializeField] PhysicsPose m_Ground;
    [SerializeField] GameObject m_PaddlePrefab;
    [SerializeField] GameObject m_CapsulePrefab;
    [SerializeField] GameObject m_CirclePrefab;
    [SerializeField] GameObject m_BoxPrefab;
    [SerializeField, Range(-360f, 360f)] float m_MotorSpeed = 200f;
    [SerializeField, Range(0f, 1000000f)] float m_MaxMotorTorque = 1000000f;
    [SerializeField] bool m_KinematicSpinner = true;
    [SerializeField, Range(1000, 5000)] int m_DebrisCount = 3000;
    [SerializeField, Range(0f, 1f)] float m_DebrisFriction = 0.1f;
    [SerializeField, Range(0f, 1f)] float m_DebrisBounciness = 0.1f;
    [SerializeField, Range(0f, 2f)] float m_GravityScale = 1f;

    readonly List<GameObject> m_Spawned = new();
    PhysicsPose m_Paddle;
    PhysicsConstraintHinge m_Hinge;
    Vector2 m_WorldGravity;

    #endregion
}
