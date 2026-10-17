# 2D Radiance Cascades (Experimental)

> **This is an experiment. It is not an official Unity package and is not supported by Unity.**
> It can change or disappear without notice, and nothing in it should be relied on for production work.

Screen-space global illumination for the URP 2D Renderer, built on radiance cascades.
Sprites act as light emitters, and a procedurally generated background is the surface that receives the light.
The scene is revealed by whatever is currently emitting, rather than lit by placed lights.

## Requirements

- Unity 6000.7
- Universal Render Pipeline 17.7 with the 2D Renderer

## Installing

The package is self-contained: this folder is everything, including its license and attribution.

- To embed it, copy the `com.unity.2d.radiancecascades` folder into your project's `Packages/` folder.
  Unity picks up an embedded package on the next refresh with no manifest entry, which is how the NeonShooter project uses it.
- To reference it from elsewhere on disk, use the Package Manager's **Add package from disk** and pick this folder's `package.json`,
  or add `"com.unity.2d.radiancecascades": "file:<path to this folder>"` to your `Packages/manifest.json`.

Keep `LICENSE.md` and `ATTRIBUTION.md` with the folder.
The shaders carry third-party work under the terms recorded there.

## Setup

All of the types are in the `Unity.U2D.RadianceCascades` namespace.

1. Add `RadianceCascades2DGIFeature` to the 2D Renderer asset.
   Assign the included `SceneCaptureInstanced` material to its scene capture material source.
   A player build only keeps a shader's instanced form when a material asset asks for it, so without this the sprites can reach the lighting as nothing in a build.
   The shaders are found by name when left unassigned.
2. Add a `SpriteRenderSystem` to the scene and assign a sprite material as its source material.
   The material's shader must read the per-instance `objectToWorld` and `spriteColor` data that `Graphics.RenderSpriteInstanced` supplies.
   URP's own `Universal Render Pipeline/2D/Sprite-Unlit-Default` shader does, so a material using it with **Enable GPU Instancing** ticked is enough; that is what NeonShooter uses.
3. Register each sprite that should emit or block light with `SpriteRenderSystem.instance.Add(transform, sprite, scale, color)`, and pass the returned handle to `Remove` when it is finished.
   The lighting reads only the sprites registered here, not the ones drawn by `SpriteRenderer` components.
   A color brighter than one, from an HDR color, emits more light.

The feature's **Sorting Layer Name** field (initially `Default`) sets where it works in the frame: the lit background is drawn before that layer's sprites, and the registered sprites are captured after them.
The background covers whatever the camera or scene had behind that layer, so the feature owns the backdrop; its look is set by the feature's background fields.

A scene can carry its own lighting settings by adding a `GILightingOverride`, which replaces the feature's settings while that scene is loaded.
Its values have setters, so a menu or a script can tune the lighting at runtime.

Game code can dim the lighting during bright effects by setting `RadianceCascades2DGIFeature.exposureOffset`, in stops, and setting it back to zero afterward.

## Sample

The package ships a **Rendering Lab** sample: a controlled scene with one bright emitter and a few solid occluders, nothing moving, so a shadow is clearly present or clearly absent.
Import it from the package's Samples section in the Package Manager window, then open `CascadeSwing` and enter play mode.
The lab's objects are ordinary children, so they can be dragged around in the scene view while playing to watch the shadows follow.
Adding a `GIProbe` next to the `GILab` switches on the feature's irradiance capture, reads the lit result back, and reports what it carries.

The NeonShooter project in this repository is the reference use in a game.
`Bullet` and `Enemy` register and remove their sprites with the `SpriteRenderSystem`, `ScreenFxSettings` dims the lighting through `exposureOffset` during the panic bomb, and `GILightingSettings` puts the scene's `GILightingOverride` on the options menu.

## Limitations

- The feature assumes a single rendering camera, because the copy the lighting reads is shared by every camera the renderer serves.
- An emitter smaller than about four screen pixels can flicker as it moves, because of the reduced resolution the lighting works at.
