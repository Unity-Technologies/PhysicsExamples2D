---
name: unity-physicscore2d-bodies
description: Patterns for Unity PhysicsCore2D PhysicsBody — choosing body type (Static/Kinematic/Dynamic), creating and destroying bodies, mass and moment of inertia configuration, sleep state management, transform sync between PhysicsBody and Unity Transform, freezing position/rotation via constraints, owner key pattern for ownership, and the WORM rule about creating/destroying bodies during simulation. Use when working with PhysicsBody lifecycle, configuration, or runtime manipulation. For full member API see unity-physicscore2d-bodies-api. For applying forces see unity-physicscore2d-forces. For batched body creation see unity-physicscore2d-batching.
---

# Unity PhysicsCore2D — Body Patterns

`PhysicsBody` is the central simulated entity — every shape, joint, and force ultimately attaches to one. Most "how do I make X behave like Y?" questions come down to body-type choice, mass setup, and lifecycle management.

> For the full type/method API surface (every property, signature, and XML doc), see `unity-physicscore2d-bodies-api`. For force/impulse/explosion patterns see `unity-physicscore2d-forces`. For batched creation/destruction see `unity-physicscore2d-batching`. This skill focuses on body lifecycle and configuration patterns.

## Body type — pick the right one

| Type | Moves | Mass | Collides with | Use for |
|------|-------|------|---------------|---------|
| **Static** | Never | Effectively infinite | Dynamic only | Level geometry, walls, immovable platforms. Cheapest. |
| **Kinematic** | Yes (you set position/velocity) | Effectively infinite | Dynamic only | Player characters with custom controllers, moving platforms, scripted animations, anything you want to move *through* code without simulation pushing it back. |
| **Dynamic** | Yes (forces and collisions) | Real | Everything | Anything that should respond to gravity, impacts, or spring/joint forces. |

**Key consequences:**
- Static-vs-static and Static-vs-Kinematic pairs **never collide**. Two moving platforms (kinematic) won't push each other.
- Dynamic-vs-anything collides. A kinematic platform pushes a dynamic crate naturally.
- Setting a kinematic body's `linearVelocity` makes it sweep correctly into the next step (no tunneling) — better than teleporting via `position`.

## Creation pattern

```csharp
var def = PhysicsBodyDefinition.defaultDefinition;
def.bodyType = PhysicsBody.BodyType.Dynamic;
def.position = new Vector2(0, 5);
def.rotation = 0f;
def.linearVelocity = Vector2.zero;
def.angularVelocity = 0f;
def.gravityScale = 1f;          // 0 = no gravity for this body
def.linearDamping = 0.1f;       // air resistance proxy
def.angularDamping = 0.1f;
def.sleepingAllowed = true;     // let the engine sleep this body when settled

var body = world.CreateBody(def);
```

**Always start from `defaultDefinition`** so future-added fields get sensible defaults. The definition is a value type — modifications after `CreateBody` don't affect the body.

## Adding shapes (a body is empty until you do)

A `PhysicsBody` with no shape simulates as a point — collisions, mass-from-density, and queries all need a shape:

```csharp
var shapeDef = PhysicsShapeDefinition.defaultDefinition;
shapeDef.surfaceMaterial.friction = 0.4f;
shapeDef.surfaceMaterial.bounciness = 0.1f;
shapeDef.density = 1f;          // contributes to body mass when massConfiguration = Auto

body.CreateShape(new CircleGeometry { center = Vector2.zero, radius = 0.5f }, shapeDef);
```

Multiple shapes per body form a **compound** — useful for shapes that aren't a single primitive (a character with circle feet + capsule torso).

## Mass configuration

```csharp
// Default: mass auto-computed from shape density.
def.massConfiguration = PhysicsBody.MassConfiguration.Automatic;

// Override: set mass explicitly, ignore shape density.
def.massConfiguration = PhysicsBody.MassConfiguration.Manual;
def.mass = 5f;                              // kg
def.rotationalInertia = 0.5f;               // kg·m² — affects how easily the body spins
def.localCenterOfMass = new Vector2(0, -0.2f);
```

**Use Manual when:**
- Density-from-shape gives wrong values (e.g. a character should weigh 70 kg regardless of shape area).
- You want a specific moment of inertia for arcade-style spinning.
- You're tweaking gameplay feel (lighter = more reactive to forces).

**Center-of-mass tricks:**
- Lower the COM (e.g. `(0, -0.3)` for a character) to make a body harder to tip over.
- Offset COM forward/back for vehicles to bias steering.

## Constraints — freezing axes

```csharp
def.constraints = PhysicsBody.BodyConstraints.FreezeRotation;
// Or: FreezePositionX, FreezePositionY, FreezeAll, etc. (flags enum, combine with |)
```

`FreezeRotation` is the classic platformer character setting — the body stays upright regardless of impacts.

## Sleep — let settled bodies stop using CPU

```csharp
body.sleepingAllowed = true;
body.Sleep();         // force-sleep now (skipped from sim until something wakes it)
body.Wake();          // force-wake (e.g. when applying forces from script)
bool active = body.isAwake;
```

A sleeping body costs near-zero per step. Bodies wake automatically on contact, force application, or transform change. Disable sleeping for bodies you always need responsive (player character, debug-visualized objects).

## Transform sync to GameObject

PhysicsCore2D bodies aren't attached to GameObjects. Sync each frame in `LateUpdate`:

```csharp
void LateUpdate()
{
    transform.position = PhysicsMath.ToPosition3D(body.position, transform.position, TransformPlane.XY);
    transform.rotation = PhysicsMath.ToRotationFast3D(body.rotation, TransformPlane.XY);
}
```

For top-down on the XZ plane, swap `TransformPlane.XZ`. See `unity-physicscore2d-math` for the full conversion patterns.

For component-based wrapping (script that owns its body), see `unity-physicscore2d-components`.

## Owner key pattern (preventing accidental destruction)

When you want a body to be "owned" by a specific script, register an owner key. Other code that doesn't have the key cannot destroy the body without warning:

```csharp
int ownerKey = body.SetOwner(this);

// Later, only the owner (this script) can destroy:
body.Destroy(ownerKey);

// Code without the key gets a warning and the body is NOT destroyed:
body.Destroy();   // logs a warning if the body is owned
```

Useful in libraries/middleware where you want defensive ownership without complex reference tracking.

## Iteration & introspection

```csharp
// All bodies in the world (allocates — do not call per frame).
using var bodies = world.GetBodies(Allocator.Temp);
foreach (var b in bodies)
{
    if (b.bodyType == PhysicsBody.BodyType.Dynamic && b.linearVelocity.magnitude > 50f)
        b.linearVelocity = b.linearVelocity.normalized * 50f;
}

// Per-body shape iteration.
for (int i = 0; i < body.shapeCount; i++)
{
    var shape = body.GetShape(i);
    // ...
}
```

For per-frame work over many bodies, see batching patterns in `unity-physicscore2d-batching`.

## WORM concurrency rule (critical)

PhysicsCore2D uses a **Write-Once-Read-Many** model: during a simulation step, the world is locked for *structural* mutations. You can read freely, but you **cannot create or destroy bodies, shapes, or joints from inside a contact callback or pre-solve callback**.

If you need to delete a body in response to a collision (e.g. bullet on impact), defer:

```csharp
private readonly List<PhysicsBody> _toDestroy = new();

public void OnContactBegin2D(PhysicsWorld world, PhysicsShape shapeA, PhysicsShape shapeB)
{
    if (shapeA.body.GetOwner<Bullet>() != null)
        _toDestroy.Add(shapeA.body);
}

void LateUpdate()   // outside the simulation step
{
    foreach (var b in _toDestroy) if (b.isValid) b.Destroy();
    _toDestroy.Clear();
}
```

This is by far the most common runtime crash for new users — symptoms are nondeterministic exceptions or "body became invalid" errors mid-callback.

## Position vs velocity vs forces — choosing how to move a body

| Method | When |
|--------|------|
| `body.position = X` (teleport) | Reset to spawn point, undo a glitch, snap to a checkpoint. Skips collision sweeping — can cause penetration. |
| `body.linearVelocity = V` | Kinematic body movement, instantaneous "set the speed", overriding gravity briefly. |
| `body.ApplyImpulse(I)` | One-shot bursts: jumps, explosions, getting kicked. Independent of mass-time integration. |
| `body.ApplyForce(F)` | Continuous influence: thrusters, wind, gravity wells. Scaled by deltaTime internally. |

Setting velocity directly each frame *fights* the physics solver and can cause jitter; for character controllers prefer impulse-on-demand (jump) + velocity adjustment (movement) over per-frame force application. See `unity-physicscore2d-forces` for environmental force patterns.

## Common gotchas

- **Creating a body with no shape** — body has zero collision area but still simulates. Probably not what you wanted.
- **Forgetting to set `bodyType`** — defaults to Static, which never moves. Symptom: "my dynamic body just sits there".
- **Setting `position` during simulation callbacks** — violates WORM, may corrupt state. Defer to `LateUpdate`.
- **Querying `body.position` and getting yesterday's value** — bodies cache transform; if you've teleported, read the value you set.
- **Density of zero on all shapes + `MassConfiguration.Automatic`** — body has zero mass, behaves erratically. Set density on at least one shape, or use Manual mass.
- **Calling `Destroy()` on an already-destroyed body** — handle struct still exists; check `body.isValid` first.

## Best practices

- Start every body from `PhysicsBodyDefinition.defaultDefinition`.
- Pick body type intentionally: Static for level, Kinematic for scripted, Dynamic for simulated.
- Use `linearVelocity` (not `position`) to move kinematic bodies smoothly.
- Add at least one shape with density > 0, or use `MassConfiguration.Manual`.
- Use `FreezeRotation` for upright characters.
- Defer body creation/destruction out of contact callbacks (WORM).
- Sync via `LateUpdate` after the simulation step, using `PhysicsMath` plane helpers.
- Sleep aggressively; wake explicitly when scripting kicks in.
