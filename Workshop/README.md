# Physics Core 2D Workshop

This project contains examples that are built with the Physics Pose, Physics Area and Physics Constraint components, so most of an example is authored in the scene rather than written in code.
Where no component does what an example needs, a small script fills the gap.

To run, load the main "Workshop" scene and press "Play". Alternately, build to a player and run.

To change the example that initially loads when you press "Play", select the `WorkshopManager` object in the "Workshop" scene and pick the example in its "Start Scene" field.

The [Sandbox](../Sandbox/README.md) project shows the same kind of examples written directly against the physics API.

---

## Start here

To try your own physics, open the **Custom > Empty** example, found in `Assets/Examples/Empty/`.
Its scene is blank, so add Physics Pose, Physics Area and Physics Constraint components to it and press "Play" to see the result.
Its `EmptyContents` and `EmptyOptions` scripts are empty too, with signposts showing where to add your own code and controls.
Look at the other examples, such as "Barrel", to see how they do it.

## How an example is laid out

Every example is a folder inside `Assets/Examples/`:

```
Assets/Examples/<Name>/<Name>.unity                 the example scene, loaded on top of the Workshop UI
Assets/Examples/<Name>/<Name>Info.asset             its name, category, description and the global settings it needs
Assets/Examples/<Name>/Scripts/<Name>Contents.cs    (optional) a script for anything no component can do
Assets/Examples/<Name>/Scripts/<Name>Options.cs     (optional) the example's own controls in the Workshop menu
```

The category an example is listed under comes from its info asset, not from where its folder is.
The example menu is generated from those info assets by **`Tools > 2D > Physics > Rebuild Workshop Registry`**.

## Adding your own

See [`AUTHORING_EXAMPLES.md`](./AUTHORING_EXAMPLES.md) for the full recipe, step by step.
