# Contact Filtering Extensions

This snippet shows how you can effecitvely filter contacts returned via [PhysicsBody.GetContacts](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsBody.GetContacts.html) or [PhysicsShaope.GetContacts](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsShape.GetContacts.html).

- Load the scene
- Select the "Example" `GameObject`.
- Select the "Filter Mode" in the `Get Contact Filtering` script.
- Hit "Play".
- Contacts that are filtered (returned) have their shape color changed.

Changing the enumeration simply select the user-define contact filter function.
The filter function can combine various other filter functions as required or simply be a lambda expression.

Filtering works simply be adding two [extension methods](ContactExtensions.cs) to extend any `NativeArray<PhysicsShape.Contact>` and allow filtering of the contacts via two methods:

- `Filter(filterFunction, [optional] shapeContext)` - Returns an enumerate set of `IEnumerable<PhysicsShape.Contact>`.
- `ToFilteredList(filterFunction, [optional] shapeContext, [optional] allocator = Allocator.Temp)` - Returns a list as a copy of filtered contacts as `NativeList<PhysicsShape.Contact>`.
