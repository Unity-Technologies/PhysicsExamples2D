# Unity PhysicsCore2D - PhysicsMath Utilities

Expert guidance on using PhysicsMath utilities for physics-specific mathematical operations in Unity PhysicsCore2D.

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

```csharp
// Get active translation axes for plane
var axes = PhysicsMath.GetTranslationAxes(TransformPlane.XY);
// Returns: { 0, 1 } (X and Y axes)

// Get ignored translation axis
int ignoredAxis = PhysicsMath.GetTranslationIgnoredAxis(TransformPlane.XY);
// Returns: 2 (Z axis is ignored)

// Get ignored axes as array
int[] ignoredAxes = PhysicsMath.GetTranslationIgnoredAxes(TransformPlane.XY);
// Returns: { 2 } (Z axis)

// Get rotation axes
var rotAxes = PhysicsMath.GetRotationAxes(TransformPlane.XY);
// Returns rotation axis indices

// Get ignored rotation axes
var ignoredRotAxes = PhysicsMath.GetRotationIgnoredAxes(TransformPlane.XY);
```

### Swizzle
```csharp
// Swap/reorder vector components
Vector3 original = new Vector3(1, 2, 3);
Vector3 swizzled = PhysicsMath.Swizzle(original, 2, 0, 1); // (z, x, y)
// Result: (3, 1, 2)
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
(float cos, float sin) = PhysicsMath.CosSin(angle);

// Equivalent to:
// float cos = Mathf.Cos(angle);
// float sin = Mathf.Sin(angle);
// But faster and deterministic
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
```csharp
// Calculate spring-damper forces
float displacement = 2.0f;      // Distance from equilibrium
float velocity = 1.0f;          // Current velocity
float stiffness = 50f;          // Spring constant
float damping = 5f;             // Damping coefficient

float force = PhysicsMath.SpringDamper(
    displacement,
    velocity,
    stiffness,
    damping
);

// Apply force to body
body.ApplyForce(direction * force);
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
    var (cos, sin) = PhysicsMath.CosSin(angleRadians);
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
```csharp
void SpringCameraToTarget(Transform camera, Vector2 targetPos, ref Vector2 cameraVelocity)
{
    // Get current 2D camera position
    Vector2 currentPos = PhysicsMath.ToPosition2D(
        camera.position,
        TransformPlane.XY
    );

    // Calculate displacement
    Vector2 displacement = targetPos - currentPos;

    // Spring parameters
    float stiffness = 20f;
    float damping = 5f;

    // Calculate spring force for each axis
    float forceX = PhysicsMath.SpringDamper(
        displacement.x,
        cameraVelocity.x,
        stiffness,
        damping
    );

    float forceY = PhysicsMath.SpringDamper(
        displacement.y,
        cameraVelocity.y,
        stiffness,
        damping
    );

    // Update velocity
    Vector2 force = new Vector2(forceX, forceY);
    cameraVelocity += force * Time.deltaTime;

    // Update position
    currentPos += cameraVelocity * Time.deltaTime;

    // Apply back to 3D
    camera.position = PhysicsMath.ToPosition3D(
        currentPos,
        camera.position,
        TransformPlane.XY
    );
}
```

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
    var (cos, sin) = PhysicsMath.CosSin(body.rotation);
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
transform.rotation = PhysicsMath.ToRotationFast3D(body.rotation, plane);
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
