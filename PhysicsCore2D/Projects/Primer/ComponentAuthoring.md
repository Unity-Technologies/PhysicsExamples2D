# Component Authoring

When you create components that use the physics API (e.g., creating `PhysicsWorld`, `PhysicsBody`, `PhysicsShape`, `PhysicsChain`, `PhysicsJoint` objects), it's important to prevent these objects from being accidentally deleted by other developers.

For instance, if your "Gear" component creates a `PhysicsShape` for a tooth, and another developer writes code that queries the world and finds that tooth shape, they might accidentally call `PhysicsShape.Destroy()` on it. From their perspective, it's a valid operation, but suddenly your "Gear" component is broken (missing a tooth) and no longer functions correctly.

To prevent such accidental deletions, the API provides an "ownership" feature. It's a simple gate-keeping mechanism.

## How Ownership Works

When you create a physics object, you can choose to "own" it by calling `SetOwner()`. As an example with a `PhysicsBody`:

```csharp
class GearComponent : MonoBehaviour
{
    PhysicsBody m_PhysicsBody;
    int m_OwnerKey;
    
    void OnEnable()
    {
        // Get the main physics world.
        var world = PhysicsWorld.defaultWorld;
          
        // Create a body.
        m_PhysicsBody = world.CreateBody();
        
        // Assign this component script as the owner (optional) and get an "owner key" (integer).
        // Once ownership is set, it cannot be changed!
        m_OwnerKey = m_PhysicsBody.SetOwner(this);
    }
}
```

If another script then tries to destroy this `PhysicsBody` without the owner key:

```csharp
// This attempt to destroy the body will FAIL!
body.Destroy();
```

The API will output a console warning:
`UnityEngine.LowLevelPhysics2D.PhysicsBody.Destroy was called but encountered a problem: Cannot destroy a body that is owned without a valid owner key being specified.`

This means that to destroy an owned `PhysicsBody`, you *must* provide the `ownerKey` that was returned when ownership was originally set.

Here's how to properly destroy an owned `PhysicsBody`:

```csharp
class GearComponent : MonoBehaviour
{
    PhysicsBody m_PhysicsBody;
    int m_OwnerKey;
    
    void OnEnable()
    {
        // ... (code to create body and set ownership as above) ...
        // Assign my component script as the owner (optional) and return an "owner key" (integer).
        // Once ownership is set, it cannot be changed!
        m_OwnerKey = m_PhysicsBody.SetOwner(this);
    }
    
    void OnDisable()
    {
      // Destroy the body using the owner key. This will succeed.
      m_PhysicsBody.Destroy(m_OwnerKey);        
    }
}
```

As a component author, you might choose *not* to set ownership if you want others to be able to destroy your physics objects. Typically, components want to control the lifetime of their own physics objects.

The ownership feature is designed to be a deterrent, making it harder to accidentally break components, rather than providing cryptographic-level security. A good example is Unity's default `PhysicsWorld`, which is owned by Unity and cannot be destroyed by user code.
