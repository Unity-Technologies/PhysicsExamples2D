using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UIElements;

public class CharacterMover : MonoBehaviour
{
    private SandboxManager m_SandboxManager;	
    private SceneManifest m_SceneManifest;
    private UIDocument m_UIDocument;
    private CameraManipulator m_CameraManipulator;

    private enum PogoType
    {
        Point = 0,
        Circle = 1,
        Segment = 2
    }
    
    private struct CollisionBits
    {
	    public static readonly PhysicsMask StaticBit = new(0);
	    public static readonly PhysicsMask MoverBit = new (1);
	    public static readonly PhysicsMask DynamicBit = new(2);
	    public static readonly PhysicsMask AllBits = PhysicsMask.All;
    }

    private const int MaxSolverIterations = 5;
    private const float DrawLifetime = 1f;
    private const float VelocityDrawScale = 2f;
    private readonly Vector2 m_ElevatorOffset = new (112f, 10f);
    private const float ElevatorAmplitude = 4f;

    private float m_JumpSpeed;
    private float m_MinSpeed ;
    private float m_MaxSpeed;
    private float m_StopSpeed;
    private float m_Accelerate;
    private float m_AirSteer;
    private float m_Friction; 
    private float m_Gravity;
    private float m_PogoScale;
    private float m_PogoFrequency;    
    private float m_PogoDamping;    
    private PogoType m_PogoType;
    
    private PhysicsTransform m_Transform;
    private Vector2 m_Velocity;
    private CapsuleGeometry m_Geometry;

    private PhysicsBody m_ElevatorBody;
    
    private float m_Time;
    private float m_PogoVelocity;
    private bool m_OnGround;
    private bool m_JumpReleased;
    private bool m_LeftPressed;
    private bool m_RightPressed;
    private bool m_JumpPressed;
    
    private void OnEnable()
    {
        m_SandboxManager = FindFirstObjectByType<SandboxManager>();
        m_SceneManifest = FindFirstObjectByType<SceneManifest>();
        m_UIDocument = GetComponent<UIDocument>();

        m_CameraManipulator = FindFirstObjectByType<CameraManipulator>();
        m_CameraManipulator.CameraSize = 10f;
        m_CameraManipulator.CameraPosition = new Vector2(20f, 9f);

        // Set up the scene reset action.
        m_SandboxManager.SceneResetAction = SetupScene;
        
        // Set Overrides.
        m_SandboxManager.SetOverrideDrawOptions(overridenOptions: PhysicsWorld.DrawOptions.AllJoints, fixedOptions: PhysicsWorld.DrawOptions.Off);
        
        // Reset Option State.
        m_JumpSpeed = 15f;
        m_MinSpeed = 0.1f;
        m_MaxSpeed = 8f;
        m_StopSpeed = 3f;
        m_Accelerate = 20f;
        m_AirSteer = 0.2f;
        m_Friction = 8f;
        m_Gravity = 50f;
        m_PogoScale = 3f;
        m_PogoFrequency = 5f;
        m_PogoDamping = 0.8f;    
        m_PogoType = PogoType.Circle;
        
        SetupOptions();

        SetupScene();
        
        PhysicsEvents.PreSimulate += CharacterMove;
    }

    private void OnDisable()
    {
	    PhysicsEvents.PreSimulate -= CharacterMove;
	    
	    // Reset overrides.
	    m_SandboxManager.ResetOverrideDrawOptions();
    }

    private void Update()
    {
	    // Finish if the world is paused.
	    if (m_SandboxManager.WorldPaused)
		    return;

	    // Fetch keyboard input.
	    var currentKeyboard = Keyboard.current;
	    m_LeftPressed = currentKeyboard.leftArrowKey.isPressed;
	    m_RightPressed = currentKeyboard.rightArrowKey.isPressed;
	    m_JumpPressed = currentKeyboard.spaceKey.isPressed;
    }
    
    private void SetupOptions()
    {
        var root = m_UIDocument.rootVisualElement;
        
        {
            // Menu Region (for camera manipulator).
            var menuRegion = root.Q<VisualElement>("menu-region");
            menuRegion.RegisterCallback<PointerEnterEvent>(_ => ++m_CameraManipulator.OverlapUI );
            menuRegion.RegisterCallback<PointerLeaveEvent>(_ => --m_CameraManipulator.OverlapUI );
            
            // Jump Speed.
            var jumpSpeed = root.Q<Slider>("jump-speed");
            jumpSpeed.RegisterValueChangedCallback(evt => { m_JumpSpeed = evt.newValue; });
            jumpSpeed.value = m_JumpSpeed;

            // Min Speed.
            var minSpeed = root.Q<Slider>("min-speed");
            minSpeed.RegisterValueChangedCallback(evt => { m_MinSpeed = evt.newValue; });
            minSpeed.value = m_MinSpeed;
            
            // Max Speed.
            var maxSpeed = root.Q<Slider>("max-speed");
            maxSpeed.RegisterValueChangedCallback(evt => { m_MaxSpeed = evt.newValue; });
            maxSpeed.value = m_MaxSpeed;            
            
            // Stop Speed.
            var stopSpeed = root.Q<Slider>("stop-speed");
            stopSpeed.RegisterValueChangedCallback(evt => { m_StopSpeed = evt.newValue; });
            stopSpeed.value = m_StopSpeed;
            
            // Accelerate.
            var accelerate = root.Q<Slider>("accelerate");
            accelerate.RegisterValueChangedCallback(evt => { m_Accelerate = evt.newValue; });
            accelerate.value = m_Accelerate;            

            // Air Steer.
            var airSteer = root.Q<Slider>("air-steer");
            airSteer.RegisterValueChangedCallback(evt => { m_AirSteer = evt.newValue; });
            airSteer.value = m_AirSteer;            
            
            // Friction.
            var friction = root.Q<Slider>("friction");
            friction.RegisterValueChangedCallback(evt => { m_Friction = evt.newValue; });
            friction.value = m_Friction;                

            // Gravity.
            var gravity = root.Q<Slider>("gravity");
            gravity.RegisterValueChangedCallback(evt => { m_Gravity = evt.newValue; });
            gravity.value = m_Gravity;

            // Pogo Scale.
            var pogoScale = root.Q<Slider>("pogo-scale");
            pogoScale.RegisterValueChangedCallback(evt => { m_PogoScale = evt.newValue; });
            pogoScale.value = m_PogoScale;                
            
            // Pogo Frequency.
            var pogoFrequency = root.Q<Slider>("pogo-frequency");
            pogoFrequency.RegisterValueChangedCallback(evt => { m_PogoFrequency = evt.newValue; });
            pogoFrequency.value = m_PogoFrequency;                
            
            // Pogo Damping.
            var pogoDamping = root.Q<Slider>("pogo-damping");
            pogoDamping.RegisterValueChangedCallback(evt => { m_PogoDamping = evt.newValue; });
            pogoDamping.value = m_PogoDamping;                
            
            // Pogo Type.
            var pogoType = root.Q<EnumField>("pogo-type");
            pogoType.value = m_PogoType;
            pogoType.RegisterValueChangedCallback(evt => { m_PogoType = (PogoType)evt.newValue; });
            
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

        m_Transform = new PhysicsTransform(new Vector2(2f, 8f));
        //m_Transform = new PhysicsTransform(new Vector2(75f, 15f));
        m_Velocity = Vector2.zero;
        m_Geometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.5f), center2 = new Vector2(0f, 0.5f), radius = 0.3f };

        m_Time = 0f;
        m_PogoVelocity = 0f;
        m_OnGround = false;
        m_JumpReleased = true;
        m_LeftPressed = false;
        m_RightPressed = false;
        m_JumpPressed = false;
        
        // Fetch the scene manager detail.
        var world = PhysicsWorld.defaultWorld;
        var bodies = m_SandboxManager.Bodies;

        // Ground #1.
        PhysicsBody groundBody1;
        {
            groundBody1 = world.CreateBody();
            bodies.Add(groundBody1);

            using var points = new NativeList<Vector2>(Allocator.Temp)
            {
                new(-9.47083378f, -0.216665655f),
                new(48.7374992f, -0.216665655f),
                new(48.7374992f, 9.30833435f),
                new(48.2083321f, 9.30833435f),
                new(46.0916634f, 7.72083426f),
                new(43.4458313f, 6.13333464f),
                new(40.7965355f, 5.07500029f),
                new(34.4499969f, 3.48749995f),
                new(30.2166634f, 2.95833445f),
                new(25.4541626f, 0.841668725f),
                new(18.5750008f, 0.841668725f),
                new(18.5750008f, 1.90000308f),
                new(17.5166683f, 1.90000308f),
                new(17.5166683f, 0.841668725f),
                new(13.8125000f, 0.841668725f),
                new(13.8125000f, 1.37083435f),
                new(13.2833347f, 1.37083435f),
                new(13.2833347f, 1.90000308f),
                new(12.7541656f, 1.90000308f),
                new(12.7541656f, 2.42916870f),
                new(12.2250004f, 2.42916870f),
                new(12.2250004f, 2.95833445f),
                new(6.93333435f, 2.95833445f),
                new(6.93333435f, 3.48749995f),
                new(5.87500143f, 3.48749995f),
                new(5.87500143f, 4.01666880f),
                new(4.81666899f, 4.01666880f),
                new(4.81666899f, 4.54583454f),
                new(3.75833440f, 4.54583454f),
                new(3.75833440f, 5.07500029f),
                new(2.70000148f, 5.07500029f),
                new(2.70000148f, 5.60416889f),
                new(0.583333611f, 5.60416889f),
                new(-2.06250000f, 4.54583454f),
                new(-3.12083364f, 4.54583454f),
                new(-5.23750019f, 2.95833445f),
                new(-6.82500029f, 2.95833445f),
                new(-8.94166660f, 2.42916870f),
                new(-8.94166660f, 9.30833435f),
                new(-9.47083378f, 9.30833435f),
                new(-9.47083378f, -0.216665655f)        
            };

            var chainGeometry = new ChainGeometry(points.AsArray());
            groundBody1.CreateChain(chainGeometry, PhysicsChainDefinition.defaultDefinition);
        }
        
        // Ground #2.
        PhysicsBody groundBody2;
        {
            groundBody2 = world.CreateBody(new PhysicsBodyDefinition { position = new Vector2(98f, 0f) });
            bodies.Add(groundBody2);

            using var points = new NativeList<Vector2>(Allocator.Temp)
            {
                new(0.529166639f, -0.216665655f),
                new(58.7374992f, -0.216665655f),
                new(58.7374992f, 4.54583454f),
                new(53.9750023f, 4.54583454f),
                new(58.2083321f, 0.312500000f),
                new(53.4458313f, 0.312500000f),
                new(45.5083275f, 2.95833445f),
                new(40.2166634f, 1.37083435f),
                new(35.4541626f, 0.841668725f),
                new(32.8083305f, 0.841668725f),
                new(32.7968292f, 0.312500000f),
                new(31.7384987f, 0.312500000f),
                new(31.7384987f, 0.841665685f),
                new(30.1647892f, 0.841668725f),
                new(30.1625099f, 1.37083435f),
                new(29.6333447f, 1.37083435f),
                new(29.1041775f, 0.841665685f),
                new(27.5166779f, 0.841665685f),
                new(26.9875088f, 0.312500000f),
                new(26.4697266f, 0.841668725f),
                new(23.8125095f, 0.841668725f),
                new(23.8125095f, 1.37083435f),
                new(23.2833443f, 1.37083435f),
                new(23.2833443f, 1.90000308f),
                new(22.2250099f, 1.89999998f),
                new(22.2250099f, 2.42916560f),
                new(21.6958447f, 2.42916560f),
                new(21.6958447f, 2.95833135f),
                new(20.6375103f, 2.95833445f),
                new(20.6375103f, 3.48749995f),
                new(20.1083431f, 3.48749995f),
                new(20.1083431f, 4.01666880f),
                new(19.0500107f, 4.01666880f),
                new(19.0500107f, 4.54583454f),
                new(18.5208340f, 4.54583454f),
                new(18.5208340f, 5.07500029f),
                new(17.4625015f, 5.07500029f),
                new(17.4625015f, 5.60416889f),
                new(10.5833349f, 5.60416555f),
                new(8.99583435f, 6.13333464f),
                new(7.40833426f, 6.66249990f),
                new(6.35000086f, 7.72083426f),
                new(4.23333359f, 7.72083426f),
                new(2.64583349f, 8.25000000f),
                new(1.05833340f, 9.30833435f),
                new(0.529166698f, 9.30833435f),
                new(0.529166698f, -0.216665655f)            
            };

            var chainGeometry = new ChainGeometry(points.AsArray());
            groundBody2.CreateChain(chainGeometry, PhysicsChainDefinition.defaultDefinition);
        }

        // Create the Bridge.
        {
	        var box = PolygonGeometry.CreateBox(new Vector2(1f, 0.25f));
	        var shapeDef = PhysicsShapeDefinition.defaultDefinition;

	        var jointDef = new PhysicsHingeJointDefinition
	        {
		        maxMotorTorque = 10f,
		        enableMotor = true,
		        springFrequency = 3f,
		        springDamping = 0.8f,
		        enableSpring = true
	        };

	        var offset = new Vector2(48.7f, 9.2f);
			var count = 50;

			var prevBody = groundBody1;
			for (var n = 0; n < count; ++n)
			{
				var bodyDef = new PhysicsBodyDefinition
				{
					bodyType = RigidbodyType2D.Dynamic,
					position = new Vector2(offset.x + 0.5f + 1f * n, offset.y),
					angularDamping = 0.2f
				};

				var body = world.CreateBody(bodyDef);
				bodies.Add(body);

				body.CreateShape(box, shapeDef);

				var pivot = new Vector2(offset.x + 1.0f * n, offset.y);
				jointDef.bodyA = prevBody;
				jointDef.bodyB = body;
				jointDef.localAnchorA = new PhysicsTransform(jointDef.bodyA.GetLocalPoint(pivot));
				jointDef.localAnchorB = new PhysicsTransform(jointDef.bodyB.GetLocalPoint(pivot));
				world.CreateJoint(jointDef);

				prevBody = body;
			}

			{
				var pivot = new Vector2(offset.x + 1.0f * count, offset.y);
				jointDef.bodyA = prevBody;
				jointDef.bodyB = groundBody2;
				jointDef.localAnchorA = new PhysicsTransform(jointDef.bodyA.GetLocalPoint(pivot));
				jointDef.localAnchorB = new PhysicsTransform(jointDef.bodyB.GetLocalPoint(pivot));
				world.CreateJoint(jointDef);
			}
        }
	    
	    // Create some random dynamic debris.
		{
			ref var random = ref m_SandboxManager.Random;
			
			for (var n = -70f; n < 75f; n += 2f)
			{
				var bodyDef = new PhysicsBodyDefinition
				{
					bodyType = RigidbodyType2D.Dynamic,
					position = new Vector2(75f + n, 12f + random.NextFloat(0f, 6f)),
					rotation = new PhysicsRotate(random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI)),
					angularVelocity = random.NextFloat(-PhysicsMath.PI, PhysicsMath.PI),
					fastCollisionsAllowed = true
				};
				var body = world.CreateBody(bodyDef);
				bodies.Add(body);
				
				var shapeDef = new PhysicsShapeDefinition
				{
					contactFilter = new PhysicsShape.ContactFilter
					{
						categories = CollisionBits.DynamicBit,
						contacts = CollisionBits.AllBits
					},
					surfaceMaterial = new PhysicsShape.SurfaceMaterial
					{
						bounciness = 0.0f,
						friction = 0.6f,
						rollingResistance = 0.2f
					}
				};

				var box = PolygonGeometry.CreateBox(new Vector2(random.NextFloat(0.1f, 0.5f), random.NextFloat(0.5f, 1.5f)));
				body.CreateShape(box, shapeDef);
			}
		}

		// Create the elevator.
		{
			var bodyDef = new PhysicsBodyDefinition
			{
				bodyType = RigidbodyType2D.Kinematic,
				position = new Vector2(m_ElevatorOffset.x, m_ElevatorOffset.y - ElevatorAmplitude)
			};
			m_ElevatorBody = world.CreateBody(bodyDef);
			bodies.Add(m_ElevatorBody);

			var shapeDef = new PhysicsShapeDefinition
			{
				contactFilter = new PhysicsShape.ContactFilter
				{
					categories = CollisionBits.DynamicBit,
					contacts = CollisionBits.AllBits
				},
				moverData = new PhysicsShape.MoverData
				{
					pushLimit = 0.01f,
					clipVelocity = true
				}
			};
			
			var box = PolygonGeometry.CreateBox(new Vector2(4f, 0.2f));
			m_ElevatorBody.CreateShape(box, shapeDef);
		}        
    }

    private void CharacterMove(PhysicsWorld world, float deltaTime)
    {
	    // Clear any drawing (specifically here the custom-drawing with lifetime).
	    PhysicsWorld.defaultWorld.ClearDraw();
	    
	    // Finish if the world is paused.
	    if (m_SandboxManager.WorldPaused || !world.isDefaultWorld)
		    return;
	    
	    // Animate the elevator.
	    var target = new PhysicsTransform(new Vector2(m_ElevatorOffset.x, ElevatorAmplitude * Mathf.Cos(1f * m_Time + PhysicsMath.PI) + m_ElevatorOffset.y));
	    m_ElevatorBody.SetTransformTarget(target, deltaTime);

	    // Bump the time.
	    m_Time += deltaTime;

	    // Character Input.
	    {
		    var throttle = 0f;

		    // Left?
		    if (m_LeftPressed)
			    throttle -= 1f;

		    // Right?
		    if (m_RightPressed)
			    throttle += 1f;

		    // Jump?
		    if (m_JumpPressed)
		    {
			    if (m_OnGround && m_JumpReleased)
			    {
				    m_Velocity.y = m_JumpSpeed;
				    m_OnGround = false;
				    m_JumpReleased = false;
			    }
		    }
		    else
		    {
			    m_JumpReleased = true;
		    }

		    // Solve the movement.
		    SolveMove(world, deltaTime, throttle);
	    }

	    // Update the camera position X to the character.
	    var cameraPosition = m_CameraManipulator.Camera.transform.position;
	    m_CameraManipulator.Camera.transform.position = new Vector3(m_Transform.position.x, cameraPosition.y, cameraPosition.z);
	    
	    // Draw.
	    world.DrawGeometry(m_Geometry, m_Transform, m_OnGround ? Color.orange : Color.aquamarine, DrawLifetime);
	    world.DrawLine(m_Transform.position, m_Transform.position + m_Velocity.normalized * VelocityDrawScale, m_SandboxManager.ShapeColorState, DrawLifetime);
    }

    /// Reference: https://github.com/id-Software/Quake/blob/master/QW/client/pmove.c#L390
    private void SolveMove(PhysicsWorld world, float deltaTime, float throttle)
    {
		// Friction
		var speed = m_Velocity.magnitude;
		if (speed < m_MinSpeed)
		{
			m_Velocity = Vector2.zero;
		}
		else if (m_OnGround)
		{
			// Linear damping above stopSpeed and fixed reduction below stopSpeed.
			var control = speed < m_StopSpeed ? m_StopSpeed : speed;

			// Friction has units of 1/time.
			var drop = control * m_Friction * deltaTime;
			var newSpeed = Mathf.Max(0f, speed - drop);
			m_Velocity *= newSpeed / speed;
		}

		// If we're on the ground, stop any vertical velocity.
		if (m_OnGround)
			m_Velocity.y = 0f;
		
		// Calculate the desired movement.
		var desiredVelocity = new Vector2(m_MaxSpeed * throttle, 0f);
		var desiredSpeed = Mathf.Min(desiredVelocity.magnitude, m_MaxSpeed);
		var desiredDirection = desiredVelocity.normalized;
		
		// Accelerate
		var currentSpeed = Vector2.Dot(m_Velocity, desiredDirection);
		var addSpeed = desiredSpeed - currentSpeed;
		if (addSpeed > 0f)
		{
			var steer = m_OnGround ? 1f : m_AirSteer;
			var accelSpeed = Mathf.Min(steer * m_Accelerate * m_MaxSpeed * deltaTime, addSpeed);
			
			m_Velocity += accelSpeed * desiredDirection;
		}

		// Add gravity.
		m_Velocity.y -= m_Gravity * deltaTime;

		// Calculate Pogo details.
		var pogoRestLength = m_PogoScale * m_Geometry.radius;
		var rayLength = pogoRestLength + m_Geometry.radius;
		var origin = m_Transform.TransformPoint(m_Geometry.center1);
		var circle = new CircleGeometry { center = origin, radius = 0.5f * m_Geometry.radius };
		var segmentOffset = new Vector2(0.75f * m_Geometry.radius, 0f);
		var segment = new SegmentGeometry { point1 = origin - segmentOffset, point2 = origin + segmentOffset };
		
		var pogoFilter = new PhysicsQuery.QueryFilter
		{
			categories = CollisionBits.MoverBit,
			hitCategories = CollisionBits.StaticBit | CollisionBits.DynamicBit
		};

		// Fetch appropriate shape proxy and translation.
		PhysicsShape.ShapeProxy shapeProxy;
		Vector2 translation;
		switch (m_PogoType)
		{
			case PogoType.Point:
			{
				shapeProxy = new PhysicsShape.ShapeProxy
				{
					vertices = new PhysicsShape.ShapeArray { vertex0 = origin },
					count = 1,
					radius = 0
				};
				
				translation = new Vector2(0f, -rayLength);
				
				break;
			}

			case PogoType.Circle:
			{
				shapeProxy = new PhysicsShape.ShapeProxy(circle);
				translation = new Vector2(0f, -rayLength + circle.radius);
				
				break;
			}
			
			case PogoType.Segment:
			{
				shapeProxy = new PhysicsShape.ShapeProxy(segment);
				translation = new Vector2(0f, -rayLength);
				
				break;
			}

			default:
				throw new ArgumentOutOfRangeException();
		}

		// Find a hit for the Pogo shape.
		using var hit = world.CastShapeProxy(shapeProxy, translation, pogoFilter);
		var castResult = hit.Length > 0 ? hit[0] : default;

		// Update grounded flag.
		if (m_OnGround)
		{
			m_OnGround = castResult.isValid;
		}
		else
		{
			// Avoid snapping to ground if still going up.
			m_OnGround = castResult.isValid && m_Velocity.y <= 0.01f;
		}

		// Did the Pogo hit something?
		if (castResult.isValid)
		{
			// Yes, so handle the pogo spring-damper
			var pogoCurrentLength = castResult.fraction * rayLength;
			var offset = pogoCurrentLength - pogoRestLength;
			m_PogoVelocity = PhysicsMath.SpringDamper(
				frequency: m_PogoFrequency,
				damping: m_PogoDamping,
				translation: offset,
				speed: m_PogoVelocity,
				deltaTime: deltaTime);

			var delta = castResult.fraction * translation;
			world.DrawLine(origin, origin + delta, Color.gray, DrawLifetime);

			// Draw the Pogo.
			DrawPogo(world, origin, delta, circle, segment, Color.plum);

			// Apply a downward force from the Pogo.
			castResult.shape.body.ApplyForce(Vector2.down * 50f, castResult.point);
		}
		else
		{
			// No, so reset the pogo velocity.
			m_PogoVelocity = 0.0f;

			var delta = translation;
			world.DrawLine(origin, origin + delta, Color.gray, DrawLifetime);

			// Draw the Pogo.
			DrawPogo(world, origin, delta, circle, segment, Color.gray);
		}

		// Calculate the new target position.
		var targetPosition = m_Transform.position + (deltaTime * m_Velocity) + (deltaTime * m_PogoVelocity * Vector2.up);
		
		var worldMoverInput = new PhysicsQuery.WorldMoverInput
		{
			geometry = m_Geometry,
			maxIterations = MaxSolverIterations,
			overlapFilter = new PhysicsQuery.QueryFilter
			{
				categories = CollisionBits.MoverBit,
				hitCategories = CollisionBits.StaticBit | CollisionBits.DynamicBit | CollisionBits.MoverBit
			},
			
			// Movers don't sweep against other movers, allows for soft collision.
			castFilter = new PhysicsQuery.QueryFilter
			{
				categories = CollisionBits.MoverBit,
				hitCategories = CollisionBits.StaticBit | CollisionBits.DynamicBit
			},
			
			moveTolerance = 0.01f,
			targetPosition = targetPosition,
			transform = m_Transform,
			velocity = m_Velocity
		};

		var moverResult = world.CastMover(worldMoverInput);

		m_Transform = moverResult.transform;
		m_Velocity = moverResult.velocity;
    }

    // Draw the Pogo.
    private void DrawPogo(PhysicsWorld world, Vector2 origin, Vector2 delta, CircleGeometry circle, SegmentGeometry segment, Color pogoColor)
    {
	    // Draw the Pogo.
	    switch (m_PogoType)
	    {
		    case PogoType.Point:
		    {
			    world.DrawPoint(origin + delta, 10f, pogoColor, DrawLifetime);
			    return;
		    }

		    case PogoType.Circle:
		    {
			    world.DrawCircle(origin + delta, circle.radius, pogoColor, DrawLifetime);
			    return;
		    }
			
		    case PogoType.Segment:
		    {
			    world.DrawLine(segment.point1 + delta, segment.point2 + delta, pogoColor, DrawLifetime);
			    return;
		    }

		    default:
			    throw new ArgumentOutOfRangeException();
	    }
    }
}
