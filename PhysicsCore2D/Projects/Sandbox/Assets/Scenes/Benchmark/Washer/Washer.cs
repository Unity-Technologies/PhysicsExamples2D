using Unity.Mathematics;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.U2D.Physics;
#else
using UnityEngine.LowLevelPhysics2D;
#endif
using UnityEngine.UIElements;

public class Washer : MonoBehaviour
{
    private SandboxManager m_SandboxManager;
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private Vector2 m_OldGravity;
    private float m_GravityScale;
    private float m_MotorSpeed;
    private int m_DebrisCount;
    private float m_DebrisFriction;
    private int m_PaddleSpacing;
    private float m_PaddleScale;

    private PhysicsBody m_WasherBody;
    private PhysicsHingeJoint m_WasherHinge;

    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();
        m_SandboxManager.SceneOptionsUI = m_UIDocument;

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 28f;
        m_CameraManipulator.CameraPosition = Vector2.up * 10f;

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;

        m_OldGravity = PhysicsWorld.defaultWorld.gravity;
        m_GravityScale = 1f;
        
        m_MotorSpeed = -30f;
        m_DebrisCount = 2500;
        m_DebrisFriction = 0.6f;
        m_PaddleSpacing = 4;
        m_PaddleScale = 0.4f;

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

            // Motor Speed.
            var motorSpeed = root.Q<Slider>("motor-speed");
            motorSpeed.value = m_MotorSpeed;
            motorSpeed.RegisterValueChangedCallback(evt =>
            {
                m_MotorSpeed = evt.newValue;
	            m_WasherBody.angularVelocity = m_MotorSpeed;
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

            // Gravity Scale.
            var gravityScale = root.Q<Slider>("gravity-scale");
            gravityScale.value = m_GravityScale;
            gravityScale.RegisterValueChangedCallback(evt =>
            {
                m_GravityScale = evt.newValue;
                world.gravity = m_OldGravity * m_GravityScale;
            });

            // Paddle Spacing
            var paddleSpace = root.Q<SliderInt>("paddle-spacing");
            paddleSpace.value = m_PaddleSpacing;
            paddleSpace.RegisterValueChangedCallback(evt =>
            {
	            m_PaddleSpacing = evt.newValue;
	            SetupScene();
            });

            // Paddle Scale.
            var paddleScale = root.Q<Slider>("paddle-scale");
            paddleScale.value = m_PaddleScale;
            paddleScale.RegisterValueChangedCallback(evt =>
            {
	            m_PaddleScale = evt.newValue;
	            SetupScene();
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

        // Washer.
        {
	        var bodyDef = new PhysicsBodyDefinition
	        {
		        type = PhysicsBody.BodyType.Kinematic,
		        position = Vector2.up * 10f,
		        angularVelocity = m_MotorSpeed,
		        linearVelocity = new Vector2(0.001f, -0.002f)
	        };

			m_WasherBody = world.CreateBody(bodyDef);

			var shapeDef = PhysicsShapeDefinition.defaultDefinition;

			var r0 = Mathf.Lerp(14.0f, 10.0f, m_PaddleScale);
			const float r1 = 16.0f;
			const float r2 = 22.0f;

			var angle = PhysicsMath.PI / 18.0f;
			var q = new PhysicsRotate(angle);
			var qo = new PhysicsRotate(angle * 0.1f);
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

				if ( n % m_PaddleSpacing == 0 )
				{
					var p1 = u1 * r0;
					var p2 = u1 * r1;
					var p3 = u2 * r0;
					var p4 = u2 * r1;

					m_WasherBody.CreateShape(PolygonGeometry.Create(vertices: new[] { p1, p2, p3, p4 }), shapeDef);
				}

				u1 = u2;
			}
        }

        // Debris.
        {
	        var gridCount = Mathf.Sqrt(m_DebrisCount);
			const float scale = 0.15f;

			var geometry = new CircleGeometry { radius = scale };
			var bodyDef = new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic };
			var shapeDef = new PhysicsShapeDefinition { surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = m_DebrisFriction, bounciness = 0f } };

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