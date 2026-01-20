# PhysicsUserData

When creating primary physics types such as `PhysicsWorld`, `PhysicsBody`, `PhysicsShape`, `PhysicsChain`, or any `PhysicsJoint`, it can be useful to attach custom data to the object.

To support this, the physics system provides the type [PhysicsUserData](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsUserData.html).
All primary physics types expose a `.userData` property where you can assign your own user data.

## Creation

The `PhysicsUserData` type offers generic slots for `bool`, `float`, `int`, `object`, and `physicsMask` values, for example:

```csharp
void Run(PhysicsShape myShape)
{
    // Create a physics user-data.
    var physicsUserData = new PhysicsUserData
    {
        objectValue = this,
        boolValue = true,
        floatValue = 123.4f,
        intValue = 567,
        physicsMaskValue = PhysicsMask.All
    };
    
    // Assign it to the shape.
    myShape.userData = physicsUserData;
    
    // Debug.
    // NOTE: Will output "567".
    Debug.Log(myShape.userData.intValue);
}
```

## Custom Usage

The physics system doesn’t interpret `PhysicsUserData`—you decide what its contents mean.

For example, you might use `PhysicsUserData.intValue` to store a power-up index, or `PhysicsUserData.physicsMaskValue` to indicate which player an item belongs to.

