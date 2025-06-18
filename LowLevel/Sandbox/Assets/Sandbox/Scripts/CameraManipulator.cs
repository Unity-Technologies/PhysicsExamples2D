using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;

using PhysicsJoint = UnityEngine.LowLevelPhysics2D.PhysicsJoint;

public class CameraManipulator : MonoBehaviour
{
    public enum InputMode
    {
        DragObject,
        Explode
    };
    
    public Camera Camera { get; private set; }
    
    public Vector2 CameraStartPosition
    {
        get => m_CameraStartPosition;
        set { m_CameraStartPosition = value; Camera.transform.position = new Vector3(m_CameraStartPosition.x, m_CameraStartPosition.y, Camera.transform.position.z); }
    }
    
    public float CameraSize
    {
        get => m_CameraSize;
        set { m_CameraSize = value; Camera.orthographicSize = m_CameraSize / math.max(m_CameraZoom, 0f); }
    }
    
    public float CameraZoom
    {
        get => m_CameraZoom;
        set { m_CameraZoom = value; Camera.orthographicSize = m_CameraSize / math.max(m_CameraZoom, 0.00001f); }
    }

    public InputMode TouchMode
    {
        get => m_TouchMode;
        set { m_TouchMode = value; m_ManipulatorState = ManipulatorState.None; }
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
    private Vector2 m_CameraStartPosition;
    private float m_CameraSize;
    private float m_CameraZoom;
    private InputMode m_TouchMode;
    private ManipulatorState m_ManipulatorState = ManipulatorState.None;
    private Vector3 m_WorldLastPosition;
    private PhysicsTargetJoint m_DragJoint;
    private PhysicsBody m_DragGroundBody;
    private int m_OverlapUI;

    private void Awake()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        Camera = GetComponentInParent<Camera>();
        CameraStartPosition = Vector2.zero;
        m_TouchMode = InputMode.DragObject;
        CameraZoom = 1f;
        CameraSize = 6f;
    }

    private void Update()
    {
        // Stop any manipulation if we're over any UI.
        if (OverlapUI != 0)
        {
            ResetInputMode();
            return;
        }
        
        // Fetch input devices.
        var currentKeyboard = Keyboard.current;
        var currentMouse = Mouse.current;
        
        // Handle the manipulator mode.
        switch (m_ManipulatorState)
        {
            case ManipulatorState.None:
            {
                // Yes, so fetch the world mouse position.
                var worldPosition = Camera.ScreenToWorldPoint(currentMouse.position.ReadValue());
                
                // Was the left button pressed?
                if (!DisableManipulators && currentMouse.leftButton.wasPressedThisFrame)
                {
                    // Camera-Pan if we're currently pressing the left-ctrl.
                    if (currentKeyboard.leftCtrlKey.isPressed)
                    {
                        m_ManipulatorState = ManipulatorState.CameraPan;
                        m_WorldLastPosition = worldPosition;
                        return;
                    }
                    
                    // Handle the touch behaviour.
                    switch (TouchMode)
                    {
                        case InputMode.DragObject:
                        {
                            var defaultWorld = PhysicsWorld.defaultWorld;
                            using var hits = defaultWorld.OverlapPoint(worldPosition, PhysicsQuery.QueryFilter.Everything);
                            foreach (var hit in hits)
                            {
                                var hitBody = hit.shape.body;
                                if (hitBody.bodyType != RigidbodyType2D.Dynamic)
                                    continue;

                                m_DragGroundBody = defaultWorld.CreateBody(PhysicsBodyDefinition.defaultDefinition);
                                var targetDefinition = new PhysicsTargetJointDefinition
                                {
                                    bodyA = m_DragGroundBody,
                                    bodyB = hitBody,
                                    localAnchorA = new PhysicsTransform(worldPosition),
                                    localAnchorB = hitBody.GetLocalPoint(worldPosition),
                                    springHertz = 5f,
                                    springDampingRatio = 0.7f,
                                    maxForce = 1000f * hitBody.mass
                                };
                                m_DragJoint = defaultWorld.CreateJoint(targetDefinition);
                                hitBody.awake = true;

                                // Flag as dragging an object.
                                m_ManipulatorState = ManipulatorState.ObjectDrag;

                                break;
                            }

                            return;
                        }

                        case InputMode.Explode:
                        {
                            const float radius = 10f;
                            PhysicsWorld.defaultWorld.DrawCircle(worldPosition, radius, Color.orangeRed, 0.02f);
                            var explodeDef = new PhysicsWorld.ExplodeDefinition { position = worldPosition, radius = radius, falloff = 2f, impulsePerLength = ExplodeImpulse };
                            
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

                var scrollDelta = currentMouse.scroll.y.ReadValue() * 0.1f;
                if (math.abs(scrollDelta) > 0f)
                {
                    m_SandboxManager.CameraZoom += scrollDelta;
                    var newWorldPosition = Camera.ScreenToWorldPoint(currentMouse.position.ReadValue());
                    Camera.transform.position -= newWorldPosition - worldPosition;
                }

                return;
            }

            case ManipulatorState.CameraPan:
            {
                if (currentMouse.leftButton.wasReleasedThisFrame)
                {
                    m_ManipulatorState = ManipulatorState.None;
                    return;
                }

                // Fetch the world mouse position.
                var worldPosition = Camera.ScreenToWorldPoint(currentMouse.position.ReadValue());
                var worldDelta = worldPosition - m_WorldLastPosition;
                if (worldDelta.sqrMagnitude < 0.01f)
                    return;
    
                m_WorldLastPosition = worldPosition - worldDelta;
                Camera.transform.position -= worldDelta;
                return;
            }

            case ManipulatorState.ObjectDrag:
            {
                if (currentMouse.leftButton.wasReleasedThisFrame)
                {
                    ResetInputMode();
                    return;
                }
                
                // Update drag target.
                var oldTarget = m_DragJoint.bodyA.GetWorldPoint(m_DragJoint.localAnchorA.position);
                var target = Camera.ScreenToWorldPoint(Input.mousePosition);
                m_DragJoint.localAnchorA = new PhysicsTransform(target);

                var world = PhysicsWorld.defaultWorld;
                world.DrawLine(oldTarget, target, Color.whiteSmoke);                
                world.DrawPoint(oldTarget, 0.05f, Color.darkGreen);
                world.DrawPoint(target, 0.05f, Color.green);

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
        Camera.transform.position = new Vector3(m_CameraStartPosition.x, m_CameraStartPosition.y, Camera.transform.position.z);
        CameraZoom = 1f;
    }
}
