# Abyss Frontier - Agent Instructions

## Project Overview
- **Game Engine**: Unity 6 (6000.3.15f1)
- **Genre**: 2D Top-down Dungeon Crawler / Action RPG (Singleplayer, PC)
- **Primary Design Document**: `base.md` at the repository root. Always reference this file for mechanics, scope, and story before implementing game logic.

## Game Context
- **Setting**: Abandoned Mine / Cursed Dungeon — 5 tầng ngục tối.
- **Core Loop**: Vào dungeon → Chiến đấu quái → Nhận EXP → Về Hub → Nâng cấp chỉ số → Xuống tầng sâu hơn.
- **No base building** — game tập trung hoàn toàn vào dungeon crawling và character progression.
- **Enemy roster**: Plant (Floor 1), Slime (Floor 2–3), Orc (Floor 3–4), Vampire (Floor 4), Boss (Floor 5).
- **Enemies have 3 levels** each — implement via ScriptableObject or `enum Level { Level1, Level2, Level3 }`.
- **Enemies only respawn** when player rests at the Hub — never auto-respawn.
- **EXP is lost on death** — reset to 0 before respawn; never carry over.
- **Stat Screen (attribute allocation) is only accessible at the Hub**.

## Technical Stack & Packages
- **Input System**: Uses the new `UnityEngine.InputSystem`.
  - *Quirk*: Use `On[Action](InputValue value)` pattern (via `PlayerInput` component). Do not use the legacy `Input` class.
- **Rendering**: Universal Render Pipeline (URP) with 2D tooling.
- **Camera**: Cinemachine 3 (`com.unity.cinemachine`). Ensure compatibility with v3 APIs (which differ from v2).
- **Physics**: Uses Unity 2D Physics (`Rigidbody2D`, `Collider2D`).
  - *Quirk*: Unity 6 deprecated `rb.velocity`. You **must** use `rb.linearVelocity` or `rb.angularVelocity`.

## Code Conventions
- Scripts live under `Assets/Scripts/` organized by domain:
  - `Player/` — PlayerController, PlayerCombat, PlayerStats, PlayerUI
  - `Enemies/` — EnemyBase, EnemyAI, EnemyAttack, EnemyHealth, BossController
  - `Combat/` — HitboxController, KnockbackHandler, HitstopManager
  - `World/` — TrapBase, Checkpoint, PuzzleDoor
  - `Hub/` — HubManager, Blacksmith
  - `UI/` — MainMenuUI, HUDController, StatScreenUI, DeathScreenUI, BossVictoryUI
- Use `[SerializeField] private` for inspector-exposed variables instead of `public`.
- Favor `Awake()` for component caching and `Start()` for external initialization.
- Uses 2D directional animation blending (`moveX`, `moveY`, `isWalk`).

## Combat Rules
- **Hitbox / Hurtbox**: Every entity (player, enemy) has separate attack hitbox and damage hurtbox.
- **Knockback**: Entities are pushed back on hit.
- **Hitstop**: Implement via `Time.timeScale` (or animator speed) — short freeze on hit connection. Do NOT pause the game fully.
- **Stamina**: Shared pool for both Sprint (hold Shift) and Dodge. Regenerates over time.

## Testing & Workflow
- *Note*: Currently no automated test suites configured (`com.unity.test-framework` is installed but no `Tests` folder exists). Rely on in-editor play mode testing.
- Changes involving new features must align with the **Scope Demo** section in `base.md`.
