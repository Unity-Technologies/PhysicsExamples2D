# Using the ShadowCaster2D with the Low-Level Physics

This snippet shows how you can create `ShadowCaster2D` providers that can interact with the low-level physics.

There are three examples scenes, simply load each and hit "Play".
- `ShadowCaster_PerSceneShape` - Will cast shadows for any `SceneShape` on the same `GameObject` as the `ShadowCaster2D`.
  - Look at the `SceneShapeShadowProvider` script for the implementation.  
- `ShadowCaster_PerSceneBodu` - Will cast shadows for all `PhysicsShape` created by any `SceneBody` on the same `GameObject` as the `ShadowCaster2D`.
  - Look at the `SceneBodyShadowProvider` script for the implementation.
- `ShadowCaster_Region` - Will cast shadows for all `PhysicsShape` in the specified region using the specified `PhysicsQuery.QueryFilter` (defined by a `SceneShadowRegion` component on the same `GameObject` as the `ShadowCaster2D`).
  - Look at the `SceneShapeShadowProvider` and `SceneShadowRegion` scripts for the implementation.

---

![ShadowCasterSnippet](ShadowCasterSnippet.png)