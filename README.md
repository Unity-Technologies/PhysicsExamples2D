# Physics Examples

This repository contains test projects, examples, and a test package for Unity's 2D physics, spanning two API generations:

- **PhysicsCore2D** — examples for the newer 2D physics core API (`Unity.U2D.Physics`), located at the repository root:
  - [Sandbox Project](Sandbox/README.md) — interactive playground scenes
  - [Primer Project](Primer/README.md) — focused single-concept examples
  - [Snippets Project](Snippets/README.md) — minimal API usage demos
- **OldPhysics2D** — examples that use the older "Physics2D" component API.

`Packages/com.unity.2d.physics.extras` is a shared local package referenced by the PhysicsCore2D projects.

`.claude/` contains skills you can copy into your own `.claude` folder to better work with PhysicsCore2D in Unity 6.5+.

- [Dev Videos](https://www.youtube.com/c/melvmay/videos)

---
Acknowledgement and thanks to Erin Catto (the creator of Box2D v3), upon which significant portions of the "Sandbox" project are based.

https://github.com/erincatto/box2d

---
## Branch Names

Each branch represents a specific version of Unity. As features are added in a public release, those features should be represented in that branch and future Unity version branches i.e. branch names of "2019", "2020", "2021" (etc) exist.

- `master` represents the current latest release of Unity, updated only when new final releases have been public for a while.
- `unsupported/xxx` represents versions that are currently in alpha/beta release state.

---

Twitter: https://twitter.com/melvmay

Unity Discussions: [https://forum.unity.com/members/melvmay.287484/](https://discussions.unity.com/u/melvmay)
