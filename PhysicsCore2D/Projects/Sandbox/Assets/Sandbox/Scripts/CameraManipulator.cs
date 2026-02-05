using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

public class CameraManipulator : MonoBehaviour
{
    public enum InputMode
    {
        Drag,
        Explode
    };

    public Camera Camera { get; private set; }

    public Vector2 ManipulatorActionPosition => Camera.ScreenToWorldPoint(m_Position.ReadValue<Vector2>());

    public Vector2 CameraPosition
    {
        get => m_CameraPosition;
        set
        {
            m_CameraPosition = value;
            Camera.transform.position = new Vector3(m_CameraPosition.x, m_CameraPosition.y, Camera.transform.position.z);
        }
    }

    public float CameraSize
    {
        get => m_CameraSize;
        set
        {
            m_CameraSize = value;
            Camera.orthographicSize = m_CameraSize / math.max(m_CameraZoom, 0f);
        }
    }

    public float CameraZoom
    {
        get => m_CameraZoom;
        set
        {
            m_CameraZoom = value;
            Camera.orthographicSize = m_CameraSize / math.max(m_CameraZoom, 0.00001f);
        }
    }

    public InputMode TouchMode
    {
        get => m_TouchMode;
        set
        {
            m_TouchMode = value;
            m_ManipulatorState = ManipulatorState.None;
        }
    }

    public bool DisableManipulators
    {
        get => m_DisableManipulators;
        set
        {
            m_DisableManipulators = value;
            m_ManipulatorState = ManipulatorState.None;
        }
    }

    public float ExplodeImpulse { get; set; }

    public int OverlapUI
    {
        get => m_OverlapUI;
        set => m_OverlapUI = math.max(value, 0);
    }

    private enum ManipulatorState
    {
        None,
        CameraPan,
        ObjectDrag
    }

    private SandboxManager m_SandboxManager;
    private bool m_DisableManipulators;
    private Vector2 m_CameraPosition;
    private float m_CameraSize;
    private float m_CameraZoom;
    private InputMode m_TouchMode;
    private ManipulatorState m_ManipulatorState = ManipulatorState.None;
    private Vector2 m_LastActionPosition;
    private PhysicsRelativeJoint m_DragJoint;
    private PhysicsBody m_DragGroundBody;
    private int m_OverlapUI;

    private InputAction m_Click;
    private InputAction m_Position;
    
    private void Awake()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        Camera = GetComponentInParent<Camera>();
        CameraPosition = Vector2.zero;
        m_TouchMode = InputMode.Drag;
        CameraZoom = 1f;
        CameraSize = 6f;

        m_Click = InputSystem.actions.FindAction("Click");
        m_Position = InputSystem.actions.FindAction("Point");
    }

    private void Update()
    {
        // Stop any manipulation if we're over any UI.
        if (OverlapUI != 0)
        {
            ResetInputMode();
            return;
        }

        // Fetch input.
        var actionWasPressedThisFrame = m_Click.WasPressedThisFrame();
        var actionWasReleasedThisFrame = m_Click.WasReleasedThisFrame();
        var actionPosition = (Vector2)Camera.ScreenToWorldPoint(m_Position.ReadValue<Vector2>());        
        var currentKeyboard = Keyboard.current;

        // Handle the manipulator mode.
        switch (m_ManipulatorState)
        {
            case ManipulatorState.None:
            {
                // Was the left button pressed?
                if (!DisableManipulators && actionWasPressedThisFrame)
                {
                    // Camera-Pan if we're currently pressing the left-ctrl.
                    if (currentKeyboard.leftCtrlKey.isPressed)
                    {
                        m_ManipulatorState = ManipulatorState.CameraPan;
                        m_LastActionPosition = actionPosition;
                        return;
                    }

                    // Handle the touch behaviour.
                    switch (TouchMode)
                    {
                        case InputMode.Drag:
                        {
                            var defaultWorld = PhysicsWorld.defaultWorld;
                            using var hits = defaultWorld.OverlapPoint(actionPosition, PhysicsQuery.QueryFilter.Everything);
                            foreach (var hit in hits)
                            {
                                var hitBody = hit.shape.body;
                                if (hitBody.type != PhysicsBody.BodyType.Dynamic)
                                    continue;

                                m_DragGroundBody = defaultWorld.CreateBody();
                                var relativeDefinition = new PhysicsRelativeJointDefinition
                                {
                                    bodyA = m_DragGroundBody,
                                    bodyB = hitBody,
                                    localAnchorA = new PhysicsTransform(actionPosition),
                                    localAnchorB = hitBody.GetLocalPoint(actionPosition),
                                    springLinearFrequency = 15f,
                                    springLinearDamping = 0.7f,
                                    springMaxForce = 1000f * hitBody.mass * defaultWorld.gravity.magnitude
                                };
                                m_DragJoint = defaultWorld.CreateJoint(relativeDefinition);
                                m_DragJoint.WakeBodies();

                                // Flag as dragging an object.
                                m_ManipulatorState = ManipulatorState.ObjectDrag;

                                break;
                            }

                            return;
                        }

                        case InputMode.Explode:
                        {
                            const float radius = 10f;
                            PhysicsWorld.defaultWorld.DrawCircle(actionPosition, radius, Color.orangeRed, 0.02f);
                            var explodeDef = new PhysicsWorld.ExplosionDefinition { position = actionPosition, radius = radius, falloff = 2f, impulsePerLength = ExplodeImpulse };

                            // Explode in all the worlds.
                            using var worlds = PhysicsWorld.GetWorlds();
                            foreach (var world in worlds)
                                world.Explode(explodeDef);

                            return;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // Zooming is based upon the mouse-wheel only.
                var currentMouse = Mouse.current;
                if (currentMouse != null)
                {
                    var mouseScroll = currentMouse.scroll;
                    if (mouseScroll != null)
                    {
                        var scrollDelta = mouseScroll.y.ReadValue() * 0.1f;
                        if (math.abs(scrollDelta) > 0f)
                        {
                            m_SandboxManager.CameraZoom += scrollDelta;

                            var newWorldPosition = (Vector2)Camera.ScreenToWorldPoint(currentMouse.position.ReadValue());
                            Camera.transform.position -= (Vector3)(newWorldPosition - actionPosition);
                        }
                    }
                }

                return;
            }

            case ManipulatorState.CameraPan:
            {
                if (actionWasReleasedThisFrame)
                {
                    ResetInputMode();
                    return;
                }
                
                // Fetch the world mouse position.
                var worldDelta = actionPosition - m_LastActionPosition;
                if (worldDelta.sqrMagnitude < 0.01f)
                    return;

                m_LastActionPosition = actionPosition - worldDelta;
                Camera.transform.position -= (Vector3)worldDelta;
                return;
            }

            case ManipulatorState.ObjectDrag:
            {
                if (actionWasReleasedThisFrame)
                {
                    ResetInputMode();
                    return;
                }

                // Update drag target.
                var oldTarget = m_DragJoint.bodyA.GetWorldPoint(m_DragJoint.localAnchorA.position);
                m_DragJoint.localAnchorA = new PhysicsTransform(actionPosition);
                m_DragJoint.WakeBodies();

                // Get the default world.
                var world = PhysicsWorld.defaultWorld;
                
                var bodyB = m_DragJoint.bodyB;
                world.DrawLine(actionPosition, bodyB.GetWorldPoint(m_DragJoint.localAnchorB.position), Color.grey);
                world.DrawLine(oldTarget, actionPosition, Color.whiteSmoke);
                world.DrawPoint(oldTarget, 0.05f, Color.darkGreen);
                world.DrawPoint(actionPosition, 0.05f, Color.green);

                return;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetInputMode()
    {
        if (m_DragGroundBody.isValid)
            m_DragGroundBody.Destroy();

        if (m_DragJoint.isValid)
            m_DragJoint.Destroy();

        m_ManipulatorState = ManipulatorState.None;
    }

    public void ResetPanZoom()
    {
        Camera.transform.position = new Vector3(m_CameraPosition.x, m_CameraPosition.y, Camera.transform.position.z);
        CameraZoom = 1f;
    }
}