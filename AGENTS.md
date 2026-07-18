# Abyss Frontier - Agent Instructions

## Project Overview
- **Game Engine**: Unity 6 (6000.3.15f1)
- **Genre**: 2D Top-down Dungeon Crawler / Action RPG (Singleplayer + optional Multiplayer, PC)
- **Primary Design Document**: `base.md` at the repository root (if present).
- **Backlog**: [`backlog.html`](backlog.html) — 85 tasks (T-01–T-94, thiếu T-52), synced with codebase 2026-07-14. Legacy Google Sheets export: `backlog_legacy_export.html`. Regenerate via `tools/generate_backlog_html.py`.

## Game Context
- **Setting**: Abandoned Mine / Cursed Dungeon — 5 tầng ngục tối (`floor1`–`floor5`). Code references `floor6` unlock but scene not yet created.
- **Core Loop**: Vào dungeon → Chiến đấu quái → Nhận EXP → Về Hub → Nâng cấp chỉ số → Xuống tầng sâu hơn.
- **No base building** — game tập trung hoàn toàn vào dungeon crawling và character progression.
- **Enemy roster**: Plant (Floor 1), Slime (Floor 2–3), Orc (Floor 3–4), Vampire (Floor 4), Boss/Mimic (Floor 4–5). Meep variants as additional enemy types.
- **Enemies have 3 levels** each — via `EnemyStats` ScriptableObject.
- **Enemies only respawn when player rests at the Hub** — never auto-respawn in-dungeon. *Known gap*: `EnemyHealth.respawnOnDeath` defaults to `true` (fix tracked as T-76). **Wave spawn in-dungeon** (T-81) is a separate mechanism: clear all enemies in a wave before the next wave spawns.
- **EXP is lost on death** — reset to 0 before respawn; never carry over.
- **Stat Screen (attribute allocation) is only accessible at the Hub** (`Base_Camp`).

## Extended Features (beyond original scope)
- **Multiplayer**: Photon Fusion — Single / Host / Client modes (`Assets/Scripts/Network/`).
- **Cloud Save**: Firebase + Dummy PlayerPrefs fallback — client integration (T-67, Đức Hải); Firestore schema & security rules (T-84/T-86, Duy Phúc).
- **Authentication**: Email login/register UI (T-68, Trung Nguyên); Google Sign-In planned (T-83).
- **Character Select**: Multiple playable characters via `CharacterData` ScriptableObjects.
- **Menu Flow**: Map → Single/Multi → Host/Join → Character → Launch (`MenuFlowController`).
- **Runtime UI**: HUD, stat screen, death/boss screens auto-created via `GameplayUIBootstrap`.

## Technical Stack & Packages
- **Input System**: Uses the new `UnityEngine.InputSystem`.
  - *Quirk*: Use `On[Action](InputValue value)` pattern (via `PlayerInput` component). Do not use the legacy `Input` class.
- **Rendering**: Universal Render Pipeline (URP) with 2D tooling.
- **Camera**: Cinemachine 3 (`com.unity.cinemachine`). Ensure compatibility with v3 APIs (which differ from v2).
- **Physics**: Uses Unity 2D Physics (`Rigidbody2D`, `Collider2D`).
  - *Quirk*: Unity 6 deprecated `rb.velocity`. You **must** use `rb.linearVelocity` or `rb.angularVelocity`.
- **Networking**: Photon Fusion 2 (`Assets/Photon/Fusion/`).
- **Cloud**: Firebase Auth + Firestore (behind `FB_SDK` define) with Dummy fallback.

## Code Conventions
- Scripts live under `Assets/Scripts/` organized by domain:
  - `Player/` — PlayerController, PlayerCombat, PlayerStats, PlayerHealth, PlayerDash, PlayerInteractor
  - `Enemy/` — EnemyAI, EnemyHealth, EnemyStats, BossController, MimicBossController
  - `Character/` — CharacterMotor, CharacterAnimationHandler
  - `Animation/` — HeroAnimatorDriver, StandardAnimatorDriver, SpumAnimatorDriver, PinkMeepAnimatorDriver
  - `Combat/` — KnockbackHandler (*HitstopManager not yet implemented*)
  - `World/` — ExpOrb, ExpDropSpawner
  - `Base_camp/` — Base_Camp (Hub rest + stat screen)
  - `Door/` — DoorController, InteractableLever
  - `Object/` — PuzzleManager, PuzzleSwitch, Torch, InteractableTrigger, WaveSpawnManager
  - `Light/` — FloorTorchManager, TorchFlicker
  - `Camera/` — CinemachinePlayerTargetBinder
  - `Audio/` — AudioManager, GameAudioLibrary
  - `Menu/` — MainMenu, MenuFlowController, LevelSelectUI, CharacterSelectUI, PauseManager
  - `UI/` — GameplayHUDController, StatScreenUI, DeathScreenUI, BossVictoryUI, AuthenticationUIController
  - `Save/` — SaveManager, GameSaveData
  - `Cloud/` — CloudServiceManager, FirebaseCloudSave, DummyCloudSave
  - `Network/` — GameLauncher, NetworkPlayer, PlayerSpawner, InputHandler
- Use `[SerializeField] private` for inspector-exposed variables instead of `public`.
- Favor `Awake()` for component caching and `Start()` for external initialization.
- Uses 2D directional animation blending (`moveX`, `moveY`, `isWalk`).

## Combat Rules
- **Hit detection**: Inline in `PlayerCombat`, `EnemyAI`, `BossController` (no separate HitboxController component).
- **Knockback**: Implemented via `KnockbackHandler` — entities are pushed back on hit.
- **Hitstop**: Planned via `Time.timeScale` or animator speed — **not yet implemented** (T-16 partial).
- **Stamina**: Shared pool for both Sprint (hold Shift) and Dodge. Regenerates over time.

## Scenes
| Scene | Purpose |
|-------|---------|
| `floor1`–`floor5` | Dungeon floors |
| `Scene_Menu` | Main menu + level select flow |
| `Authenticaion` | Login/signup UI |
| `Scene-Server` | Fusion network bootstrap |

## Testing & Workflow
- EditMode tests exist: `Assets/Tests/EditMode/Editor/StatScreenUITests.cs` (T-80).
- Primary validation: in-editor play mode testing.
- Backlog update script: `tools/update_backlog.py` (re-run after major feature changes).

## Team Assignment
See [docs/08-team-assignment.md](docs/08-team-assignment.md) for scene ownership and member roles (Duy Phúc, Trung Nguyên, Bảo Nguyên, Khải Toàn, Đức Hải).
