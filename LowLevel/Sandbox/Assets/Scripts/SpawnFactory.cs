using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

public static class SpawnFactory
{
    public static class Softbody
    {
        public static NativeList<PhysicsBody> SpawnDonut(PhysicsWorld world, IShapeColorProvider colorProvider, Vector2 position, int sides = 7, float scale = 1f, bool triggerEvents = false, float jointFrequency = 5f, float jointDamping = 0.0f, Allocator allocator = Allocator.Temp)
        {
            NativeList<PhysicsBody> bodies = new(allocator);

            var radius = 1.0f * scale;
            var deltaAngle = PhysicsMath.TAU / sides;
            var length = PhysicsMath.TAU * radius / sides;

            var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.5f * length), center2 = new Vector2(0f, 0.5f * length), radius = 0.25f * scale };
            var center = position;

            var bodyDef = new PhysicsBodyDefinition { bodyType = RigidbodyType2D.Dynamic };
            var shapeDef = new PhysicsShapeDefinition
            {
                surfaceMaterial = new PhysicsShape.SurfaceMaterial
                {
                    friction = 0.3f,
                    customColor = colorProvider.ShapeColorState
                },
                contactFilter = new PhysicsShape.ContactFilter { categories = 1, contacts = PhysicsMask.All, groupIndex = -1 },
                triggerEvents = triggerEvents
            };

            // Create bodies.
            var bodyIndex = bodies.Length;
            var angle = 35f;
            for (var i = 0; i < sides; ++i)
            {
                bodyDef.position = new Vector2(radius * math.cos(angle) + center.x, radius * math.sin(angle) + center.y);
                bodyDef.rotation = new PhysicsRotate(angle);

                var body = world.CreateBody(bodyDef);
                bodies.Add(body);

                body.CreateShape(capsuleGeometry, shapeDef);

                shapeDef.surfaceMaterial.customColor = colorProvider.ShapeColorState;

                angle += deltaAngle;
            }

            // Create joints.
            var fixedDefinition = new PhysicsFixedJointDefinition
            {
                angularHertz = jointFrequency,
                angularDampingRatio = jointDamping,
                localAnchorA = new Vector2(0.0f, 0.5f * length),
                localAnchorB = new Vector2(0.0f, -0.5f * length)
            };

            var prevBody = bodies[bodies.Length - 1];
            for (var i = 0; i < sides; ++i)
            {
                var currentBody = bodies[bodyIndex + i];
                fixedDefinition.bodyA = prevBody;
                fixedDefinition.bodyB = currentBody;
                world.CreateJoint(fixedDefinition);
                prevBody = currentBody;
            }

            return bodies;
        }
    }

    public static class Ragdoll
    {
        [Serializable]
        public struct Configuration
        {
            public Configuration(
                Vector2 scaleRange,
                float jointHertz,
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
                JointHertz = jointHertz;
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
            [Min(0f)] public float JointHertz;
            [Min(0f)] public float JointDamping;
            [Min(0f)] public float JointFriction;
            [Min(0f)] public float GravityScale;
            public UInt64 ContactBodyLayer;
            public UInt64 ContactFeetLayer;
            public int ContactGroupIndex;
            public IShapeColorProvider ColorProvider;
            public bool TriggerEvents;
            [FormerlySerializedAs("FastCollisions")] public bool FastCollisionsAllowed;
            public bool EnableMotor;
            public bool EnableLimits;
        }

        private enum BoneType
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

        private struct Bone
        {
            public PhysicsBody body;
            public PhysicsJoint joint;
            public float frictionScale;
            public BoneType parentBone;
        }

        private struct BoneArray : IDisposable
        {
            private NativeArray<Bone> m_BoneArray;

            public BoneArray(Allocator allocator = Allocator.Temp)
            {
                m_BoneArray = new NativeArray<Bone>((int)BoneType.Count, allocator);

                for (var i = 0; i < (int)BoneType.Count; ++i)
                {
                    m_BoneArray[i] = new Bone
                    {
                        body = new PhysicsBody(),
                        joint = new PhysicsJoint(),
                        frictionScale = 1.0f,
                        parentBone = BoneType.Invalid
                    };
                }
            }

            public void Dispose()
            {
                m_BoneArray.Dispose();
            }

            public Bone this[BoneType boneType]
            {
                get => m_BoneArray[(int)boneType];
                set => m_BoneArray[(int)boneType] = value;
            }
        }

        public static NativeList<PhysicsBody> SpawnRagdoll(PhysicsWorld world, Vector2 position, Configuration configuration, bool rightFacing, ref Random random)
        {
            NativeList<PhysicsBody> bodies = new(Allocator.Persistent);

            // Create the bone array.
            var bones = new BoneArray(Allocator.Temp);

            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
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
            var maxTorque = configuration.JointFriction * scale;
            var hertz = configuration.JointHertz;
            var dampingRatio = configuration.JointDamping;
            var enableMotor = configuration.EnableMotor;
            var enableLimit = configuration.EnableLimits;

            var shirtColor = Color.mediumTurquoise;
            var trouserColor = Color.dodgerBlue;
            var skinColors = new[] { Color.navajoWhite, Color.lightYellow, Color.peru, Color.tan };
            var skinColor = skinColors[random.NextInt(0, skinColors.Length)];

            // Hip.
            {
                // Fetch the bone.
                var bone = bones[BoneType.Hip];

                // Create body.
                bodyDef.position = position + new Vector2(0f, 0.95f * scale);
                bodyDef.linearDamping = 0f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = trouserColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.02f * scale), center2 = new Vector2(0f, 0.02f * scale), radius = 0.095f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Set the bone.
                bones[BoneType.Hip] = bone;
            }

            // Torso.
            {
                // Fetch the bone.
                var bone = bones[BoneType.Torso];
                bone.parentBone = BoneType.Hip;
                bone.frictionScale = 0.5f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 1.2f * scale);
                bodyDef.linearDamping = 0f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = shirtColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.135f * scale), center2 = new Vector2(0f, 0.135f * scale), radius = 0.09f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 1.0f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.25f * PhysicsMath.PI;
                var upperAngleLimit = 0f * PhysicsMath.PI;
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
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.Torso] = bone;
            }

            // Head
            {
                // Fetch the bone.
                var bone = bones[BoneType.Head];
                bone.parentBone = BoneType.Torso;
                bone.frictionScale = 0.25f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 1.475f * scale);
                bodyDef.linearDamping = 0.1f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = skinColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.038f * scale), center2 = new Vector2(0f, 0.039f * scale), radius = 0.075f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 1.4f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.3f * PhysicsMath.PI;
                var upperAngleLimit = 0.1f * PhysicsMath.PI;
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
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.Head] = bone;
            }

            // Upper Left Leg
            {
                // Fetch the bone.
                var bone = bones[BoneType.UpperLeftLeg];
                bone.parentBone = BoneType.Hip;
                bone.frictionScale = 1f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 0.775f * scale);
                bodyDef.linearDamping = 0f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = trouserColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.06f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 0.9f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.05f * PhysicsMath.PI;
                var upperAngleLimit = 0.4f * PhysicsMath.PI;
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
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.UpperLeftLeg] = bone;
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
                var bone = bones[BoneType.LowerLeftLeg];
                bone.parentBone = BoneType.UpperLeftLeg;
                bone.frictionScale = 0.5f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 0.475f * scale);
                bodyDef.linearDamping = 0f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = trouserColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.155f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.045f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create foot shape.
                bone.body.CreateShape(footPolygonGeometry, footShapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 0.625f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.05f * PhysicsMath.PI;
                var upperAngleLimit = 0.02f * PhysicsMath.PI;
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
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.LowerLeftLeg] = bone;
            }

            // Upper Right Leg
            {
                // Fetch the bone.
                var bone = bones[BoneType.UpperRightLeg];
                bone.parentBone = BoneType.Hip;
                bone.frictionScale = 1.0f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 0.775f * scale);
                bodyDef.linearDamping = 0f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = trouserColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.06f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 0.9f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.05f * PhysicsMath.PI;
                var upperAngleLimit = 0.4f * PhysicsMath.PI;
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
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.UpperRightLeg] = bone;
            }

            // Lower Right Leg.
            {
                // Fetch the bone.
                var bone = bones[BoneType.LowerRightLeg];
                bone.parentBone = BoneType.UpperRightLeg;
                bone.frictionScale = 0.5f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 0.475f * scale);
                bodyDef.linearDamping = 0f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = trouserColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.155f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.045f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create foot shape.
                bone.body.CreateShape(footPolygonGeometry, footShapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 0.625f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.5f * PhysicsMath.PI;
                var upperAngleLimit = 0.02f * PhysicsMath.PI;
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
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.LowerRightLeg] = bone;
            }

            // Upper Left Arm.
            {
                // Fetch the bone.
                var bone = bones[BoneType.UpperLeftArm];
                bone.parentBone = BoneType.Torso;
                bone.frictionScale = 0.5f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 1.225f * scale);
                bodyDef.linearDamping = 0f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = shirtColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.035f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 1.35f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.1f * PhysicsMath.PI;
                var upperAngleLimit = 0.8f * PhysicsMath.PI;
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
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.UpperLeftArm] = bone;
            }

            // Lower Left Arm.
            {
                // Fetch the bone.
                var bone = bones[BoneType.LowerLeftArm];
                bone.parentBone = BoneType.UpperLeftArm;
                bone.frictionScale = 0.1f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 0.975f * scale);
                bodyDef.linearDamping = 0.1f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = skinColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.03f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 1.1f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.2f * PhysicsMath.PI;
                var upperAngleLimit = 0.3f * PhysicsMath.PI;
                var referenceAngle = 0.25f * PhysicsMath.PI;
                var jointDef = new PhysicsHingeJointDefinition
                {
                    bodyA = bodyA,
                    bodyB = bodyB,
                    localAnchorA = bodyA.GetLocalPoint(pivot),
                    localAnchorB = bodyB.GetLocalPoint(pivot),
                    //referenceAngle = rightFacing ? referenceAngle : -referenceAngle,
                    enableLimit = enableLimit,
                    lowerAngleLimit = rightFacing ? lowerAngleLimit : -upperAngleLimit,
                    upperAngleLimit = rightFacing ? upperAngleLimit : -lowerAngleLimit,
                    enableMotor = enableMotor,
                    maxMotorTorque = bone.frictionScale * maxTorque,
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.LowerLeftArm] = bone;
            }

            // Upper Right Arm.
            {
                // Fetch the bone.
                var bone = bones[BoneType.UpperRightArm];
                bone.parentBone = BoneType.Torso;
                bone.frictionScale = 0.5f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 1.225f * scale);
                bodyDef.linearDamping = 0f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = shirtColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.035f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 1.35f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.1f * PhysicsMath.PI;
                var upperAngleLimit = 0.8f * PhysicsMath.PI;
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
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.UpperRightArm] = bone;
            }

            // Lower Right Arm.
            {
                // Fetch the bone.
                var bone = bones[BoneType.LowerRightArm];
                bone.parentBone = BoneType.UpperRightArm;
                bone.frictionScale = 0.1f;

                // Create body.
                bodyDef.position = position + new Vector2(0f, 0.975f * scale);
                bodyDef.linearDamping = 0.1f;
                bone.body = world.CreateBody(bodyDef);
                bodies.Add(bone.body);

                // Create shape.
                if (configuration.ColorProvider is not { IsShapeColorActive: true })
                    shapeDef.surfaceMaterial.customColor = skinColor;

                var capsuleGeometry = new CapsuleGeometry { center1 = new Vector2(0f, -0.125f * scale), center2 = new Vector2(0f, 0.125f * scale), radius = 0.03f * scale };
                bone.body.CreateShape(capsuleGeometry, shapeDef);

                // Create joint.
                var pivot = position + new Vector2(0f, 1.1f * scale);
                var bodyA = bones[bone.parentBone].body;
                var bodyB = bone.body;
                var lowerAngleLimit = -0.2f * PhysicsMath.PI;
                var upperAngleLimit = 0.3f * PhysicsMath.PI;
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
                    enableSpring = hertz > 0.0f,
                    springHertz = hertz,
                    springDampingRatio = dampingRatio
                };
                bone.joint = world.CreateJoint(jointDef);

                // Set the bone.
                bones[BoneType.LowerRightArm] = bone;
            }

            // Dispose of the bones.
            bones.Dispose();

            return bodies;
        }
    }

    public static class Gear
    {
        public static NativeList<PhysicsBody> SpawnGear(PhysicsWorld world, IShapeColorProvider colorProvider, PhysicsShape.ContactFilter contactFilter, Vector2 gearPosition, float gearRadius, bool useMotor = false, float motorSpeed = 0f, Allocator allocator = Allocator.Temp)
        {
            NativeList<PhysicsBody> bodies = new(allocator);
            
            var toothHalfWidth = 0.09f * gearRadius;
            var toothHalfHeight = 0.06f * gearRadius;
            var toothRadius = 0.03f * gearRadius;
            
            // Ground Body.
            var groundBody = world.CreateBody(PhysicsBodyDefinition.defaultDefinition);
            bodies.Add(groundBody);
            bodies.Add(groundBody);
            
            var bodyDef = new PhysicsBodyDefinition
            {
                bodyType = RigidbodyType2D.Dynamic,
                position = gearPosition
            };

            var gearBody = world.CreateBody(bodyDef);
            bodies.Add(gearBody);

            var shapeDef = new PhysicsShapeDefinition
            {
                contactFilter = contactFilter,
                surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f }
            };

            if (colorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = Color.saddleBrown;

            var circle = new CircleGeometry { radius = gearRadius };
            gearBody.CreateShape(circle, shapeDef);

            const int count = 16;
            var deltaAngle = PhysicsMath.TAU / 16f;
            var dq = new PhysicsRotate(deltaAngle);
            var center = new Vector2(gearRadius + toothHalfHeight, 0f);
            var rotation = PhysicsRotate.identity;

            shapeDef.surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.1f };
            if (colorProvider is not { IsShapeColorActive: true })
                shapeDef.surfaceMaterial.customColor = Color.gray;
            
            for (var i = 0; i < count; ++i)
            {
                var tooth = PolygonGeometry.CreateBox(
                    size: new Vector2(toothHalfWidth, toothHalfHeight) * 2f,
                    radius: toothRadius,
                    transform: new PhysicsTransform(center, rotation));

                gearBody.CreateShape(tooth, shapeDef);

                rotation = dq.MultiplyRotation(rotation);
                center = rotation.RotateVector(new Vector2(gearRadius + toothHalfHeight, 0.0f));
            }

            var jointDef = new PhysicsHingeJointDefinition
            {
                bodyA = groundBody,
                bodyB = gearBody,
                localAnchorA = groundBody.GetLocalPoint(gearPosition),
                localAnchorB = Vector2.zero,
                enableMotor = useMotor,
                maxMotorTorque = 100000f,
                motorSpeed = motorSpeed
            };
			
            // Create the gear hinge.
            world.CreateJoint(jointDef);

            return bodies;
        }
    }
}
