# Unity PhysicsCore2D - PhysicsMath Utilities

Expert guidance on using PhysicsMath utilities for physics-specific mathematical operations in Unity PhysicsCore2D.

> For the full type/method API surface (every signature, param, and XML doc), see `unity-physicscore2d-math-api`. This skill is a usage-oriented tutorial with worked examples covering TransformPlane conversion patterns, deterministic math, and 12 practical recipes (spring camera, projectile trajectory, angle interpolation, etc.).

## Overview

**PhysicsMath** provides math utilities tailored to the physics system. It focuses on physics-specific operations rather than general mathematical functions.

Key areas:
- TransformPlane conversion (2D physics ↔ 3D Unity)
- Deterministic math functions
- Common physics calculations
- Angle conversions and trigonometry

## TransformPlane Conversion

Convert between 2D physics coordinates and 3D Unity Transform systems using three plane configurations:

### Plane Types
- **XY Plane** - Extracts (X, Y) from 3D coordinates (default Unity 2D)
- **XZ Plane** - Extracts (X, Z) from 3D coordinates (horizontal plane)
- **ZY Plane** - Extracts (Z, Y) from 3D coordinates

### Position Conversion

#### ToPosition2D
```csharp
// Convert 3D position to 2D
Vector3 worldPos3D = transform.position;
Vector2 physicsPos2D = PhysicsMath.ToPosition2D(worldPos3D, TransformPlane.XY);

// Different planes
Vector2 xyPos = PhysicsMath.ToPosition2D(worldPos3D, TransformPlane.XY); // (x, y)
Vector2 xzPos = PhysicsMath.ToPosition2D(worldPos3D, TransformPlane.XZ); // (x, z)
Vector2 zyPos = PhysicsMath.ToPosition2D(worldPos3D, TransformPlane.ZY); // (z, y)
```

#### ToPosition3D
```csharp
// Convert 2D physics position back to 3D
Vector2 physicsPos2D = new Vector2(5, 3);
Vector3 referencePos3D = transform.position; // Preserves depth on ignored axis

Vector3 worldPos3D = PhysicsMath.ToPosition3D(
    physicsPos2D,
    referencePos3D,
    TransformPlane.XY
);

// Result: x=5, y=3, z=referencePos3D.z (preserved)
```

### Rotation Conversion

#### ToRotation2D
```csharp
// Extract 2D rotation from quaternion
Quaternion rotation3D = transform.rotation;
float angle2D = PhysicsMath.ToRotation2D(rotation3D, TransformPlane.XY);

// angle2D is in radians
```

#### ToRotationSlow3D
```csharp
// Integrate 2D rotation into existing 3D rotation (preserves other axes)
float angle2D = Mathf.PI / 4; // 45 degrees in radians
Quaternion referenceRotation = transform.rotation; // Preserves X and Z rotations

Quaternion newRotation3D = PhysicsMath.ToRotationSlow3D(
    angle2D,
    referenceRotation,
    TransformPlane.XY
);

// Result: Y rotation updated, X and Z preserved
```

#### ToRotationFast3D
```csharp
// Convert 2D rotation to 3D (sets missing axes to zero)
float angle2D = Mathf.PI / 4; // 45 degrees in radians

Quaternion rotation3D = PhysicsMath.ToRotationFast3D(
    angle2D,
    TransformPlane.XY
);

// Result: Only Y rotation set, X and Z are zero (faster than Slow version)
```

### Axis Helper Functions

All axis-helper methods accept `PhysicsWorld.TransformPlane` and return `Vector3` bitmasks where **1 = axis active, 0 = axis ignored** — not index arrays or integers.

```csharp
using Unity.U2D.Physics;
using UnityEngine;

// --- GetTranslationAxes ---
// Returns a Vector3 mask: 1 on each axis that PhysicsCore2D uses for translation.
// XY plane → (1, 1, 0)   XZ plane → (1, 0, 1)   ZY plane → (0, 1, 1)
var world = PhysicsWorld.defaultWorld;
PhysicsWorld.TransformPlane plane = world.transformPlane;

Vector3 usedAxes = PhysicsMath.GetTranslationAxes(plane);

// Typical use: verify that the target's scale is non-zero on the physics axes.
// (Direct lift from TestShapeEditorTool.cs / TestChainGeometryTool.cs)
bool hasValidScale = !Mathf.Approximately(
    Vector3.Scale(usedAxes, transform.lossyScale).sqrMagnitude, 0f);

// --- GetTranslationIgnoredAxes ---
// Inverse mask: 1 on the axis that is NOT part of the 2D plane.
// XY plane → (0, 0, 1)   XZ plane → (0, 1, 0)   ZY plane → (1, 0, 0)
// Used as the "depth normal" for 3D handles so they slide in the correct plane.
// (Direct lift from TestChainGeometryTool.cs, TestOutlineShapeTool.cs)
Vector3 handleDirection = PhysicsMath.GetTranslationIgnoredAxes(plane);
// Pass to Handles.Slider2D as the normal so the handle moves in the physics plane.

// --- GetTranslationIgnoredAxis(Vector3, TransformPlane) ---
// Scalar overload: extracts the ignored-axis *value* from a concrete position.
// XY plane → returns position.z   XZ plane → returns position.y   ZY plane → returns position.x
Vector3 worldPos = transform.position;
float depth = PhysicsMath.GetTranslationIgnoredAxis(worldPos, plane);
// Useful to read the "out-of-plane" coordinate when you need to preserve it manually.

// --- GetRotationAxes / GetRotationIgnoredAxes ---
// Same mask pattern for the rotation axis. For all standard planes a single axis
// carries the rotation, so one component is 1 and the other two are 0.
Vector3 rotAxes        = PhysicsMath.GetRotationAxes(plane);        // e.g. XY → (0, 0, 1)
Vector3 rotIgnoredAxes = PhysicsMath.GetRotationIgnoredAxes(plane); // e.g. XY → (1, 1, 0)
```

### Swizzle

`Swizzle` reorders a vector's components so that the physics-plane axes map to the correct 3D axes. There is **no** `(Vector3, int, int, int)` overload — the only overloads take a `TransformPlane`.

```csharp
using Unity.U2D.Physics;
using UnityEngine;

var world = PhysicsWorld.defaultWorld;
PhysicsWorld.TransformPlane plane = world.transformPlane;

// --- Swizzle(Vector3, TransformPlane) ---
// Reorders the components of a 3D vector to match the selected plane.
// For XY: (x, y, z) → (x, y, z)  (no change, XY is the default layout)
// For XZ: (x, y, z) → (x, z, y)  (swaps Y and Z)
// For ZY: (x, y, z) → (z, y, x)  (swaps X and Z)

// Typical editor-tool use: transform a local 2D axis vector into the correct
// 3D orientation, then rotate it by the body's matrix.
// (Direct lift from TestChainGeometryTool.cs / TestShapeEditorTool.cs)
var axisRight = new Vector3(1f, 0f, 0f);
var axisUp    = new Vector3(0f, 1f, 0f);

Matrix4x4 bodyMatrix = body.rotation.GetMatrix(plane);
Vector3 handleRight = bodyMatrix.MultiplyVector(PhysicsMath.Swizzle(axisRight, plane)).normalized;
Vector3 handleUp    = bodyMatrix.MultiplyVector(PhysicsMath.Swizzle(axisUp,    plane)).normalized;
// handleRight / handleUp are now correct 3D screen-space handle directions for Handles.Slider2D.

// --- Swizzle(Vector4, TransformPlane) ---
// Same reorder for a homogeneous position vector (perspective divide preserved).
var pos4 = new Vector4(3f, 2f, 0f, 1f);
Vector4 swizzled4 = PhysicsMath.Swizzle(pos4, plane);

// --- Swizzle(Matrix4x4, TransformPlane) ---
// Swizzles the position column of a 4×4 matrix (the matrix returned by GetRelativeMatrix).
Matrix4x4 relMatrix = PhysicsMath.GetRelativeMatrix(transformFrom, transformTo, plane, useScale: false);
Matrix4x4 swizzledMatrix = PhysicsMath.Swizzle(relMatrix, plane);
```

### ToPhysicsTransform
```csharp
// Direct Transform to PhysicsTransform conversion
Transform unityTransform = gameObject.transform;
PhysicsTransform physicsTransform = PhysicsMath.ToPhysicsTransform(
    unityTransform,
    TransformPlane.XY
);

// Converts position and rotation in one call
```

## Angle Conversion

### Degrees ↔ Radians
```csharp
// Degrees to radians
float degrees = 90f;
float radians = PhysicsMath.ToRadians(degrees);
// Result: ~1.5708 (π/2)

// Radians to degrees
float radians = Mathf.PI;
float degrees = PhysicsMath.ToDegrees(radians);
// Result: 180
```

## Deterministic Math

For reproducible physics simulations across platforms:

### Constants
```csharp
// Deterministic PI
float pi = PhysicsMath.PI;
// Guaranteed same value across platforms

// Deterministic TAU (2 * PI)
float tau = PhysicsMath.TAU;
```

### Atan2
```csharp
// Deterministic arc tangent (two arguments)
float angle = PhysicsMath.Atan2(y, x);

// Use instead of Mathf.Atan2 for deterministic simulations
Vector2 direction = new Vector2(1, 1);
float angleInRadians = PhysicsMath.Atan2(direction.y, direction.x);
```

### CosSin
```csharp
// Combined cosine and sine calculation (more efficient)
float angle = PhysicsMath.PI / 4;

// out-param form (CosSin(float, float, float)):
PhysicsMath.CosSin(angle, out var cos, out var sin);

// Vector2 form (CosSin(float) returns Vector2 where x=cos, y=sin):
Vector2 cs = PhysicsMath.CosSin(angle);
float cosVal = cs.x;
float sinVal = cs.y;
```

## Utility Functions

### MaxAbsComponent
```csharp
// Get maximum absolute component value
Vector2 vec = new Vector2(-5, 3);
float maxAbs = PhysicsMath.MaxAbsComponent(vec);
// Result: 5 (abs(-5) is larger than abs(3))

Vector3 vec3 = new Vector3(2, -8, 4);
float maxAbs3 = PhysicsMath.MaxAbsComponent(vec3);
// Result: 8
```

### MinAbsComponent
```csharp
// Get minimum absolute component value
Vector2 vec = new Vector2(-5, 3);
float minAbs = PhysicsMath.MinAbsComponent(vec);
// Result: 3 (abs(3) is smaller than abs(-5))

Vector3 vec3 = new Vector3(2, -8, 4);
float minAbs3 = PhysicsMath.MinAbsComponent(vec3);
// Result: 2
```

### AngularVelocityToQuaternion
```csharp
// Convert angular velocity to quaternion rotation
float angularVelocity = 2f; // radians per second
float deltaTime = Time.fixedDeltaTime;

Quaternion deltaRotation = PhysicsMath.AngularVelocityToQuaternion(
    angularVelocity,
    deltaTime,
    TransformPlane.XY
);

// Apply to transform
transform.rotation *= deltaRotation;
```

### SpringDamper

`SpringDamper` runs a **1-D mass-spring-damper** that drives a translation toward zero. It returns the **new speed**; the caller integrates position separately.

```
newSpeed = SpringDamper(frequency, damping, translation, speed, deltaTime);
translation += newSpeed * deltaTime;
```

| Param | Type | Meaning |
|---|---|---|
| `frequency` | `float` | Spring frequency in cycles/second (Hz). Higher = stiffer. |
| `damping` | `float` | Damping ratio (≥ 0). 1 = critically damped, >1 = overdamped. |
| `translation` | `float` | Current displacement from equilibrium. |
| `speed` | `float` | Current speed (signed). |
| `deltaTime` | `float` | Time step in seconds. |

```csharp
using Unity.U2D.Physics;
using UnityEngine;

// Pogo-stick spring — drives the leg toward its rest length.
// (Direct lift from Sandbox/Assets/Examples/CharacterMover.cs)
private float m_PogoVelocity;
private float m_PogoFrequency = 8f;  // Hz
private float m_PogoDamping   = 0.5f; // under-damped for a bouncy feel

void UpdatePogo(float pogoCurrentLength, float pogoRestLength, float deltaTime)
{
    float offset = pogoCurrentLength - pogoRestLength; // positive = leg too long

    m_PogoVelocity = PhysicsMath.SpringDamper(
        frequency:  m_PogoFrequency,
        damping:    m_PogoDamping,
        translation: offset,
        speed:      m_PogoVelocity,
        deltaTime:  deltaTime);

    // Integrate: apply the new speed as a vertical impulse on the character body.
    float verticalDelta = m_PogoVelocity * deltaTime;
    // e.g. body.ApplyLinearImpulse(new Vector2(0f, verticalDelta));
}
```

## Practical Examples

### Sync Physics Body with Transform (XY Plane)
```csharp
void SyncPhysicsToTransform(PhysicsBody body, Transform transform)
{
    // Get 2D physics position and rotation
    Vector2 physicsPos2D = body.position;
    float physicsAngle2D = body.rotation;

    // Convert to 3D Transform
    transform.position = PhysicsMath.ToPosition3D(
        physicsPos2D,
        transform.position, // Preserve Z
        TransformPlane.XY
    );

    transform.rotation = PhysicsMath.ToRotationSlow3D(
        physicsAngle2D,
        transform.rotation, // Preserve X and Z rotations
        TransformPlane.XY
    );
}
```

### Sync Transform to Physics Body (XZ Plane)
```csharp
void SyncTransformToPhysics(Transform transform, PhysicsBody body)
{
    // Convert 3D Transform to 2D physics (XZ horizontal plane)
    Vector2 physicsPos2D = PhysicsMath.ToPosition2D(
        transform.position,
        TransformPlane.XZ
    );

    float physicsAngle2D = PhysicsMath.ToRotation2D(
        transform.rotation,
        TransformPlane.XZ
    );

    // Update physics body
    body.SetTransform(physicsPos2D, physicsAngle2D);
}
```

### Calculate Direction Angle
```csharp
Vector2 GetDirectionAngle(Vector2 from, Vector2 to)
{
    Vector2 direction = to - from;
    float angle = PhysicsMath.Atan2(direction.y, direction.x);
    return angle;
}

// Usage
Vector2 playerPos = new Vector2(0, 0);
Vector2 targetPos = new Vector2(5, 5);
float angleToTarget = GetDirectionAngle(playerPos, targetPos);

// Rotate physics body to face target
body.rotation = angleToTarget;
```

### Create Rotation from Direction
```csharp
void FaceDirection(PhysicsBody body, Vector2 direction)
{
    float angle = PhysicsMath.Atan2(direction.y, direction.x);
    body.rotation = angle;
}

// Usage
Vector2 moveDirection = new Vector2(1, 1).normalized;
FaceDirection(body, moveDirection);
```

### Calculate Unit Circle Point
```csharp
Vector2 GetPointOnCircle(float angleRadians, float radius)
{
    PhysicsMath.CosSin(angleRadians, out var cos, out var sin);
    return new Vector2(cos * radius, sin * radius);
}

// Usage - spawn objects in circle
int count = 8;
for (int i = 0; i < count; i++)
{
    float angle = (i / (float)count) * PhysicsMath.TAU;
    Vector2 position = GetPointOnCircle(angle, 5f);
    // Spawn object at position
}
```

### Spring Camera Follow

Apply independent X and Y spring-dampers to smoothly follow a physics body. Each axis maintains its own speed state and integrates via `translation += newSpeed * deltaTime`.

```csharp
using Unity.U2D.Physics;
using UnityEngine;

public class SpringCameraFollow : MonoBehaviour
{
    [Header("Spring settings")]
    public float frequency = 3f;   // Hz — lower for a lazier follow
    public float damping   = 0.8f; // 1 = critically damped (no overshoot)

    private Vector2 m_CameraSpeed; // per-axis spring speed, persists across frames

    // Call from LateUpdate after physics has stepped.
    public void Follow(PhysicsBody target, PhysicsWorld.TransformPlane plane)
    {
        var world = PhysicsWorld.defaultWorld;
        float dt = world.stepDeltaTime;

        // Convert the target body's 2D position to a 3D camera goal.
        Vector3 goalPos3D = PhysicsMath.ToPosition3D(
            target.position,
            transform.position,   // preserve the out-of-plane axis (e.g. camera Z)
            plane);

        // Compute displacement on each active axis.
        float dx = goalPos3D.x - transform.position.x;
        float dy = goalPos3D.y - transform.position.y;

        // Run a spring-damper per axis.
        m_CameraSpeed.x = PhysicsMath.SpringDamper(
            frequency:   frequency,
            damping:     damping,
            translation: dx,
            speed:       m_CameraSpeed.x,
            deltaTime:   dt);

        m_CameraSpeed.y = PhysicsMath.SpringDamper(
            frequency:   frequency,
            damping:     damping,
            translation: dy,
            speed:       m_CameraSpeed.y,
            deltaTime:   dt);

        // Integrate: move the camera.
        transform.position += new Vector3(
            m_CameraSpeed.x * dt,
            m_CameraSpeed.y * dt,
            0f);
    }
}
```

Key points:
- `translation` is the **signed error** (goal minus current), not absolute position.
- `speed` state must be **stored between frames** — do not pass zero every tick.
- `deltaTime` should match `PhysicsWorld.stepDeltaTime` (or `Time.fixedDeltaTime`) so the spring responds at physics rate, not render rate.

### Deterministic Projectile Trajectory
```csharp
Vector2 CalculateTrajectoryPoint(Vector2 start, Vector2 velocity, float gravity, float time)
{
    // Use deterministic math
    float halfGravity = gravity * 0.5f;
    float timeSquared = time * time;

    return new Vector2(
        start.x + velocity.x * time,
        start.y + velocity.y * time - halfGravity * timeSquared
    );
}
```

### Angle Interpolation
```csharp
float InterpolateAngle(float fromAngle, float toAngle, float t)
{
    // Normalize angles to [-PI, PI]
    float delta = toAngle - fromAngle;

    // Wrap delta to [-PI, PI]
    while (delta > PhysicsMath.PI) delta -= PhysicsMath.TAU;
    while (delta < -PhysicsMath.PI) delta += PhysicsMath.TAU;

    return fromAngle + delta * t;
}

// Usage
float startAngle = 0;
float endAngle = PhysicsMath.PI * 1.5f; // 270 degrees
float interpolated = InterpolateAngle(startAngle, endAngle, 0.5f);
```

### Convert Degrees to 2D Rotation
```csharp
void SetRotationDegrees(PhysicsBody body, float degrees)
{
    float radians = PhysicsMath.ToRadians(degrees);
    body.rotation = radians;
}

// Usage
SetRotationDegrees(body, 45f); // 45 degrees
SetRotationDegrees(body, 90f); // 90 degrees
```

### Get Body Heading Vector
```csharp
Vector2 GetHeadingVector(PhysicsBody body)
{
    PhysicsMath.CosSin(body.rotation.radians, out var cos, out var sin);
    return new Vector2(cos, sin);
}

// Usage
Vector2 forward = GetHeadingVector(body);
body.ApplyForce(forward * thrustPower);
```

## TransformPlane Usage Patterns

### XY Plane (Standard 2D)
```csharp
// Standard Unity 2D games
const TransformPlane plane = TransformPlane.XY;

// Sync physics to GameObjects
transform.position = PhysicsMath.ToPosition3D(body.position, transform.position, plane);
transform.rotation = PhysicsMath.ToRotationFast3D(body.rotation.radians, plane);
```

### XZ Plane (Top-Down)
```csharp
// Top-down games (horizontal plane)
const TransformPlane plane = TransformPlane.XZ;

// Physics operates on X and Z axes, Y is height
Vector2 groundPos = PhysicsMath.ToPosition2D(transform.position, plane);
body.SetTransform(groundPos, PhysicsMath.ToRotation2D(transform.rotation, plane));
```

### ZY Plane (Side View with Z)
```csharp
// Side-scrolling with Z instead of X
const TransformPlane plane = TransformPlane.ZY;

// Physics operates on Z and Y axes
Vector2 physicsPos = PhysicsMath.ToPosition2D(transform.position, plane);
body.SetTransform(physicsPos, PhysicsMath.ToRotation2D(transform.rotation, plane));
```

## Best Practices

- **Use deterministic math for networked games** - Ensures consistency
- **Use CosSin instead of separate Cos/Sin** - More efficient
- **Cache TransformPlane value** - Don't recalculate every frame
- **Use ToRotationFast3D when possible** - Faster than Slow version
- **Use PhysicsMath constants** - PI and TAU are deterministic
- **Normalize angles to [-PI, PI]** - Prevents overflow issues
- **Use ToPhysicsTransform** - More efficient than separate calls
- **Profile coordinate conversions** - Can be expensive in loops

## Performance Considerations

- TransformPlane conversions have a cost
- Cache converted values when possible
- Use Fast variants when precision isn't critical
- Batch conversions when updating many objects
- Consider using jobs for bulk conversions

## Common Pitfalls

- **Don't mix degrees and radians** - PhysicsCore2D uses radians
- **Don't forget TransformPlane parameter** - Different planes give different results
- **Preserve ignored axes** - Use ToPosition3D correctly with reference
- **Angle wrapping** - Angles can exceed [-PI, PI], wrap them
- **Fast vs Slow rotation** - Fast zeros out other axes, Slow preserves them

## Thread Safety

PhysicsMath functions are thread-safe and can be used in Burst-compiled jobs:

```csharp
[BurstCompile]
struct ConvertPositionsJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> positions3D;
    [WriteOnly] public NativeArray<Vector2> positions2D;

    public void Execute(int index)
    {
        positions2D[index] = PhysicsMath.ToPosition2D(
            positions3D[index],
            TransformPlane.XY
        );
    }
}
```
