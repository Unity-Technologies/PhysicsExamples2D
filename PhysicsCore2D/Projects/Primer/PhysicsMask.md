# PhysicsMask

Many physics operations, like collision filtering or world queries, need to quickly select specific items.
A `PhysicsMask` is a 64-bit value that can represent up to 64 different items.
It's used for `contact filtering` for shapes and for filtering results in world queries.
Having a 64-bit `PhysicsMask` has many other physics and non-physics use-cases too.

It is most commonly used in conjunction with physics layers. See [PhysicsLayers](PhysicsLayers.md) for more information.

## Creation

```csharp
void Run()
{
    // Create a physics mask with all bits reset.
    var physicsMask1 = new PhysicsMask();

    // Create a physics mask
    var physicsMask2 = new PhysicsMask(1, 4, 10, 60);

    // Get a physics mask by converting a 32-bit layer mask. 
    var physicsMask3 = new PhysicsMask(LayerMask.NameToLayer("Player"));
}
```

## Operations

The `PhysicsMask` type provides a set of commonly useful operations:
- **Default Selections**: You can select [All](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.All.html), [One](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.One.html) or [None](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.None.html) bits by default.
- **Modify Bits**: You can [Set](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.SetBit.html) and [Reset](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.ResetBit.html) a bit.
- **Read Bits**: You can check if a [Bit](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.IsBitSet.html) is set or if multiple [Bits](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.AreBitsSet.html) are set.
- **BitMask**: You can get or set the underlying `long` [bitmask](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask-bitMask.html) directly.
- **Legacy LayerMask**: You can convert a legacy `LayerMask` with [ToLayerMask](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.ToLayerMask.html).

## Bit Operators

The `PhysicsMask` provides multiple bitwise operators:

```csharp
void Run()
{
    // Create a physics mask
    var physicsMask = new PhysicsMask();

    // OR a bitmask of 1.
    physicsMask |= new PhysicsMask(1);

    // XOR a bit mask of 2.
    physicsMask ^= new PhysicsMask(2);
    
    // Get a NOT of a bitmask.
    var notPhysicsMask = ~physicsMask;
    
    // AND a bitmask.
    physicsMask &= notPhysicsMask;

    // Left-shift a bitmask.
    var shiftLeftPhysicsMask = physicsMask << 3;
    
    // Right-shift a bitmask.
    var shiftRightPhysicsMask = physicsMask >> 2;
}
```

## Bit Iteration

The `PhysicMask` can iterate all the set bits in the mask like so:

```csharp
void Run()
{
    // Create a physics mask
    var physicsMask = new PhysicsMask(1, 4, 10, 60);

    // Iterate the set bits.
    // NOTE: This should output 1, 4, 10 and 60.
    foreach (var bit in physicsMask)
    {
        Debug.Log(bit);
    }
}
```

## Editor

By default, a `PhysicsMask` field in a script is shown in the Unity Editor as a set of layers:

![PhysicsMask-ShowAsPhysicsLayers](Images/PhysicsMask-ShowAsPhysicsLayers.png)

If your `PhysicsMask` represents an arbitrary 64-bit flag mask rather than layers, add the [`PhysicsMask.ShowAsPhysicsMask`](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsMask.ShowAsPhysicsMaskAttribute.html) attribute to the field.
The Editor will then display it as a raw 64-bit mask, showing only the selected bits:

![PhysicsMask-ShowAsPhysicsMaskAttribute](Images/PhysicsMask-ShowAsPhysicsMaskAttribute.png)

See [PhysicsLayers](PhysicsLayers.md) for more information on the physics layer system.
