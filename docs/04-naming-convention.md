# 🏷️ Quy tắc đặt tên file & folder

> ← [Về trang chính](../GUIDES.md)

> Đặt tên đúng ngay từ đầu giúp tránh lỗi đường dẫn, dễ tìm file, và tránh conflict `.meta`.

---

## Folders

- Dùng **PascalCase**: `Scripts/`, `Prefabs/`, `ScriptableObjects/`
- **Không dùng dấu cách** (tránh lỗi đường dẫn trên các OS khác nhau)
- Nhóm theo **hệ thống chức năng**, không theo loại file

---

## Scripts (.cs)

- Dùng **PascalCase**: `PlayerController.cs`, `EnemyAI.cs`
- Tên phải mô tả rõ chức năng

| ✅ Đúng | ❌ Sai |
|---------|--------|
| `PlayerController.cs` | `player.cs` |
| `BossController.cs` | `Boss1Script.cs` |
| `ResourceManager.cs` | `Manager.cs` |
| `InventorySystem.cs` | `Inventory_new.cs` |

---

## Sprites / Textures

- Dùng **snake_case**: `player_idle.png`, `tileset_dungeon.png`
- Thêm tiền tố theo loại:

| Tiền tố | Dùng cho | Ví dụ |
|---------|---------|-------|
| `char_` | Character sprite | `char_player_idle.png` |
| `env_` | Environment (tileset, background) | `env_dungeon_wall.png` |
| `ui_` | UI element | `ui_hp_bar.png`, `ui_button.png` |
| `item_` | Item icon | `item_crystal.png` |
| `fx_` | Effect sprite | `fx_slash.png` |

---

## Audio

- Dùng **snake_case**
- **Dùng `.ogg` thay `.wav`** — nhỏ hơn ~10x, tiết kiệm Git LFS quota

| Tiền tố | Dùng cho | Ví dụ |
|---------|---------|-------|
| `bgm_` | Background Music | `bgm_dungeon_01.ogg` |
| `sfx_` | Sound Effect | `sfx_sword_swing.ogg`, `sfx_enemy_hit.ogg` |

---

## Scenes

- Dùng **PascalCase**
- Tên phải mô tả nội dung scene

| ✅ Đúng | ❌ Sai |
|---------|--------|
| `DungeonFloor1.unity` | `SampleScene.unity` |
| `MainMenu.unity` | `Test.unity` |
| `Surface.unity` | `Scene1.unity` |
| `DungeonFloor2.unity` | `NewScene.unity` |

---

## Prefabs

- Dùng **PascalCase**
- **Không thêm "Prefab" vào tên**

| ✅ Đúng | ❌ Sai |
|---------|--------|
| `Slime.prefab` | `SlimePrefab.prefab` |
| `HPBar.prefab` | `HP_bar_prefab.prefab` |
| `DialogueBox.prefab` | `Dialogue_Box.prefab` |

---

## ScriptableObjects

- Dùng **PascalCase** + hậu tố `Data` hoặc `Config`
- Đuôi file là `.asset`

| ✅ Đúng | ❌ Sai |
|---------|--------|
| `SlimeData.asset` | `slime.asset` |
| `GameConfig.asset` | `Config1.asset` |
| `ItemDatabase.asset` | `Items.asset` |

---

## Animation

- **Animator Controller**: `PascalCase` + `Animator` → `PlayerAnimator.controller`
- **Animation Clip**: `PascalCase_ActionName` → `Player_Idle.anim`, `Slime_Walk.anim`

---

## Bảng tóm tắt nhanh

| Loại | Convention | Ví dụ |
|------|-----------|-------|
| Folder | PascalCase | `Scripts/`, `Enemies/` |
| C# Script | PascalCase | `PlayerController.cs` |
| Scene | PascalCase | `DungeonFloor1.unity` |
| Prefab | PascalCase | `Slime.prefab` |
| ScriptableObject | PascalCase + Data/Config | `SlimeData.asset` |
| Sprite / Texture | snake_case + tiền tố | `char_player_idle.png` |
| Audio | snake_case + tiền tố | `sfx_sword_swing.ogg` |
| Animation Clip | PascalCase_Action | `Player_Attack.anim` |
| Animator Controller | PascalCaseAnimator | `PlayerAnimator.controller` |
