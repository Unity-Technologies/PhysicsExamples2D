# PhysicsCore2D — project notes for Claude

## Skill system

This project ships a curated set of `unity-physicscore2d-*` Claude skills under `.claude/skills/`. They split into two layers:

### Layer 1 — `*-api` skills (auto-generated, **do not hand-edit**)

13 skills with the suffix `-api`, e.g. `unity-physicscore2d-bodies-api`, `unity-physicscore2d-joints-api`. Each is a complete reference dump for one type cluster (every type, property, field, method with XML doc summary), generated from Unity's IntelliSense XML.

**Source of truth:** the Unity editor's `UnityEngine.PhysicsCore2DModule.xml` (path is hard-coded in `.claude/api-reference/_generate.py` as `XML_PATH`).

**To regenerate** (after a Unity version bump, or any time you suspect the API has drifted):

```bash
python .claude/api-reference/_generate.py
```

This rewrites every `*-api/SKILL.md` and the cluster markdown under `.claude/api-reference/`. Output is deterministic and idempotent — re-running with no XML change produces no diff.

**To target a different Unity version**, edit `XML_PATH` at the top of `_generate.py` to point at that editor's `Data/Managed/UnityEngine/UnityEngine.PhysicsCore2DModule.xml`.

**Never hand-edit a `*-api/SKILL.md` file** — your changes will be overwritten on next regenerate. If you need to add narrative or examples for an API surface, add them to (or create) the corresponding **topic skill** instead.

### Layer 2 — Topic skills (hand-curated)

22 skills without the `-api` suffix, e.g. `unity-physicscore2d-joints` (patterns), `unity-physicscore2d-bodies` (lifecycle), `unity-physicscore2d-batching` (bulk patterns), etc. These cover patterns, recipes, decision rules, and worked examples — the *how* and *why*, complementing the *what* of the `-api` skills.

These are **hand-edited**. Each topic skill that has a corresponding `-api` skill includes a header pointer to it (e.g. `> For the full type/method API surface see unity-physicscore2d-bodies-api`).

The umbrella `unity-physicscore2d` skill (no suffix) is the orientation/routing skill — it lists both layers and explains when to invoke which.

## Distribution

- Project-local skills live at `PhysicsCore2D/.claude/skills/`.
- The same set is also installed at `~/.claude/skills/` (global) so the 2D team can use the skills outside this project.
- When you change skills here, you may want to mirror to global. The user typically does this manually with `cp -r` rather than via a script.

## Layout

```
PhysicsCore2D/
├── CLAUDE.md                          # this file
└── .claude/
    ├── settings.local.json
    ├── api-reference/                 # auto-generated reference docs + generator
    │   ├── _generate.py               # regeneration script
    │   ├── INDEX.md                   # type catalog + skill→files map
    │   └── *.md                       # 13 cluster files (world.md, joints.md, ...)
    └── skills/
        ├── unity-physicscore2d/                      # umbrella (orientation + routing)
        ├── unity-physicscore2d-<area>/               # 22 hand-curated topic skills
        └── unity-physicscore2d-<area>-api/           # 13 auto-generated API references
```

## Working with this project

- For PhysicsCore2D code questions, the umbrella skill `unity-physicscore2d` will route you to the right sub-skill. Always invoke the most specific skill — `*-api` for signature verification, topic skill for patterns.
- **Verify API signatures against the relevant `*-api` skill before writing code** (the umbrella skill has an explicit "never invent API" rule for this reason — Unity 6000.5 PhysicsCore2D pre-dates Claude's training data).
- The legacy `Physics2D` (component-based: `Rigidbody2D`, `BoxCollider2D`, etc.) is unrelated and incompatible. PhysicsCore2D lives in the `Unity.U2D.Physics` namespace and is not component-based.
