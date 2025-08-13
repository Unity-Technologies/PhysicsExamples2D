using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class UserJoint : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private float m_JointFrequency;
    private float m_JointDamping;
    private float m_JointMaxForce;
    private float m_AnchorOffsetX;
    private float m_AnchorOffsetY;

    private readonly float[] m_Impulses = new float[2];
    private FloatField m_DisplayImpulse0;
    private FloatField m_DisplayImpulse1;
    private PhysicsBody m_Body;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 4f;
        m_CameraManipulator.CameraPosition = new Vector2(0f, -1f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;
        
        // Set Overrides.
        m_SandboxManager.SetOverrideColorShapeState(false);
        
        m_JointFrequency = 3f;
        m_JointDamping = 0.7f;
        m_JointMaxForce = 1000f;
        m_AnchorOffsetX = 0.5f;
        m_AnchorOffsetY = 1f;

        SetupOptions();

        SetupScene();

        PhysicsEvents.PostSimulate += OnUpdateJoint;
    }

    private void OnDisable()
    {
        PhysicsEvents.PostSimulate -= OnUpdateJoint;
        
        // Reset overrides.
        m_SandboxManager.ResetOverrideColorShapeState();
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Joint Frequency.
            var jointFrequency = root.Q<Slider>("joint-frequency");
            jointFrequency.value = m_JointFrequency;
            jointFrequency.RegisterValueChangedCallback(evt => { m_JointFrequency = evt.newValue; });

            // Joint Damping.
            var jointDamping = root.Q<Slider>("joint-damping");
            jointDamping.value = m_JointDamping;
            jointDamping.RegisterValueChangedCallback(evt => { m_JointDamping = evt.newValue; });

            // Joint Max Force.
            var jointMaxForce = root.Q<Slider>("joint-max-force");
            jointMaxForce.value = m_JointMaxForce;
            jointMaxForce.RegisterValueChangedCallback(evt => { m_JointMaxForce = evt.newValue; });

            // Anchor Offset X.
            var anchorOffsetX = root.Q<Slider>("anchor-offset-x");
            anchorOffsetX.value = m_AnchorOffsetX;
            anchorOffsetX.RegisterValueChangedCallback(evt => { m_AnchorOffsetX = evt.newValue; });

            // Anchor Offset Y.
            var anchorOffsetY = root.Q<Slider>("anchor-offset-y");
            anchorOffsetY.value = m_AnchorOffsetY;
            anchorOffsetY.RegisterValueChangedCallback(evt => { m_AnchorOffsetY = evt.newValue; });

            // Impulse Display.
            m_DisplayImpulse0 = root.Q<FloatField>("impulse-0");
            m_DisplayImpulse1 = root.Q<FloatField>("impulse-1");

            // Reset Scene.
            var resetScene = root.Q<Button>("reset-scene");
            resetScene.clicked += SetupScene;

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        var world = PhysicsWorld.defaultWorld;

        var bodyDef = new PhysicsBodyDefinition
        {
            bodyType = RigidbodyType2D.Dynamic,
            gravityScale = 1f,
            angularDamping = 0.5f,
            linearDamping = 0.2f
        };

        m_Body = world.CreateBody(bodyDef);

        var geometry = PolygonGeometry.CreateBox(new Vector2(2f, 1f), 0.2f);
        var shapeDef = new PhysicsShapeDefinition { density = 20f };
        m_Body.CreateShape(geometry, shapeDef);

        // Reset impulse.
        m_Impulses[0] = 0.0f;
        m_Impulses[1] = 0.0f;
    }

    private void OnUpdateJoint(PhysicsWorld world, float deltaTime)
    {
        if (deltaTime == 0.0f)
            return;

        // Set-up.
        var omega = 2.0f * PhysicsMath.PI * m_JointFrequency;
        var sigma = 2.0f * m_JointDamping + deltaTime * omega;
        var s = deltaTime * omega * sigma;
        var impulseCoefficient = 1.0f / (1.0f + s);
        var massCoefficient = s * impulseCoefficient;
        var biasCoefficient = omega / sigma;

        var mass = m_Body.mass;
        var invMass = mass < 0.0001f ? 0.0f : 1.0f / mass;
        var inertiaTensor = m_Body.rotationalInertia;
        var invI = inertiaTensor < 0.0001f ? 0.0f : 1.0f / inertiaTensor;

        // Fetch the body state.
        var bodyLinearVelocity = m_Body.linearVelocity;
        var bodyAngularVelocity = PhysicsMath.ToRadians(m_Body.angularVelocity);
        var bodyWorldCenterOfMass = m_Body.worldCenterOfMass;

        var localAnchors = new[] { new(m_AnchorOffsetY, -m_AnchorOffsetX), new Vector2(m_AnchorOffsetY, m_AnchorOffsetX) };

        // Draw the ground anchor.
        var anchorA = Vector2.zero;
        world.DrawPoint(anchorA, 8f, Color.azure, deltaTime);

        // Iterate the two impulses.
        for (var i = 0; i < 2; ++i)
        {
            // Anchors.
            var anchorB = m_Body.GetWorldPoint(localAnchors[i]);
            world.DrawPoint(anchorB, 8f, Color.greenYellow, deltaTime);
            var deltaAnchor = anchorB - anchorA;

            // Spring.
            const float springLength = 1.0f;
            var length = deltaAnchor.magnitude;
            var compression = length - springLength;
            if (compression < 0.0f || length < 0.001f)
            {
                world.DrawLine(anchorA, anchorB, Color.lightCyan, deltaTime);
                m_Impulses[i] = 0.0f;
                continue;
            }

            // Draw constraint.
            world.DrawLine(anchorA, anchorB, Color.yellow, deltaTime);

            // Mass.
            var axis = deltaAnchor.normalized;
            var rB = anchorB - bodyWorldCenterOfMass;
            var Jb = rB.x * axis.y - rB.y * axis.x; // Cross product.
            var K = invMass + Jb * invI * Jb;
            var invK = K < 0.0001f ? 0.0f : 1.0f / K;

            // Impulse.
            var dotVelocity = Vector2.Dot(bodyLinearVelocity, axis) + Jb * bodyAngularVelocity;
            var impulse = -massCoefficient * invK * (dotVelocity + biasCoefficient * compression);
            var appliedImpulse = Mathf.Clamp(impulse, -m_JointMaxForce * deltaTime, 0.0f);

            // Velocity Sum.
            bodyLinearVelocity += invMass * appliedImpulse * axis;
            bodyAngularVelocity += appliedImpulse * invI * Jb;

            // Impulse.
            m_Impulses[i] = appliedImpulse;
        }

        // Update impulse display.
        m_DisplayImpulse0.value = m_Impulses[0];
        m_DisplayImpulse1.value = m_Impulses[1];

        // Update the body.
        m_Body.linearVelocity = bodyLinearVelocity;
        m_Body.angularVelocity = PhysicsMath.ToDegrees(bodyAngularVelocity);
    }
}