# PhysicsCore2D Examples

A collection of demo scenes and snippets for Unity's **PhysicsCore2D** API (namespace `Unity.U2D.Physics`). The Unity version this branch targets is encoded in the branch name (e.g. `6000.5`).

## Use the bundled physicscore2d skills

The `.claude/skills/unity-physicscore2d*` directory contains the canonical reference for PhysicsCore2D API signatures, patterns, and conventions. When doing any PhysicsCore2D work in this repo:

- **API lookups** ("does X exist?", "what's the signature of Y?") → consult the matching `unity-physicscore2d-*-api` skill. These are auto-generated from Unity's IntelliSense XML and are the source of truth for member names and signatures.
- **Implementation questions** ("how should I X?", "which type do I pick?") → consult the matching topic skill (e.g. `unity-physicscore2d-bodies`, `unity-physicscore2d-joints`, `unity-physicscore2d-queries`, `unity-physicscore2d-shapes-advanced`).
- **Orientation / first-time** → start with the `unity-physicscore2d` umbrella skill, which also contains the API Verification Protocol for any member not in the auto-generated `*-api` skills.

Never guess at PhysicsCore2D API surface — always verify against a skill or the editor's IntelliSense.

## Never use the legacy Physics2D component system

This project is **PhysicsCore2D only**. Do not use `Rigidbody2D`, `Collider2D`, or any other `UnityEngine` component-based Physics2D types here — those examples live in the sibling `LegacyPhysicsExamples2D/` repo. The two systems coexist in Unity but are unrelated.

## Project layout

- `Projects/Primer/` — focused single-concept examples
- `Projects/Sandbox/` — interactive playground scenes. To add or modify a Sandbox example, follow [`Projects/Sandbox/AUTHORING_EXAMPLES.md`](Projects/Sandbox/AUTHORING_EXAMPLES.md) — the authoritative recipe. Examples derive from `SandboxExampleBehaviour`, are tagged `[ExampleScene(category, description)]`, build their option controls in code via the base-class `AddSlider`/`AddToggle`/`AddEnum`/… helpers (no per-example uxml), and self-register via `Tools/2D/Physics/Rebuild Sandbox Registry`.
- `Projects/Snippets/` — minimal API usage demos
- `Packages/` — Unity package dependencies

When adding a new scene, look at nearby existing scenes for the established pattern (component authoring, lifecycle, naming) before designing a new one.
