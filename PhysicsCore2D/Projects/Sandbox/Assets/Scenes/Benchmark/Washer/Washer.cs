using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.UIElements;

public class Washer : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private Vector2 m_OldGravity;
    private float m_GravityScale;
    private float m_AngularVelocity;
    private bool m_KinematicWasher;
    private int m_DebrisCount;
    private float m_DebrisFriction;
    private float m_DebrisBounciness;

    private PhysicsBody m_WasherBody;
    private PhysicsHingeJoint m_WasherHinge;

    private void OnEnable()
    {
        m_SandboxManager = FindAnyObjectByType<SandboxManager>();
        m_SceneManifest = FindAnyObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindAnyObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 28f;
        m_CameraManipulator.CameraPosition = Vector2.up * 10f;

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GravityScale = 1f;
        
        m_AngularVelocity = 25f;
        m_KinematicWasher = true;
        m_DebrisCount = 2500;
        m_DebrisFriction = 0.6f;
        m_DebrisBounciness = 0.0f;

        SetupOptions();

        SetupScene();
    }

    private void OnDisable()
    {
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;
        
        world.gravity = m_OldGravity;
    }

    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;
        
        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI);
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI);

            // Angular Velocity.
            var angularVelocity = root.Q<Slider>("angular-velocity");
            angularVelocity.value = m_AngularVelocity;
            angularVelocity.RegisterValueChangedCallback(evt =>
            {
                m_AngularVelocity = evt.newValue;
                if (m_KinematicWasher)
	                m_WasherBody.angularVelocity = m_AngularVelocity;
                else
	                m_WasherHinge.motorSpeed = PhysicsMath.ToRadians(m_AngularVelocity); // This is fixed in 6000.3b2.
            });
            
            // Kinematic Washer.
            var kinematicWasher = root.Q<Toggle>("kinematic-washer");
            kinematicWasher.value = m_KinematicWasher;
            kinematicWasher.RegisterValueChangedCallback(evt =>
            {
                m_KinematicWasher = evt.newValue;
                SetupScene();
            });
            
            // Debris Count.
            var debrisCount = root.Q<SliderInt>("debris-count");
            debrisCount.value = m_DebrisCount;
            debrisCount.RegisterValueChangedCallback(evt =>
            {
                m_DebrisCount = evt.newValue;
                SetupScene();
            });

            // Debris Friction.
            var debrisFriction = root.Q<Slider>("debris-friction");
            debrisFriction.value = m_DebrisFriction;
            debrisFriction.RegisterValueChangedCallback(evt =>
            {
                m_DebrisFriction = evt.newValue;
                SetupScene();
            });

            // Debris Bounciness.
            var debrisBounciness = root.Q<Slider>("debris-bounciness");
            debrisBounciness.value = m_DebrisBounciness;
            debrisBounciness.RegisterValueChangedCallback(evt =>
            {
                m_DebrisBounciness = evt.newValue;
                SetupScene();
            });

            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.value = m_GravityScale;
            gravityScale.RegisterValueChangedCallback(evt =>
            {
                m_GravityScale = evt.newValue;
                world.gravity = m_OldGravity * m_GravityScale;
            });

            // Fetch the scene description.
            var sceneDescription = root.Q<Label>("scene-description");
            sceneDescription.text = $"\"{m_SceneManifest.LoadedSceneName}\"\n{m_SceneManifest.LoadedSceneDescription}";
        }
    }

    private void SetupScene()
    {
        // Reset the scene state.
        m_SandboxManager.ResetSceneState();

        // Get the default world.
        var world = PhysicsWorld.defaultWorld;

        // Create the ground body.
        var groundBody = world.CreateBody();

        // Washer.
        {
	        var bodyDef = PhysicsBodyDefinition.defaultDefinition;

	        bodyDef.position = Vector2.up * 10f;

			if (m_KinematicWasher)
			{
				bodyDef.type = PhysicsBody.BodyType.Kinematic;
				bodyDef.angularVelocity = m_AngularVelocity;
				bodyDef.linearVelocity = new Vector2(0.001f, -0.002f);
			}
			else
			{
				bodyDef.type = PhysicsBody.BodyType.Dynamic;
			}

			m_WasherBody = world.CreateBody(bodyDef);

			var shapeDef = PhysicsShapeDefinition.defaultDefinition;

			const float r0 = 14.0f;
			const float r1 = 16.0f;
			const float r2 = 18.0f;

			var angle = PhysicsMath.PI / 18.0f;
			var q = PhysicsRotate.FromRadians(angle);
			var qo = PhysicsRotate.FromRadians(angle * 0.1f);
			var u1 = Vector2.right;
			for (var n = 0; n < 36; ++n )
			{
				var u2 = n == 35 ? Vector2.right : q.RotateVector(u1);

				{
					var a1 = qo.InverseRotateVector(u1);
					var a2 = qo.RotateVector(u2);

					var p1 = a1 * r1;
					var p2 = a1 * r2;
					var p3 = a2 * r1;
					var p4 = a2 * r2;
					m_WasherBody.CreateShape(PolygonGeometry.Create(vertices: new[] { p1, p2, p3, p4 }), shapeDef);
				}

				if ( n % 9 == 0 )
				{
					var p1 = u1 * r0;
					var p2 = u1 * r1;
					var p3 = u2 * r0;
					var p4 = u2 * r1;

					m_WasherBody.CreateShape(PolygonGeometry.Create(vertices: new[] { p1, p2, p3, p4 }), shapeDef);
				}

				u1 = u2;
			}

			if (!m_KinematicWasher)
			{
				// For a dynamic body, create a motorised hinge joint.
				m_WasherHinge = world.CreateJoint(new PhysicsHingeJointDefinition
				{
					bodyA = groundBody,
					bodyB = m_WasherBody,
					localAnchorA = Vector2.up * 10f,
					localAnchorB = Vector2.zero,
					enableMotor = true,
					motorSpeed = PhysicsMath.ToRadians(m_AngularVelocity), // This is fixed in 6000.3b2.

					maxMotorTorque = 1e8f
				});
			}
        }

        // Debris.
        {
	        var gridCount = Mathf.Sqrt(m_DebrisCount);
			const float scale = 0.1f;

			var geometry = PolygonGeometry.CreateBox(new Vector2(scale, scale) * 2f);
			var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
			var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_DebrisFriction, bounciness = m_DebrisBounciness } };

			var y = -1.1f * scale * gridCount + 10.0f;
			for (var i = 0; i < gridCount; ++i)
			{
				var x = -1.1f * scale * gridCount;

				for ( var j = 0; j < gridCount; ++j )
				{
					bodyDef.position = new Vector2(x, y);

					// Update color state.
					shapeDef.surfaceMaterial.customColor = m_SandboxManager.ShapeColorState;
					
					// Create shape.
					world.CreateBody(bodyDef).CreateShape(geometry, shapeDef);

					x += 2.1f * scale;
				}

				y += 2.1f * scale;
			}
		}
    }
}