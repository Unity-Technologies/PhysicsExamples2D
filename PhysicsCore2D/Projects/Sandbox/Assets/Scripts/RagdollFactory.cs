using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

public static class RagdollFactory
{
    [Serializable]
    public struct Configuration
    {
        public Configuration(
            Vector2 scaleRange,
            Vector2 angularImpulseRange,
            float jointFrequency,
            float jointDamping,
            float jointFriction,
            UInt64 contactBodyLayer,
            UInt64 contactFeetLayer,
            int contactGroupIndex,
            IShapeColorProvider colorProvider,
            float gravityScale = 1.0f,
            bool triggerEvents = false,
            bool fastCollisionsAllowed = true,
            bool enableMotor = true,
            bool enableLimits = true
        )
        {
            ScaleRange = scaleRange;
            AngularImpulseRange = angularImpulseRange;
            JointFrequency = jointFrequency;
            JointDamping = jointDamping;
            JointFriction = jointFriction;
            ContactBodyLayer = contactBodyLayer;
            ContactFeetLayer = contactFeetLayer;
            ContactGroupIndex = contactGroupIndex;
            ColorProvider = colorProvider;
            GravityScale = gravityScale;
            TriggerEvents = triggerEvents;
            FastCollisionsAllowed = fastCollisionsAllowed;
            EnableMotor = enableMotor;
            EnableLimits = enableLimits;
        }

        public Vector2 ScaleRange;
        public Vector2 AngularImpulseRange;
        [Min(0f)] public float JointFrequency;
        [Min(0f)] public float JointDamping;
        [Min(0f)] public float JointFriction;
        [Min(0f)] public float GravityScale;
        public UInt64 ContactBodyLayer;
        public UInt64 ContactFeetLayer;
        public int ContactGroupIndex;
        public IShapeColorProvider ColorProvider;
        public bool TriggerEvents;

        [FormerlySerializedAs("FastCollisions")]
        public bool FastCollisionsAllowed;

        public bool EnableMotor;
        public bool EnableLimits;
    }

    public enum BoneType
    {
        Hip = 0,
        Torso = 1,
        Head = 2,
        UpperLeftLeg = 3,
        LowerLeftLeg = 4,
        UpperRightLeg = 5,
        LowerRightLeg = 6,
        UpperLeftArm = 7,
        LowerLeftArm = 8,
        UpperRightArm = 9,
        LowerRightArm = 10,

        Count = 11,
        Invalid
    }

    public struct Bone
    {
        public PhysicsBody body;
        public PhysicsHingeJoint joint;
        public float frictionScale;
        public BoneType parentBone;
    }

    public struct Ragdoll : IEnumerable<PhysicsBody>, IDisposable
    {
        private NativeArray<Bone> m_BoneArray;

        public Ragdoll(Allocator allocator, float scale, Configuration configuration)
        {
            m_BoneArray = new NativeArray<Bone>((int)BoneType.Count, allocator);
            for (var i = 0; i < (int)BoneType.Count; ++i)
            {
                m_BoneArray[i] = new Bone
                {
                    body = new PhysicsBody(),
                    joint = new PhysicsHingeJoint(),
                    frictionScale = 1.0f,
                    parentBone = BoneType.Invalid
                };
            }

            // Set the scale.
            this.scale = originalScale = scale;
            this.configuration = configuration;
        }

        public Configuration configuration { get; set; }
        public float originalScale { get; set; }
        public float scale { get; set; }
        public float boneCount => m_BoneArray.Length;

        public bool IsCreated => m_BoneArray.IsCreated;

        public Bone this[BoneType boneType]
        {
            get => m_BoneArray[(int)boneType];
            set => m_BoneArray[(int)boneType] = value;
        }

        public Bone this[int boneIndex]
        {
            get => m_BoneArray[boneIndex];
            set => m_BoneArray[boneIndex] = value;
        }

        public IEnumerator<PhysicsBody> GetEnumerator()
        {
            foreach (var bone in m_BoneArray)
                yield return bone.body;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            m_BoneArray.Dispose();
        }

        public void Rescale(float newScale)
        {
            if (newScale < 0.01f || scale <= 0.0f)
                throw new InvalidOperationException("Scale must be greater than zero.");

            var scaleRatio = newScale / scale;

            var originalRatio = newScale / originalScale;
            var jointFriction = originalRatio * originalRatio * originalRatio * configuration.JointFriction;

            var origin = this[0].body.position;

            for (var boneIndex = 0; boneIndex < boneCount; ++boneIndex)
            {
                var bone = this[boneIndex];
                var body = bone.body;

                if (boneIndex > 0)
                {
                    // Update the transform.
                    var transform = body.transform;
                    transform.position = origin + (transform.position - origin) * scaleRatio;
                    body.transform = transform;

                    var joint = bone.joint;
                    var localAnchorA = joint.localAnchorA;
                    var localAnchorB = joint.localAnchorB;
                    localAnchorA.position *= scaleRatio;
                    localAnchorB.position *= scaleRatio;
                    joint.localAnchorA = localAnchorA;
                    joint.localAnchorB = localAnchorB;

                    joint.maxMotorTorque = bone.frictionScale * jointFriction;
                }

                // Handle thw shapes.
                using var shapes = body.GetShapes();
                var shapeCount = shapes.Length;
                for (var shapeIndex = 0; shapeIndex < shapeCount; ++shapeIndex)
                {
                    var shape = shapes[shapeIndex];
                    var shapeType = shape.shapeType;
                    if (shapeType == PhysicsShape.ShapeType.Capsule)
                    {
                        var geometry = shape.capsuleGeometry;
                        shape.capsuleGeometry = new CapsuleGeometry
                        {
                            center1 = geometry.center1 * scaleRatio,
                            center2 = geometry.center2 * scaleRatio,
                            radius = geometry.radius * scaleRatio
                        };
                    }
                    else if (shapeType == PhysicsShape.ShapeType.Polygon)
                    {
                        var scaledPolygon = shape.polygonGeometry.Transform(transform: Matrix4x4.Scale(new Vector3(scaleRatio, scaleRatio, scaleRatio)), scaleRadius: true);
                        shape.polygonGeometry = scaledPolygon;
                    }
                }

                body.ApplyMassFromShapes();
            }

            scale = newScale;
        }
    }

    public static Ragdoll Spawn(PhysicsWorld world, Vector2 position, Configuration configuration, bool rightFacing, ref Random random, Allocator allocator = Allocator.Temp)
    {
        var bodyDef = new PhysicsBodyDefinition
        {
            type = PhysicsBody.BodyType.Dynamic,
            gravityScale = configuration.GravityScale,
            fastCollisionsAllowed = configuration.FastCollisionsAllowed,
            sleepThreshold = 0.1f
        };

        var shapeDef = new PhysicsShapeDefinition
        {
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.2f },
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = configuration.ContactBodyLayer,
                contacts = configuration.ContactBodyLayer | configuration.ContactFeetLayer,
                groupIndex = -configuration.ContactGroupIndex
            },
            triggerEvents = configuration.TriggerEvents
        };

        var footShapeDef = new PhysicsShapeDefinition
        {
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.05f },
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = configuration.ContactBodyLayer,
                contacts = configuration.ContactFeetLayer,
                groupIndex = -configuration.ContactGroupIndex
            },
            triggerEvents = configuration.TriggerEvents
        };

        if (configuration.ColorProvider is not { IsShapeColorActive: true })
            footShapeDef.surfaceMaterial.customColor = Color.saddleBrown;

        var scale = random.NextFloat(configuration.ScaleRange.x, configuration.ScaleRange.y);
        var angularImpulse = random.NextFloat(configuration.AngularImpulseRange.x, configuration.AngularImpulseRange.y);
        var maxTorque = configuration.JointFriction * scale;
        var jointFrequency = configuration.JointFrequency;
        var dampingRatio = configuration.JointDamping;
        var enableMotor = configuration.EnableMotor;
        var enableLimit = configuration.EnableLimits;

        var shirtColor = Color.mediumTurquoise;
        var trouserColor = Color.dodgerBlue;
        var skinColors = new[] { Color.navajoWhite, Color.lightYellow, Color.peru, Color.tan };
        var skinColor = skinColors[random.NextInt(0, skinColors.Length)];

        // Create the ragdoll.
        var ragdoll = new Ragdoll(allocator, scale, configuration);

        // Hip.
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.Hip];

            // Create body.
            bodyDef.position = position + new Vector2(0f, 0.95f * scale);
            bodyDef.linearDamping = 0f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = trouserColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.02f * scale), center2 = new Vector2(0f, 0.02f * scale), radius = 0.095f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Set the bone.
            ragdoll[BoneType.Hip] = bone;
        }

        // Torso.
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.Torso];
            bone.parentBone = BoneType.Hip;
            bone.frictionScale = 0.5f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 1.2f * scale);
            bodyDef.linearDamping = 0f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = shirtColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.135f * scale), center2 = new Vector2(0f, 0.135f * scale), radius = 0.09f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 1.0f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.25f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Apply the angular impulse.
            if (angularImpulse != 0.0f)
                bone.body.ApplyAngularImpulse(angularImpulse);

            // Set the bone.
            ragdoll[BoneType.Torso] = bone;
        }

        // Head
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.Head];
            bone.parentBone = BoneType.Torso;
            bone.frictionScale = 0.25f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 1.475f * scale);
            bodyDef.linearDamping = 0.1f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = skinColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.038f * scale), center2 = new Vector2(0f, 0.039f * scale), radius = 0.075f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 1.4f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.3f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0.1f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Set the bone.
            ragdoll[BoneType.Head] = bone;
        }

        // Upper Left Leg
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.UpperLeftLeg];
            bone.parentBone = BoneType.Hip;
            bone.frictionScale = 1f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 0.775f * scale);
            bodyDef.linearDamping = 0f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = trouserColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.06f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 0.9f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.05f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0.4f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Set the bone.
            ragdoll[BoneType.UpperLeftLeg] = bone;
        }

        var footHorizontalScale = rightFacing ? 1f : -1f;
        Vector2[] footVertices =
        {
            new Vector2(-0.03f * footHorizontalScale, -0.185f) * scale,
            new Vector2(0.11f * footHorizontalScale, -0.185f) * scale,
            new Vector2(0.11f * footHorizontalScale, -0.16f) * scale,
            new Vector2(-0.03f * footHorizontalScale, -0.14f) * scale
        };
        var footPolygonGeometry = PolygonGeometry.Create(footVertices.AsSpan(), 0.015f * scale);

        // Lower Left Leg
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.LowerLeftLeg];
            bone.parentBone = BoneType.UpperLeftLeg;
            bone.frictionScale = 0.5f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 0.475f * scale);
            bodyDef.linearDamping = 0f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = trouserColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.155f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.045f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create foot shape.
            bone.body.CreateShape(footPolygonGeometry, footShapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 0.625f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.05f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0.02f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Set the bone.
            ragdoll[BoneType.LowerLeftLeg] = bone;
        }

        // Upper Right Leg
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.UpperRightLeg];
            bone.parentBone = BoneType.Hip;
            bone.frictionScale = 1.0f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 0.775f * scale);
            bodyDef.linearDamping = 0f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = trouserColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.06f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 0.9f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.05f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0.4f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Set the bone.
            ragdoll[BoneType.UpperRightLeg] = bone;
        }

        // Lower Right Leg.
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.LowerRightLeg];
            bone.parentBone = BoneType.UpperRightLeg;
            bone.frictionScale = 0.5f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 0.475f * scale);
            bodyDef.linearDamping = 0f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = trouserColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.155f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.045f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create foot shape.
            bone.body.CreateShape(footPolygonGeometry, footShapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 0.625f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.5f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0.02f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Set the bone.
            ragdoll[BoneType.LowerRightLeg] = bone;
        }

        // Upper Left Arm.
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.UpperLeftArm];
            bone.parentBone = BoneType.Torso;
            bone.frictionScale = 0.5f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 1.225f * scale);
            bodyDef.linearDamping = 0f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = shirtColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.035f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 1.35f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.1f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0.8f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Set the bone.
            ragdoll[BoneType.UpperLeftArm] = bone;
        }

        // Lower Left Arm.
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.LowerLeftArm];
            bone.parentBone = BoneType.UpperLeftArm;
            bone.frictionScale = 0.1f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 0.975f * scale);
            bodyDef.linearDamping = 0.1f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = skinColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.03f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 1.1f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.2f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0.3f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Set the bone.
            ragdoll[BoneType.LowerLeftArm] = bone;
        }

        // Upper Right Arm.
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.UpperRightArm];
            bone.parentBone = BoneType.Torso;
            bone.frictionScale = 0.5f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 1.225f * scale);
            bodyDef.linearDamping = 0f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = shirtColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.035f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 1.35f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.1f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0.8f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Set the bone.
            ragdoll[BoneType.UpperRightArm] = bone;
        }

        // Lower Right Arm.
        {
            // Fetch the bone.
            var bone = ragdoll[BoneType.LowerRightArm];
            bone.parentBone = BoneType.UpperRightArm;
            bone.frictionScale = 0.1f;

            // Create body.
            bodyDef.position = position + new Vector2(0f, 0.975f * scale);
            bodyDef.linearDamping = 0.1f;
            bone.body = world.CreateBody(bodyDef);

            // Create shape.
            if (configuration.ColorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = skinColor;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.03f * scale };
            bone.body.CreateShape(capsuleGeometry, shapeDef);

            // Create joint.
            var pivot = position + new Vector2(0f, 1.1f * scale);
            var bodyA = ragdoll[bone.parentBone].body;
            var bodyB = bone.body;
            var lowerAngleLimit = PhysicsMath.ToDegrees(-0.2f * PhysicsMath.PI);
            var upperAngleLimit = PhysicsMath.ToDegrees(0.3f * PhysicsMath.PI);
            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bodyA.GetLocalPoint(pivot),
                localAnchorB = bodyB.GetLocalPoint(pivot),
                enableLimit = enableLimit,
                lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                enableMotor = enableMotor,
                maxMotorTorque = bone.frictionScale * maxTorque,
                enableSpring = jointFrequency > 0.0f,
                springFrequency = jointFrequency,
                springDamping = dampingRatio
            };
            bone.joint = world.CreateJoint(jointDef);

            // Set the bone.
            ragdoll[BoneType.LowerRightArm] = bone;
        }

        return ragdoll;
    }
}