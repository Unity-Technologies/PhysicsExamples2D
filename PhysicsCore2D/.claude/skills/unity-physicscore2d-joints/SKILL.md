---
name: unity-physicscore2d-joints
description: Patterns and decision rules for Unity PhysicsCore2D joints — choosing between PhysicsDistanceJoint, PhysicsFixedJoint, PhysicsHingeJoint, PhysicsIgnoreJoint, PhysicsRelativeJoint, PhysicsSliderJoint, PhysicsWheelJoint; tuning motors, limits, springs; anchor placement; breaking joints via force/torque thresholds; common assemblies (pendulums, vehicle suspension, doors, conveyors, bridges, mouse drag, ragdolls). Use when wiring up physical relationships between bodies. For full member API see unity-physicscore2d-joints-api.
---

# Unity PhysicsCore2D — Joint Patterns

Joints constrain two `PhysicsBody` instances (or one body + the implicit world body) in a particular way. Choosing the right joint and configuring its motor/limit/spring is most of the work — the API surface is small but the parameter space is large.

> For the full type/method API surface (every property, signature, and XML doc), see `unity-physicscore2d-joints-api`. This skill focuses on selection rules, tuning patterns, and worked assemblies.

## Joint selection — quick decision rules

| Goal | Joint |
|------|-------|
| Two bodies separated by a fixed distance (rope, chain link, suspension top tether) | `PhysicsDistanceJoint` |
| Two bodies welded rigidly (with optional softness) — gluing pieces of a destructible together | `PhysicsFixedJoint` |
| Rotation around a single point (door hinge, wheel axle, pendulum, ragdoll limb) | `PhysicsHingeJoint` |
| Disable collision between a specific pair of bodies without affecting layer filtering | `PhysicsIgnoreJoint` |
| Match B's position/rotation to A with controllable spring + simulated friction (top-down character control, AI follow) | `PhysicsRelativeJoint` |
| Linear motion along an axis, no rotation (elevator, piston, sliding door) | `PhysicsSliderJoint` |
| Linear suspension along an axis with rotation around a perpendicular axis (vehicle wheel) | `PhysicsWheelJoint` |

Two joints often compose well: a `PhysicsHingeJoint` plus a `PhysicsDistanceJoint` for a swinging tether with a length stop; a `PhysicsSliderJoint` plus a `PhysicsHingeJoint` for a piston-rod assembly.

## Creation pattern (consistent across all joint types)

```csharp
// Build the definition (struct, copied into the joint on Create).
var def = PhysicsHingeJointDefinition.defaultDefinition;
def.bodyA = anchorBody;
def.bodyB = swingBody;
def.localAnchorA = new PhysicsTransform(Vector2.zero, 0f);
def.localAnchorB = new PhysicsTransform(new Vector2(0, 1f), 0f);
def.collideConnected = false;

// Create — returns a handle struct (cheap to copy, do not cache stale copies past Destroy).
var hinge = PhysicsHingeJoint.Create(world, def);
```

**Key gotchas:**
- Definition is a value type. Always start from `defaultDefinition` so future-added fields get sensible defaults.
- `localAnchorA/B` are in each body's local space. The joint origin is where the two anchor frames coincide in world space at creation time. **The initial transforms can violate the constraint slightly** — the solver will pull them together over a few steps, often with a visible jolt. Pre-position bodies so the anchors coincide.
- `collideConnected = false` is almost always what you want for body pairs you're explicitly joining (otherwise the wrist will collide with the forearm).

## Motors

Hinge, slider, and wheel joints have motors. The pattern is uniform:

```csharp
hinge.enableMotor = true;
hinge.motorSpeed = Mathf.PI;        // radians/sec for hinge, m/s for slider/wheel
hinge.maxMotorTorque = 50f;          // Nm cap (slider/wheel use maxMotorForce in N)
```

**Tuning tips:**
- Start with a generous max torque (~10× expected steady-state load); reduce only if you see oscillation.
- Motor speed sets the *target*; max torque limits how aggressively the solver pursues it. A stalled motor (hitting the cap) is normal and how you simulate friction or weight.
- For "free spinning" with no drive, set `enableMotor = false` rather than `motorSpeed = 0` — the latter actively brakes.
- Read `currentMotorTorque` (hinge) / `currentMotorForce` (slider/wheel) to drive UI, audio, or fuel/battery drain.

## Limits

```csharp
hinge.enableLimit = true;
hinge.lowerAngleLimit = -Mathf.PI / 4;  // -45°
hinge.upperAngleLimit =  Mathf.PI / 4;  //  45°
```

- Angles for hinge are in **radians**. Slider/wheel limits are in meters along the axis.
- The solver enforces limits by injecting impulses at the boundary. Hard limits can chatter — pair with a small `tuningDamping` if needed (defaults are usually fine).

## Springs (soft constraints)

All joint types except `PhysicsIgnoreJoint` expose spring-style softness via `enableSpring`, `springFrequency` (Hz), and `springDamping` (0–1, where 1 ≈ critical). Use this when you want bouncy/compliant behavior:

```csharp
fixedJoint.enableSpring = true;
fixedJoint.springFrequency = 4f;     // 4 Hz oscillation if undamped
fixedJoint.springDamping   = 0.7f;   // mild damping — visible bounce, settles in ~1 sec
```

**Tuning rules of thumb:**
- `springFrequency` < your step rate / 4 (e.g. <15 Hz at 60 fps) for stable behavior.
- `springDamping = 1` is critically damped (no bounce, fastest settle). `< 1` overshoots; `> 1` over-damps.
- A "rigid weld" with `PhysicsFixedJoint` and `enableSpring = false` cannot perfectly hold heavy chains — for those cases, mild spring + high frequency is usually better than fighting the solver.

## Breaking joints via thresholds

Set `forceThreshold` / `torqueThreshold` to non-infinity to receive `JointThresholdEvent` from `PhysicsWorld.jointThresholdEvents` once the constraint exceeds the value. Wire a `PhysicsCallbacks.IJointThreshold` callback to destroy the joint and trigger an effect:

```csharp
hinge.forceThreshold  = 500f;        // newtons
hinge.torqueThreshold = 100f;        // newton-meters
hinge.callbackTarget  = this;        // implements IJointThresholdCallback

public void OnJointThreshold2D(PhysicsWorld world, PhysicsJoint joint, float force, float torque)
{
    joint.Destroy();
    PlaySnapSfx();
}
```

The threshold is checked per simulation step, so brief impulse spikes can break a joint even if average load stays below.

## Worked assemblies

### 1. Door on a wall hinge (limited rotation, weak spring)

```csharp
var def = PhysicsHingeJointDefinition.defaultDefinition;
def.bodyA = wall;
def.bodyB = door;
def.localAnchorA = new PhysicsTransform(new Vector2(doorEdgeX, doorMidY), 0f);
def.localAnchorB = new PhysicsTransform(new Vector2(-doorWidth/2f, 0f), 0f);
def.collideConnected = false;
def.enableLimit = true;
def.lowerAngleLimit = 0f;
def.upperAngleLimit = Mathf.PI;          // 0–180° swing
def.enableSpring = true;
def.springFrequency = 0.5f;              // gentle return-to-closed
def.springDamping = 0.3f;
def.springTargetAngle = 0f;
PhysicsHingeJoint.Create(world, def);
```

### 2. Vehicle wheel suspension (wheel joint with motor)

```csharp
var def = PhysicsWheelJointDefinition.defaultDefinition;
def.bodyA = chassis;
def.bodyB = wheel;
def.localAnchorA = new PhysicsTransform(wheelMountLocal, 0f);
def.localAnchorB = new PhysicsTransform(Vector2.zero, 0f);
def.localAxisA   = new PhysicsRotate(Mathf.PI / 2);     // suspension travels vertically (up axis)
def.collideConnected = false;
def.enableSpring = true;
def.springFrequency = 4f;
def.springDamping = 0.7f;
def.enableMotor = true;
def.motorSpeed  = 0f;                                   // set per-frame from input
def.maxMotorTorque = 200f;
var wheelJoint = PhysicsWheelJoint.Create(world, def);

// Per-frame:
wheelJoint.motorSpeed = throttleInput * maxWheelSpeed;
```

### 3. Mouse drag (distance joint to a kinematic mouse body)

Create one kinematic anchor body that you teleport to the cursor each frame. Attach a soft distance joint when the user picks up an object; destroy it on release.

```csharp
// On pickup:
var def = PhysicsDistanceJointDefinition.defaultDefinition;
def.bodyA = mouseAnchor;                           // kinematic body following cursor
def.bodyB = grabbed;
def.localAnchorA = PhysicsTransform.identity;
def.localAnchorB = new PhysicsTransform(grabbed.WorldToLocal(cursorWorld), 0f);
def.distance = 0f;
def.minDistance = 0f;
def.maxDistance = 0f;
def.enableSpring = true;
def.springFrequency = 5f;       // springy follow
def.springDamping = 0.7f;
mouseJoint = PhysicsDistanceJoint.Create(world, def);

// On release:
mouseJoint.Destroy();
```

### 4. Pendulum / swing rope (hinge anchored to world)

For a single swing point in world space (no body), create one static body to anchor against:

```csharp
var anchorDef = PhysicsBodyDefinition.defaultDefinition;
anchorDef.bodyType = PhysicsBody.BodyType.Static;
anchorDef.position = pivotWorldPos;
var anchorBody = world.CreateBody(anchorDef);

var def = PhysicsHingeJointDefinition.defaultDefinition;
def.bodyA = anchorBody;
def.bodyB = pendulum;
def.localAnchorA = PhysicsTransform.identity;
def.localAnchorB = new PhysicsTransform(new Vector2(0, ropeLength), 0f);
PhysicsHingeJoint.Create(world, def);
```

### 5. Bridge of planks (chain of hinge joints)

```csharp
PhysicsBody prev = leftAnchor;
foreach (var plank in planks)
{
    var def = PhysicsHingeJointDefinition.defaultDefinition;
    def.bodyA = prev;
    def.bodyB = plank;
    def.localAnchorA = new PhysicsTransform(new Vector2(prevHalfWidth, 0f), 0f);
    def.localAnchorB = new PhysicsTransform(new Vector2(-plankHalfWidth, 0f), 0f);
    def.collideConnected = false;
    PhysicsHingeJoint.Create(world, def);
    prev = plank;
}
// Final hinge to the right anchor.
```

### 6. Disable collision between specific pairs (PhysicsIgnoreJoint)

When two bodies should never collide — e.g. a player character and the platform they're parented to — but you don't want to add layer-mask complexity:

```csharp
var def = PhysicsIgnoreJointDefinition.defaultDefinition;
def.bodyA = player;
def.bodyB = platform;
PhysicsIgnoreJoint.Create(world, def);
```

This also keeps both bodies in the same simulation island (woken/slept and solved together), which can be useful for performance with linked mechanisms.

## Drawing & debugging

Every joint has a `Draw()` method and a `worldDrawing` flag. Set `worldDrawing = true` (or call the world's debug draw with joint visuals enabled) to see anchors, axes, and motor state. Adjust `drawScale` if the visuals are too small/large for your scene scale.

## Best practices

- **Always start from `defaultDefinition`** — never zero-init a joint definition struct.
- **Set `collideConnected = false`** for any pair you're joining unless you have a specific reason otherwise.
- **Anchor frames must coincide at creation** — pre-position bodies before calling `Create`.
- **Use `localAxisA` (slider/wheel) deliberately** — the axis is in body-A's local frame and rotates with it.
- **Don't read `currentConstraintForce` / `currentMotorTorque` before the first simulation step** — values are zero until then.
- **Destroy the joint, not the bodies, when you want to disconnect** — destroying a body cascades to its joints automatically.
- **For breakable joints, set thresholds, not "infinite force" limits** — limits are about position; thresholds are about force.

## Common pitfalls

- Setting `motorSpeed = 0` to "stop" a motor — this brakes hard. Use `enableMotor = false`.
- Using degrees for hinge angle limits — they're in **radians**.
- Chains of `PhysicsFixedJoint` going limp under load — the approximate solver can't perfectly weld many bodies. Use `PhysicsComposer` to merge geometry into one body where possible, or use mild spring softness.
- Tightening springs with `springFrequency` >> step rate — causes instability. Cap at ~step rate / 4.
- Forgetting to subscribe a callback target before threshold events fire — the event still fires from `world.jointThresholdEvents`, but `IJointThreshold.OnJointThreshold2D` only fires if `callbackTarget` is set.
