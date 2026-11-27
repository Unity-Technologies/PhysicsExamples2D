using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Unity.U2D.Physics;

public class CameraManipulator : MonoBehaviour
{
    public enum InputMode
    {
        Drag,
        Explode
    };

    public Camera Camera { get; private set; }

    // Pan, zoom and picking all run through this, so the same behavior works with an orthographic or a perspective camera.
    public CameraProjection Projection { get; private set; }

    public Vector2 ManipulatorActionPosition => Projection.ScreenToPlanePoint(m_Position.ReadValue<Vector2>());

    public Vector2 CameraPosition
    {
        get => m_CameraPosition;
        set
        {
            m_CameraPosition = value;
            m_PendingPan = Vector2.zero;
            Projection.MoveTo(m_CameraPosition);
        }
    }

    public float CameraSize
    {
        get => m_CameraSize;
        set
        {
            m_CameraSize = value;
            Projection.SetFraming(m_CameraSize, m_CameraZoom);
        }
    }

    public float CameraZoom
    {
        get => m_CameraZoom;
        set
        {
            m_CameraZoom = value;
            Projection.SetFraming(m_CameraSize, m_CameraZoom);
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

    private enum ManipulatorState
    {
        None,
        CameraPan,
        ObjectDrag
    }

    private WorkshopManager m_WorkshopManager;
    private bool m_DisableManipulators;
    private Vector2 m_CameraPosition;
    private float m_CameraSize;
    private float m_CameraZoom;
    private InputMode m_TouchMode;
    private ManipulatorState m_ManipulatorState = ManipulatorState.None;
    private bool m_PanViaRightButton;
    private Vector2 m_LastActionPosition;
    private PhysicsRelativeJoint m_DragJoint;
    private PhysicsBody m_DragGroundBody;
    private IPanel m_UIPanel;

    private InputAction m_Click;
    private InputAction m_Position;

    // How long the camera takes to ease most of the way through a mouse-wheel zoom or a pan, in seconds; zero follows them at once.
    private const float Smoothing = 0.1f;

    // The zoom the mouse wheel asked for, which the camera eases toward, and the zoom as the easing last left it, so a zoom set from elsewhere can be told apart.
    private float m_TargetZoom;
    private float m_AppliedZoom;

    // The pan the camera still has to make, measured on the physics plane in meters, which it eases through.
    private Vector2 m_PendingPan;

    // The smallest zoom the mouse wheel can ask for, which keeps the eased zoom above zero.
    private const float MinimumZoom = 0.01f;

    // Set up here rather than in Awake because Unity calls this again after a script reload while playing, and Awake it does not.
    private void OnEnable()
    {
        m_WorkshopManager = FindAnyObjectByType<WorkshopManager>();
        Camera = GetComponentInParent<Camera>();
        Projection = CameraProjection.For(Camera);
        m_TouchMode = InputMode.Drag;

        // Take the framing from how the camera is set up in the example's own scene, rather than imposing one.
        // The fields are set directly because the camera is already in this state, so there is nothing to apply.
        m_CameraZoom = 1f;
        m_CameraSize = Projection.framing;
        m_CameraPosition = Projection.planePosition;
        m_TargetZoom = m_CameraZoom;
        m_AppliedZoom = m_CameraZoom;
        m_PendingPan = Vector2.zero;

        m_Click = InputSystem.actions.FindAction("Click");
        m_Position = InputSystem.actions.FindAction("Point");
    }

    private void Update()
    {
        // Stop any manipulation if the pointer is over any menu UI.
        if (IsPointerOverUI())
        {
            ResetInputMode();
            return;
        }

        // Fetch input.
        var actionWasPressedThisFrame = m_Click.WasPressedThisFrame();
        var actionWasReleasedThisFrame = m_Click.WasReleasedThisFrame();
        var actionPosition = Projection.ScreenToPlanePoint(m_Position.ReadValue<Vector2>());
        var currentKeyboard = Keyboard.current;

        // Handle the manipulator mode.
        switch (m_ManipulatorState)
        {
            case ManipulatorState.None:
            {
                // Camera-Pan on a right-mouse drag (an alternative to ctrl + left-drag, kept because
                // a right mouse button may be unavailable on some platforms).
                if (!DisableManipulators && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                {
                    m_ManipulatorState = ManipulatorState.CameraPan;
                    m_PanViaRightButton = true;
                    m_LastActionPosition = actionPosition;
                    return;
                }

                // Was the left button pressed?
                if (!DisableManipulators && actionWasPressedThisFrame)
                {
                    // Camera-Pan if we're currently pressing either ctrl.
                    if (currentKeyboard.leftCtrlKey.isPressed || currentKeyboard.rightCtrlKey.isPressed)
                    {
                        m_ManipulatorState = ManipulatorState.CameraPan;
                        m_PanViaRightButton = false;
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
                        // The wheel moves the target zoom, which the camera eases toward in LateUpdate.
                        var scrollDelta = mouseScroll.y.ReadValue() * 0.1f;
                        if (math.abs(scrollDelta) > 0f)
                            m_TargetZoom = math.max(m_TargetZoom + scrollDelta, MinimumZoom);
                    }
                }

                return;
            }

            case ManipulatorState.CameraPan:
            {
                // End the pan when the button that started it is released.
                var panWasReleasedThisFrame = m_PanViaRightButton
                    ? (Mouse.current == null || Mouse.current.rightButton.wasReleasedThisFrame)
                    : actionWasReleasedThisFrame;
                if (panWasReleasedThisFrame)
                {
                    ResetInputMode();
                    return;
                }

                // The pan owed is how far the point grabbed has moved from under the pointer, which the camera eases through in LateUpdate.
                m_PendingPan = actionPosition - m_LastActionPosition;
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

    // Eases the camera toward the zoom the mouse wheel asked for and through the pan still owed, so the view glides rather than jumps.
    // It runs after Update every frame, whatever Update returned early for, so the easing finishes after the wheel stops or a pan is released.
    private void LateUpdate()
    {
        // A zoom set from elsewhere, such as the zoom slider, an example's framing or a reset, becomes the target at once, so it is not eased.
        if (m_CameraZoom != m_AppliedZoom)
            m_TargetZoom = m_CameraZoom;

        // How far toward the target the camera moves this frame, the same over a second whatever the frame rate; all the way with no smoothing.
        var blend = Smoothing > 0f ? 1f - math.exp(-Time.unscaledDeltaTime / Smoothing) : 1f;

        if (m_TargetZoom != m_CameraZoom)
        {
            // The zoom eases by ratio rather than by difference, so a zoom step looks the same speed zoomed in or out.
            var currentZoom = math.max(m_CameraZoom, MinimumZoom);
            var zoom = math.exp(math.lerp(math.log(currentZoom), math.log(m_TargetZoom), blend));
            if (math.abs(zoom - m_TargetZoom) < 0.0001f)
                zoom = m_TargetZoom;

            var cursor = m_Position.ReadValue<Vector2>();
            var before = Projection.ScreenToPlanePoint(cursor);

            m_WorkshopManager.CameraZoom = zoom;

            // The zoom slider holds the zoom inside its range, so a target past the range is dropped once the zoom reaches its edge.
            if (m_CameraZoom != zoom)
                m_TargetZoom = m_CameraZoom;

            // Keep whatever was under the pointer before this frame's zoom under it afterwards.
            Projection.PanBy(Projection.ScreenToPlanePoint(cursor) - before);
        }

        m_AppliedZoom = m_CameraZoom;

        if (m_PendingPan == Vector2.zero)
            return;

        var pan = m_PendingPan.sqrMagnitude < 0.000001f ? m_PendingPan : m_PendingPan * blend;
        m_PendingPan -= pan;
        Projection.PanBy(pan);
    }

    private void ResetInputMode()
    {
        if (m_DragGroundBody.isValid)
            m_DragGroundBody.Destroy();

        if (m_DragJoint.isValid)
            m_DragJoint.Destroy();

        m_PanViaRightButton = false;
        m_ManipulatorState = ManipulatorState.None;
    }

    // True when the pointer is over any menu UI. Hit-tests the shared UI Toolkit panel each frame:
    // every panel root is picking-mode Ignore, so Pick only returns a non-null element when the
    // pointer is actually over a menu's content. This is robust to panel sort order and overlap,
    // unlike tracking PointerEnter/Leave per menu-region (which desyncs when regions occlude).
    private bool IsPointerOverUI()
    {
        // Resolve the shared panel lazily (all UIDocuments use the same PanelSettings → one panel).
        if (m_UIPanel == null)
        {
            var document = FindAnyObjectByType<UIDocument>();
            m_UIPanel = document != null ? document.rootVisualElement?.panel : null;
            if (m_UIPanel == null)
                return false;
        }

        var screenPosition = m_Position.ReadValue<Vector2>();
        var panelPosition = RuntimePanelUtils.ScreenToPanel(m_UIPanel, screenPosition);
        return m_UIPanel.Pick(panelPosition) != null;
    }

    public void ResetPanZoom()
    {
        m_PendingPan = Vector2.zero;
        Projection.MoveTo(m_CameraPosition);
        CameraZoom = 1f;
    }
}