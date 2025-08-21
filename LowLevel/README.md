# Physics 2D LowLevel Examples

This repository contains test projects, examples and a test package for use with the LowLevel 2D Physics features in Unity.

To use the low-level features, you will require Unity 6000.3.0a5 or beyond.

---
Acknowledgement and thanks is made to Erin Catto, the creator of Box2D v3, from which significant portions of this "Sandbox" project is based upon.

Go try it here!
https://github.com/erincatto/box2d
---

### Physics 2D LowLevel Sandbox

This repository contains examples that can be built to a player.

You MUST first enable "Development Build" in the build options for the debug rendering to display anything as debug rendering is only available in the Editor and Development Players.

To run, load the main "Sandbox" scene and press "Play". Alternately, build to a player and run.

Each sample can be found in `Assets/Scenes/<Category>/<SampleName>` and all share the same configuration.

In each sample there is at least a single ".cs" file containing all the sample code. There's also a ".unity" scene and finally a ".uxml" for the sample UI.

The structure of the ".cs" script file is consistent for all samples and typically contains the following methods:

- **OnEnable()** - Grabs the Sandbox Manager and Camera Manipulator and initializes any sample fields. Also, other global physics or sandbox state may be overriden here. Here we always call "SetupOptions" and "SetupScene" (see below).
- **OnDisable()** - Typically destroys any temporary storage and restores any global physics or Sandbox state.
- **SetupOptions()** - This is always called from "OnEnable" and always configures the sample UI to dynamically control the sample (sample option in the lower-level UI).
- **SetupScene** - This is always called from "OnEnable" but is also called from the sample UI when pressing the "ResetScene" button on all samples. Here is where all the sample physics objects are created.
- **Update()** - Some samples use this to perform per-frame updates such as world drawing.
- **Misc** - Some samples have miscellaneous methods (sometimes used in "SetupScene") to spawn or control the physics objects. Some are hooked into the pre/post simulate to perform an action when the simulation is about to or has run.  

The "Scripts" folder typically contains utility scripts only used by various scenes such as spawning physics objects i.e. ragdolls, softbodies etc.
