# Physics Examples

This repository contains test projects, examples, and a test package for Unity's 2D physics, spanning two API generations:

- [Sandbox Project](Sandbox/README.md) — interactive playground scenes that use the physics API directly
- [Workshop Project](Workshop/README.md) — interactive workshop scenes that use the physics components
- [Examples Package](Packages/com.unity.2d.physics.examples/README.md) - unsupported experimental example components
- [OldPhysics2D](OldPhysics2D/README.md) — examples that use the older "Physics2D" component API.

---
- [2D Physics Core Manual](https://docs.unity3d.com/6000.7/Documentation/Manual/2d-physics-api/2d-physics-api-landing.html)
- [2D Physics Package Documentation](https://docs.unity3d.com/Packages/com.unity.2d.physics@latest/)
- [Dev Videos](https://www.youtube.com/c/melvmay/videos)
- [Twitter](https://x.com/melvmay)
- [Unity Discussions](https://discussions.unity.com/u/melvmay)

## Branch Names

Each branch represents a specific version of Unity. As features are added in a public release, those features should be represented in that branch and future Unity version branches i.e. branch names such as "2022", "6000.3" (etc) exist.

- `master` represents the Unity version currently in mid/late beta or final release.
- `unsupported/xxx` represents versions that are in early releases and not yet stable.
- `6000.x` (etc) branches represent released Unity versions, kept as live branches while they are still supported.

### Archived Versions

Older Unity version branches that are far out of support are not kept as live branches. They are archived as annotated tags named `archive/<version>` (for example `archive/2019`, `archive/6000.1`). A tag preserves the exact final state of that version permanently while keeping the branch list focused on supported versions.

To restore an archived version into a working branch:

```bash
git branch 2019 archive/2019
```

---
## Acknowledgements

Thanks to Erin Catto (the creator of Box2D v3), upon which significant portions of the "Sandbox" and "Workshop" projects are based.

https://github.com/erincatto/box2d

