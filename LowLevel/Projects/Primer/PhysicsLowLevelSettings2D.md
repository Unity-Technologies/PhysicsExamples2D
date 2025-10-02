# Project Settings

The physics system maintains a set of project-wide settings that are applied both in the Editor and at runtime.
These project settings are often useful across multiple projects, so the physics system uses an asset-based approach for configuring and assigning them.
The asset type used to store these settings is [PhysicsLowLevelSettings2D](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LowLevelPhysics2D.PhysicsLowLevelSettings2D.html), which is notably the only `class` type in the API.

## Create and Configure Settings

**Create the Asset**

Go to `Assets > Create > 2D > Physics LowLevel Settings` in the Unity Editor:

![Physics LowLevel Settings Asset](Images/LowLevelPhysicsSettings2D.png)

**Assign the Asset**

Drag the created `Physics LowLevel Settings` asset into the property in the `Low Level` tab located in `Project Settings > Physics 2D`:

![Project Settings / Physics 2D / Low Level](Images/ProjectSettingsPhysics2D.png)

**Edit the Settings**

Select the asset in the `Project` window to edit its properties directly in the `Inspector` window.
Any changes to this asset will take effect immediately if it is assigned to the project settings:

![Physics LowLevel Settings Inspector](Images/LowLevelPhysicsSettings2D-Inspector.png)

## Layers

The settings asset lets you configure dedicated physics layer names and provides an option to enable or disable the use of these layers. For more details, see [Physics Layers](PhysicsLayers.md).

## Default Definitions

As outlined in [Definitions](Definitions.md), many physics objects rely on definitions to specify their initial state before creation.
Each definition comes with a configurable set of default values, which are stored in the settings asset.

While definitions are typically used in user scripts, Unity itself also uses a `PhysicsWorldDefinition` when creating the default `PhysicsWorld` (see [PhysicsWorld](PhysicsWorld.md)).
This default definition determines the initial state of the default `PhysicsWorld`.

It's important to note that, as with all physics objects, modifying a definition will not affect any objects that have already been created.
The same principle applies to the `PhysicsWorldDefinition`—changes to it will not impact the existing default `PhysicsWorld`.
Any updates will only take effect after restarting the Editor or re-entering Play mode.

**Caution:** Be mindful when adjusting default definitions.
Your code may depend on certain default values (for example, `PhysicsBody.bodyType` being set to `Static`).
If you change a default, any code that doesn’t explicitly set that property will now use your new default value, potentially resulting in unexpected behavior.

## Globals

A variety of settings are available that directly control the global behavior of the physics system, as well as options specific to player builds. These global settings are generally configured in Editor mode and are not intended to be changed dynamically at runtime.