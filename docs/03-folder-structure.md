# 📁 Cấu trúc thư mục chuẩn — Abyss Frontier

> ← [Về trang chính](../GUIDES.md)

> ⚠️ Cấu trúc này phản ánh đúng thực tế project. **Luôn đặt file đúng thư mục!**

---

## Cây thư mục đầy đủ

```
Abyss-Frontier/
├── Assets/
│   ├── Animations/                 ← Animator Controller + Animation Clips
│   │   ├── Player/
│   │   │   ├── Player_Idle.anim
│   │   │   ├── Player_Walk.anim
│   │   │   ├── Player_Attack.anim
│   │   │   └── PlayerAnimator.controller
│   │   ├── Enemy/
│   │   │   ├── Slime_Walk.anim
│   │   │   ├── Skeleton_Attack.anim
│   │   │   └── Boss_Idle.anim
│   │   └── UI/
│   │
│   ├── Audio/                      ← Toàn bộ âm thanh
│   │   ├── BGM/                    ← Nhạc nền (background music)
│   │   │   ├── bgm_dungeon_01.ogg
│   │   │   └── bgm_surface.ogg
│   │   └── SFX/                    ← Hiệu ứng âm thanh
│   │       ├── sfx_sword_swing.ogg
│   │       ├── sfx_enemy_hit.ogg
│   │       └── sfx_build.ogg
│   │
│   ├── Prefabs/                    ← Prefab tổ chức theo hệ thống
│   │   ├── Player/
│   │   │   └── Player.prefab
│   │   ├── Enemies/
│   │   │   ├── Slime.prefab
│   │   │   ├── Skeleton.prefab
│   │   │   └── Boss_Floor1.prefab
│   │   ├── Buildings/
│   │   │   ├── House.prefab
│   │   │   ├── FarmPlot.prefab
│   │   │   └── Forge.prefab
│   │   ├── Items/
│   │   │   ├── Gold.prefab
│   │   │   ├── Stone.prefab
│   │   │   └── Crystal.prefab
│   │   └── UI/
│   │       ├── HPBar.prefab
│   │       └── DialogueBox.prefab
│   │
│   ├── Scenes/                     ← Tất cả Scene (KHÔNG dùng SampleScene!)
│   │   ├── MainMenu.unity
│   │   ├── Surface.unity           ← Khu base building
│   │   ├── DungeonFloor1.unity
│   │   ├── DungeonFloor2.unity
│   │   └── DungeonFloor3.unity
│   │
│   ├── ScriptableObjects/          ← Data-driven design
│   │   ├── Enemies/
│   │   │   ├── SlimeData.asset
│   │   │   └── BossData.asset
│   │   ├── Buildings/
│   │   │   ├── HouseData.asset
│   │   │   └── ForgeData.asset
│   │   └── Items/
│   │       └── ItemDatabase.asset
│   │
│   ├── Scripts/                    ← C# scripts, tổ chức theo hệ thống
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerCombat.cs
│   │   │   └── PlayerHealth.cs
│   │   ├── Enemy/
│   │   │   ├── EnemyAI.cs
│   │   │   ├── EnemyHealth.cs
│   │   │   ├── EnemyAttack.cs
│   │   │   └── BossController.cs
│   │   ├── Dungeon/
│   │   │   ├── DungeonManager.cs
│   │   │   ├── RoomTrigger.cs
│   │   │   └── FloorManager.cs
│   │   ├── BaseBuilding/
│   │   │   ├── BuildSystem.cs
│   │   │   ├── BuildingData.cs
│   │   │   └── ResourceManager.cs
│   │   ├── Inventory/
│   │   │   └── InventorySystem.cs
│   │   ├── UI/
│   │   │   ├── UIManager.cs
│   │   │   ├── InventoryUI.cs
│   │   │   ├── BuildMenuUI.cs
│   │   │   └── DialogueUI.cs
│   │   └── Core/                   ← Các script dùng chung
│   │       ├── GameManager.cs
│   │       └── SaveSystem.cs
│   │
│   ├── Sprites/                    ← Toàn bộ ảnh pixel art (sprite sheet nguồn)
│   │   ├── Characters/
│   │   │   ├── Player/
│   │   │   │   ├── player_idle.png
│   │   │   │   └── player_attack.png
│   │   │   └── Enemies/
│   │   │       ├── slime_walk.png
│   │   │       └── boss_attack.png
│   │   ├── Environment/
│   │   │   ├── Dungeon/
│   │   │   │   ├── tileset_dungeon.png   ← Sprite sheet → cắt → tạo Tile asset
│   │   │   │   └── walls.png
│   │   │   └── Surface/
│   │   │       ├── tileset_grass.png
│   │   │       └── ruins.png
│   │   └── UI/
│   │       ├── hp_bar.png
│   │       ├── inventory_slot.png
│   │       └── buttons.png
│   │
│   ├── Tilemaps/                   ← Asset liên quan đến Tilemap
│   │   ├── Palettes/               ← Tile Palette (.prefab) dùng để vẽ trong editor
│   │   │   ├── DungeonPalette.prefab
│   │   │   └── SurfacePalette.prefab
│   │   ├── Tiles/                  ← Tile asset (.asset) — từng ô tile đơn lẻ
│   │   │   ├── Dungeon/
│   │   │   │   ├── tile_wall.asset
│   │   │   │   ├── tile_floor.asset
│   │   │   │   └── tile_door.asset
│   │   │   └── Surface/
│   │   │       ├── tile_grass.asset
│   │   │       └── tile_water.asset
│   │   └── RuleTiles/              ← Rule Tile (.asset) — tile tự ghép biên
│   │       └── rt_dungeon_wall.asset
│   │
│   ├── Settings/                   ← Render Pipeline, Input Actions
│   └── UI/                         ← UI Toolkit Document, USS, UXML (nếu dùng)
│
├── docs/                           ← Tài liệu nhóm (thư mục này!)
├── Packages/                       ← Unity Package Manager (không chỉnh tay)
├── ProjectSettings/                ← Cài đặt project (PHẢI commit)
├── .gitignore
├── .gitattributes
├── base.md                         ← Game Design Document
└── GUIDES.md                       ← Trang index tài liệu
```

---

## 💡 Lưu ý quan trọng

- Project **không dùng `_Game/`** — asset nằm thẳng trong `Assets/`
- Các folder cần **tạo thêm khi cần**: `ScriptableObjects/`, `Tilemaps/`, `Materials/`
- **Không tạo folder tùy tiện** — thảo luận với nhóm trước

---

## 🗺️ Tilemap — Giải thích chi tiết từng loại file

Tilemap trong Unity sinh ra **4 loại file khác nhau**, dễ nhầm lẫn:

| File | Đuôi | Nằm ở đâu | Giải thích |
|------|------|-----------|------------|
| **Sprite Sheet** | `.png` | `Sprites/Environment/` | Ảnh gốc chứa nhiều tile ghép lại. Cắt bằng Sprite Editor |
| **Tile Asset** | `.asset` | `Tilemaps/Tiles/` | Một ô tile đơn lẻ, tạo từ 1 sprite đã cắt |
| **Rule Tile** | `.asset` | `Tilemaps/RuleTiles/` | Tile thông minh — tự chọn sprite theo tile xung quanh |
| **Tile Palette** | `.prefab` | `Tilemaps/Palettes/` | Bảng tile dùng trong Editor để vẽ map (không phải GameObject trong game) |

### Quy trình tạo Tilemap từ đầu

```
1. Có tileset_dungeon.png  →  để vào  Sprites/Environment/Dungeon/
         ↓
2. Unity: chọn PNG → Sprite Mode: Multiple → Sprite Editor → Slice
         ↓
3. Kéo từng sprite đã cắt vào Tile Palette → Unity tự tạo Tile Asset (.asset)
         ↓  Unity hỏi lưu Tile Asset ở đâu
4. Chỉ định lưu vào:  Tilemaps/Tiles/Dungeon/
         ↓
5. Dùng Tile Palette để vẽ lên Tilemap Grid trong Scene
```

### ⚠️ Lỗi hay gặp với Tilemap & Git

**Tile Palette là `.prefab` → phải commit!**
```bash
git add Assets/Tilemaps/Palettes/DungeonPalette.prefab
git add Assets/Tilemaps/Palettes/DungeonPalette.prefab.meta
```

**Tile Asset là `.asset` → phải commit!**
```bash
git add Assets/Tilemaps/Tiles/Dungeon/
# Nhớ add cả thư mục để bao gồm .meta của từng .asset
```

**KHÔNG để Tile Palette trong `Prefabs/`** — Tile Palette là công cụ vẽ của editor, không phải GameObject trong game.

**Mỗi người vẽ map trên Scene khác nhau** — tilemap data nằm trong file `.unity`, rất dễ conflict khi 2 người cùng sửa.
