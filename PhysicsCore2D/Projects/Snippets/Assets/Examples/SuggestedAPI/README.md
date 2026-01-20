# Miscellaneous Suggestions for API

This snippet contains implementations for suggestions to the API.
When or if the API is implemented, these suggestions will be removed and noted here.

## Suggestions
- [Discussions](https://discussions.unity.com/t/lowlevel-physics-question/1696693/2) - PhysicsQuery.ShapeDistance extended to include a span of shapes and other utility.
  - See `PhysicsAPIExtensions.Shape-Distance`
- [Discussions](https://discussions.unity.com/t/low-level-2d-physics-setting-struct-properties-is-cumbersome) - Returning physics objects when using auto-properties results in the compiler error (`CS1612`) indicating that you cannot modify the return value of 'X' because it is not a variable. These extensions allow that.
  - See: `PhysicsAPIExtensions.Get()` 