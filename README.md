# Physics Examples

This repository contains test projects, examples, and a test package for Unity's 2D physics, spanning two API generations:

- [Sandbox Project](Sandbox/README.md) — interactive playground scenes
- [Primer Project](Primer/README.md) — focused single-concept examples
- [Examples Package](Packages/com.unity.2d.physics.examples/README.md) - unsupported experimental example components
- [OldPhysics2D](OldPhysics2D/README.md) — examples that use the older "Physics2D" component API.
- `.claude/` contains skills you can copy into your own `.claude` folder to better work with PhysicsCore2D.

---
- [Dev Videos](https://www.youtube.com/c/melvmay/videos)
- [Twitter](https://x.com/melvmay)
- [Unity Discussions](https://discussions.unity.com/u/melvmay)

## Branch Names

Each branch represents a specific version of Unity. As features are added in a public release, those features should be represented in that branch and future Unity version branches i.e. branch names such as "2022", "6000.3" (etc) exist.

- `master` represents the current latest release of Unity, updated only when new final releases have been public for a while.
- `unsupported/xxx` represents versions that are currently in alpha/beta release state.

### Archived Versions

Older Unity version branches are not kept as live branches. They are archived as annotated tags named `archive/<version>` (for example `archive/2019`, `archive/6000.1`). A tag preserves the exact final state of that version permanently while keeping the branch list focused on supported versions.

To archive a branch, tag its tip, push the tag, then delete the branch:

```bash
git tag -a archive/2019 origin/2019 -m "Archive 2019 branch"
git push origin archive/2019
git push origin --delete 2019
```

To restore an archived version into a working branch:

```bash
git branch 2019 archive/2019
```

---
## Acknowledgements

Thanks to Erin Catto (the creator of Box2D v3), upon which significant portions of the "Sandbox" project are based.

https://github.com/erincatto/box2d

