# 🎮 ABYSS FRONTIER — HƯỚNG DẪN TỔ CHỨC PROJECT & SỬ DỤNG GIT

> Dành cho nhóm mới học làm game với Unity. Đọc kỹ trước khi bắt đầu làm việc!

---

## 📋 MỤC LỤC

1. [Tại sao làm game với Git khó hơn làm web?](#1-tại-sao-làm-game-với-git-khó-hơn-làm-web)
2. [Cấu trúc thư mục chuẩn cho Abyss Frontier](#2-cấu-trúc-thư-mục-chuẩn-cho-abyss-frontier)
3. [Quy tắc đặt tên file & folder](#3-quy-tắc-đặt-tên-file--folder)
4. [Thiết lập Git cho Unity](#4-thiết-lập-git-cho-unity)
5. [Chiến lược Branching](#5-chiến-lược-branching)
6. [Quy trình làm việc hàng ngày](#6-quy-trình-làm-việc-hàng-ngày)
7. [Commit Message chuẩn](#7-commit-message-chuẩn)
8. [Xử lý Conflict trong Unity](#8-xử-lý-conflict-trong-unity)
9. [Git LFS — Quản lý file lớn](#9-git-lfs--quản-lý-file-lớn)
10. [Phân công công việc trong nhóm](#10-phân-công-công-việc-trong-nhóm)
11. [Các lỗi thường gặp & cách xử lý](#11-các-lỗi-thường-gặp--cách-xử-lý)
12. [Checklist trước khi Push](#12-checklist-trước-khi-push)

---

## 1. Tại sao làm game với Git khó hơn làm web?

Khi làm web, hầu hết file đều là **text** (HTML, CSS, JS) — Git merge rất dễ dàng.

Với Unity, bạn gặp thêm nhiều vấn đề:

| Vấn đề | Giải thích |
|--------|-----------|
| **File nhị phân (binary)** | Ảnh PNG, audio WAV, file FBX không thể merge — ai push trước thì thắng |
| **File `.unity` (Scene)** | Scene là YAML nhưng rất dài, conflict cực kỳ khó đọc và sửa |
| **File `.prefab`** | Tương tự Scene, dễ conflict khi nhiều người cùng sửa một Prefab |
| **Thư mục `Library/`** | Unity tự tạo ra, nặng hàng GB, **không được commit** |
| **File `.meta`** | Unity dùng để track GUID của asset — **phải commit**, nếu mất thì mất hết references |

### ✅ Nguyên tắc vàng:
- **Không bao giờ** xoá file `.meta` thủ công
- **Không bao giờ** commit thư mục `Library/`, `Temp/`, `Logs/`
- Mỗi người chịu trách nhiệm **một Scene hoặc một hệ thống** tại một thời điểm

---

## 2. Cấu trúc thư mục chuẩn cho Abyss Frontier

Dưới đây là cấu trúc **đã được cải thiện** dựa trên project hiện tại của nhóm:

```
Abyss-Frontier/
├── Assets/
│   ├── _Game/                          ← Toàn bộ asset do nhóm tự làm nằm ở đây
│   │   ├── Animations/                 ← Animator Controller + Animation Clips
│   │   │   ├── Player/
│   │   │   │   ├── Player_Idle.anim
│   │   │   │   ├── Player_Walk.anim
│   │   │   │   ├── Player_Attack.anim
│   │   │   │   └── PlayerAnimator.controller
│   │   │   ├── Enemy/
│   │   │   │   ├── Slime_Walk.anim
│   │   │   │   ├── Skeleton_Attack.anim
│   │   │   │   └── Boss_Idle.anim
│   │   │   └── UI/
│   │   │
│   │   ├── Audio/                      ← Toàn bộ âm thanh
│   │   │   ├── BGM/                    ← Nhạc nền (background music)
│   │   │   │   ├── bgm_dungeon_01.wav
│   │   │   │   └── bgm_surface.wav
│   │   │   └── SFX/                    ← Hiệu ứng âm thanh
│   │   │       ├── sfx_sword_swing.wav
│   │   │       ├── sfx_enemy_hit.wav
│   │   │       └── sfx_build.wav
│   │   │
│   │   ├── Materials/                  ← Material & Shader Graph
│   │   │
│   │   ├── Tilemaps/                   ← Toàn bộ asset liên quan đến Tilemap
│   │   │   ├── Palettes/               ← Tile Palette (.prefab) dùng để vẽ trong editor
│   │   │   │   ├── DungeonPalette.prefab
│   │   │   │   └── SurfacePalette.prefab
│   │   │   ├── Tiles/                  ← Tile asset (.asset) — từng ô tile
│   │   │   │   ├── Dungeon/
│   │   │   │   │   ├── tile_wall.asset
│   │   │   │   │   ├── tile_floor.asset
│   │   │   │   │   └── tile_door.asset
│   │   │   │   └── Surface/
│   │   │   │       ├── tile_grass.asset
│   │   │   │       ├── tile_ruin.asset
│   │   │   │       └── tile_water.asset
│   │   │   └── RuleTiles/              ← Rule Tile (.asset) — tile tự ghép biên (nếu dùng)
│   │   │       ├── rt_dungeon_wall.asset
│   │   │       └── rt_surface_grass.asset
│   │   │
│   │   ├── Prefabs/                    ← Prefab được tổ chức theo hệ thống
│   │   │   ├── Player/
│   │   │   │   └── Player.prefab
│   │   │   ├── Enemies/
│   │   │   │   ├── Slime.prefab
│   │   │   │   ├── Skeleton.prefab
│   │   │   │   └── Boss_Floor1.prefab
│   │   │   ├── Buildings/
│   │   │   │   ├── House.prefab
│   │   │   │   ├── FarmPlot.prefab
│   │   │   │   └── Forge.prefab
│   │   │   ├── Items/
│   │   │   │   ├── Gold.prefab
│   │   │   │   ├── Stone.prefab
│   │   │   │   └── Crystal.prefab
│   │   │   └── UI/
│   │   │       ├── HPBar.prefab
│   │   │       └── DialogueBox.prefab
│   │   │
│   │   ├── Scenes/                     ← Tất cả Scene
│   │   │   ├── MainMenu.unity
│   │   │   ├── Surface.unity           ← Khu base building
│   │   │   ├── DungeonFloor1.unity
│   │   │   ├── DungeonFloor2.unity
│   │   │   └── DungeonFloor3.unity
│   │   │
│   │   ├── ScriptableObjects/          ← Data-driven design (quan trọng!)
│   │   │   ├── Enemies/
│   │   │   │   ├── SlimeData.asset
│   │   │   │   └── BossData.asset
│   │   │   ├── Buildings/
│   │   │   │   ├── HouseData.asset
│   │   │   │   └── ForgeData.asset
│   │   │   └── Items/
│   │   │       └── ItemDatabase.asset
│   │   │
│   │   ├── Scripts/                    ← C# scripts, tổ chức theo hệ thống
│   │   │   ├── Player/
│   │   │   │   ├── PlayerController.cs
│   │   │   │   ├── PlayerCombat.cs
│   │   │   │   └── PlayerHealth.cs
│   │   │   ├── Enemy/
│   │   │   │   ├── EnemyAI.cs
│   │   │   │   ├── EnemyHealth.cs
│   │   │   │   ├── EnemyAttack.cs
│   │   │   │   └── BossController.cs
│   │   │   ├── Dungeon/
│   │   │   │   ├── DungeonManager.cs
│   │   │   │   ├── RoomTrigger.cs
│   │   │   │   └── FloorManager.cs
│   │   │   ├── BaseBuilding/
│   │   │   │   ├── BuildSystem.cs
│   │   │   │   ├── BuildingData.cs
│   │   │   │   └── ResourceManager.cs
│   │   │   ├── Inventory/
│   │   │   │   └── InventorySystem.cs
│   │   │   ├── UI/
│   │   │   │   ├── UIManager.cs
│   │   │   │   ├── InventoryUI.cs
│   │   │   │   ├── BuildMenuUI.cs
│   │   │   │   └── DialogueUI.cs
│   │   │   └── Core/                   ← Các script dùng chung
│   │   │       ├── GameManager.cs
│   │   │       └── SaveSystem.cs
│   │   │
│   │   └── Sprites/                    ← Toàn bộ ảnh pixel art (sprite sheet nguồn)
│   │       ├── Characters/
│   │       │   ├── Player/
│   │       │   │   ├── player_idle.png
│   │       │   │   └── player_attack.png
│   │       │   └── Enemies/
│   │       │       ├── slime_walk.png
│   │       │       └── boss_attack.png
│   │       ├── Environment/            ← Sprite sheet nguồn, KHÔNG phải tile asset
│   │       │   ├── Dungeon/
│   │       │   │   ├── tileset_dungeon.png   ← Sprite sheet → cắt ra → tạo Tile asset
│   │       │   │   └── walls.png
│   │       │   └── Surface/
│   │       │       ├── tileset_grass.png
│   │       │       └── ruins.png
│   │       └── UI/
│   │           ├── hp_bar.png
│   │           ├── inventory_slot.png
│   │           └── buttons.png
│   │
│   ├── Plugins/                        ← Third-party plugins (không sửa code ở đây)
│   └── Settings/                       ← Render Pipeline, Input Actions, v.v.
│
├── Packages/                           ← Unity Package Manager (không chỉnh tay)
├── ProjectSettings/                    ← Cài đặt project (PHẢI commit)
├── .gitignore
├── .gitattributes
├── base.md                             ← Game Design Document
└── GUIDES.md                           ← File này
```

### 💡 Tại sao dùng tiền tố `_Game/`?
- Tách biệt asset tự làm với asset từ Asset Store hoặc Plugins
- Trong Unity Editor, `_` giúp folder này hiện lên đầu danh sách
- Dễ dàng tìm kiếm và không nhầm lẫn giữa code của nhóm và code bên thứ ba

---

## 2.5 Tilemap — Giải thích chi tiết từng loại file

Tilemap trong Unity sinh ra **4 loại file khác nhau**, dễ nhầm lẫn:

| File | Đuôi | Nằm ở đâu | Giải thích |
|------|------|-----------|------------|
| **Sprite Sheet** | `.png` | `Sprites/Environment/` | Ảnh gốc chứa nhiều tile ghép lại. Cắt trong Unity bằng Sprite Editor |
| **Tile Asset** | `.asset` | `Tilemaps/Tiles/` | Một ô tile đơn lẻ, được tạo từ 1 sprite đã cắt |
| **Rule Tile** | `.asset` | `Tilemaps/RuleTiles/` | Tile thông minh — tự chọn sprite dựa theo tile xung quanh (tường tự ghép góc, v.v.) |
| **Tile Palette** | `.prefab` | `Tilemaps/Palettes/` | Bảng tile dùng trong Unity Editor để vẽ map, **không phải asset trong game** |

### Quy trình tạo Tilemap từ đầu

```
1. Có file tileset_dungeon.png  →  để vào  Sprites/Environment/Dungeon/
         ↓
2. Trong Unity: chọn PNG → Sprite Mode: Multiple → Sprite Editor → Slice
         ↓
3. Kéo từng sprite đã cắt vào Tile Palette  →  Unity tự tạo Tile Asset (.asset)
         ↓ Tile asset tự động lưu vào nơi bạn chỉ định
4. Chỉ định lưu vào:  Tilemaps/Tiles/Dungeon/
         ↓
5. Dùng Tile Palette để vẽ lên Tilemap Grid trong Scene
```

### ⚠️ Lỗi hay gặp với Tilemap & Git

**Tile Palette là `.prefab` → phải commit!**
```bash
git add Assets/_Game/Tilemaps/Palettes/DungeonPalette.prefab
git add Assets/_Game/Tilemaps/Palettes/DungeonPalette.prefab.meta
```

**Tile Asset là `.asset` → phải commit!**
```bash
git add Assets/_Game/Tilemaps/Tiles/Dungeon/
# Nhớ add cả thư mục để bao gồm .meta của từng .asset
```

**KHÔNG để Tile Palette trong `Prefabs/`** — dễ nhầm với Prefab của enemy/building. Tile Palette là công cụ vẽ của editor, không phải GameObject trong game.

**Mỗi người vẽ map trên Scene khác nhau** — hai người không được cùng sửa `DungeonFloor1.unity` vì tilemap data nằm trong Scene file, rất dễ conflict.

---

## 3. Quy tắc đặt tên file & folder

### Folders
- Dùng **PascalCase**: `Scripts/`, `Prefabs/`, `ScriptableObjects/`
- Không dùng dấu cách (tránh lỗi đường dẫn)
- Nhóm theo **hệ thống chức năng**, không theo loại file

### Scripts (.cs)
- Dùng **PascalCase**: `PlayerController.cs`, `EnemyAI.cs`
- Tên phải mô tả rõ chức năng: ✅ `BossController` thay vì ❌ `Boss1Script`

### Sprites / Textures
- Dùng **snake_case**: `player_idle.png`, `tileset_dungeon.png`
- Thêm tiền tố loại: `sfx_`, `bgm_`, `ui_`, `char_`, `env_`

### Scenes
- Dùng **PascalCase**: `DungeonFloor1.unity`, `MainMenu.unity`
- Không được đặt tên chung chung: ❌ `SampleScene`, ❌ `Test`, ❌ `Scene1`

### Prefabs
- Dùng **PascalCase**, không thêm "Prefab" vào tên: ✅ `Slime.prefab` thay vì ❌ `SlimePrefab.prefab`

---

## 4. Thiết lập Git cho Unity

Project của nhóm đã có `.gitignore` và `.gitattributes` đúng chuẩn. Tuy nhiên cần kiểm tra thêm:

### 4.1 Bật Visible Meta Files trong Unity

**Bắt buộc làm bước này trước khi push lần đầu!**

```
Unity Editor → Edit → Project Settings → Editor
→ Version Control Mode: Visible Meta Files
→ Asset Serialization Mode: Force Text
```

> **Tại sao?** Nếu để chế độ binary, file `.unity` và `.prefab` sẽ không thể merge, mọi conflict đều phải chọn "lấy của mình" hoặc "lấy của người kia", không thể kết hợp.

### 4.2 Thiết lập Git LFS

File ảnh, âm thanh, video rất nặng — dùng Git LFS để lưu trữ riêng:

```bash
# Cài Git LFS (chỉ làm 1 lần trên máy)
git lfs install

# Kiểm tra file .gitattributes đã có các dòng này chưa:
# *.png lfs
# *.wav lfs
# *.mp3 lfs
# *.fbx lfs
# (Project của nhóm đã có sẵn ✅)

# Khi push lần đầu:
git lfs push --all origin main
```

### 4.3 Cài UnityYAMLMerge (xử lý conflict Scene/Prefab)

```bash
# Thêm vào file .git/config hoặc ~/.gitconfig toàn cục:
[merge]
    tool = unityyamlmerge

[mergetool "unityyamlmerge"]
    trustExitCode = false
    cmd = 'C:/Program Files/Unity/Hub/Editor/<VERSION>/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
```

> Thay `<VERSION>` bằng phiên bản Unity của nhóm (ví dụ: `6000.0.47f1`)

---

## 5. Chiến lược Branching

### Mô hình branch cho nhóm làm game (Feature Branch Workflow)

```
main                    ← Branch ổn định, luôn chạy được, chỉ merge khi xong một milestone
  └── develop           ← Branch tích hợp, merge feature vào đây để test chung
        ├── feature/player-combat       ← Thành viên A làm
        ├── feature/dungeon-floor1      ← Thành viên B làm
        ├── feature/base-building       ← Thành viên C làm
        ├── feature/enemy-ai            ← Thành viên D làm
        └── fix/boss-spawn-bug          ← Hotfix khi có bug
```

### Quy tắc đặt tên branch

| Loại | Pattern | Ví dụ |
|------|---------|-------|
| Tính năng mới | `feature/tên-tính-năng` | `feature/inventory-system` |
| Sửa bug | `fix/mô-tả-bug` | `fix/player-fall-through-floor` |
| Cải thiện | `refactor/tên-phần` | `refactor/enemy-ai-cleanup` |
| Tài sản/Art | `art/loại-asset` | `art/dungeon-tileset` |
| Scene | `scene/tên-scene` | `scene/dungeon-floor2` |

### Tạo và làm việc trên branch

```bash
# Lấy code mới nhất từ develop trước khi tạo branch
git checkout develop
git pull origin develop

# Tạo branch mới cho tính năng của mình
git checkout -b feature/player-combat

# ... làm việc, commit thường xuyên ...

# Push branch lên GitHub
git push origin feature/player-combat

# Khi xong → mở Pull Request vào develop trên GitHub
```

---

## 6. Quy trình làm việc hàng ngày

### Buổi sáng — Bắt đầu làm việc

```bash
# 1. Chắc chắn mình đang ở đúng branch
git branch

# 2. Lấy code mới nhất (QUAN TRỌNG: làm trước khi mở Unity)
git fetch origin
git pull origin develop        # hoặc pull branch của mình

# 3. Mở Unity sau khi đã pull xong
```

> ⚠️ **KHÔNG bao giờ** pull trong khi Unity đang mở! Unity có thể ghi đè file hoặc gây conflict ảo.

### Trong ngày — Commit thường xuyên

```bash
# Xem những gì đã thay đổi
git status

# Thêm file vào staging (chỉ thêm những file liên quan)
git add Assets/Scripts/Player/PlayerCombat.cs
git add Assets/Prefabs/Player/Player.prefab
git add Assets/Prefabs/Player/Player.prefab.meta  # đừng quên .meta!

# KHÔNG dùng git add . một cách bừa bãi trong Unity project
# Vì có thể add nhầm file tạm, file build, v.v.

# Commit
git commit -m "feat(combat): add basic melee attack for player"

# Push lên remote
git push origin feature/player-combat
```

### Cuối ngày — Sync với nhóm

```bash
# Xem nhóm đã làm gì
git fetch origin
git log --oneline --graph origin/develop

# Nếu develop có thay đổi, merge vào branch của mình
git merge origin/develop

# Nếu có conflict → xem phần 8
```

---

## 7. Commit Message chuẩn

Dùng format: `type(scope): mô tả ngắn gọn bằng tiếng Anh`

### Các type thường dùng

| Type | Khi nào dùng |
|------|-------------|
| `feat` | Thêm tính năng mới |
| `fix` | Sửa bug |
| `art` | Thêm/chỉnh sprite, animation, audio |
| `scene` | Chỉnh sửa Scene |
| `refactor` | Cải thiện code không thêm tính năng |
| `docs` | Cập nhật tài liệu |
| `chore` | Config, setup, không ảnh hưởng gameplay |

### Scope (phần chức năng)

`player`, `enemy`, `boss`, `dungeon`, `base`, `ui`, `audio`, `inventory`, `build-system`

### Ví dụ commit message tốt ✅

```
feat(player): implement WASD movement with collision
feat(enemy): add Slime chase AI using NavMesh
fix(dungeon): fix RoomTrigger not firing on re-enter
art(sprites): add player attack animation frames
scene(dungeon-f1): place enemies and room layout for floor 1
feat(base): implement ResourceManager singleton
fix(ui): correct HP bar not updating on damage
chore: update .gitignore to exclude UserSettings
```

### Ví dụ commit message tệ ❌

```
update
fix bug
add stuff
test commit
sửa lỗi
làm xong phần player
aaa
```

---

## 8. Xử lý Conflict trong Unity

### Loại conflict thường gặp

#### a) Conflict trong file `.cs` (Script)
Đây là dễ nhất — giống web, đọc code và merge tay:

```
<<<<<<< HEAD (của mình)
    private float moveSpeed = 5f;
=======
    private float moveSpeed = 3.5f;
>>>>>>> origin/develop (của người kia)
```

Sửa thành giá trị đúng, xóa các dấu `<<<<`, `====`, `>>>>`, rồi commit.

#### b) Conflict trong file `.unity` (Scene) hoặc `.prefab`
**Đây là loại nguy hiểm nhất!**

**Cách tốt nhất: Phòng tránh trước**
- Mỗi người **chỉ được chỉnh 1 Scene** tại một thời điểm
- Thông báo cho nhóm trên chat: *"Mình đang sửa DungeonFloor1, chưa ai vào sửa nhé!"*
- Xong rồi mới báo: *"Đã push DungeonFloor1, mọi người pull về nhé"*

**Khi đã bị conflict trong Scene:**
```bash
# Dùng UnityYAMLMerge (nếu đã cài)
git mergetool

# Hoặc lấy hoàn toàn của một bên
git checkout --ours Assets/Scenes/DungeonFloor1.unity    # giữ của mình
git checkout --theirs Assets/Scenes/DungeonFloor1.unity  # lấy của người kia
git add Assets/Scenes/DungeonFloor1.unity
git commit -m "fix: resolve scene conflict, keep develop version"
```

#### c) Conflict trong file `.meta`
```bash
# .meta conflict thường do xóa/thêm cùng 1 file
# Thường nên giữ của develop (theirs)
git checkout --theirs Assets/Sprites/Characters/player_idle.png.meta
git add Assets/Sprites/Characters/player_idle.png.meta
```

---

## 9. Git LFS — Quản lý file lớn

Project của nhóm đã có `.gitattributes` với LFS cho tất cả định dạng binary. Một số lưu ý quan trọng:

### Kiểm tra file có được track bởi LFS không

```bash
git lfs ls-files
```

### Khi clone project lần đầu

```bash
git clone https://github.com/PhucPhucc/Abyss-Frontier.git
git lfs pull  # Tải tất cả file binary từ LFS storage
```

### Khi thêm asset mới

```bash
# Thêm ảnh mới vào project
git add Assets/_Game/Sprites/Characters/Player/player_idle.png
git add Assets/_Game/Sprites/Characters/Player/player_idle.png.meta

# Kiểm tra xem file có vào LFS không
git lfs status

# Commit bình thường — Git tự biết gửi qua LFS
git commit -m "art(sprites): add player idle sprite sheet"
```

### ⚠️ Giới hạn LFS trên GitHub Free

- GitHub Free: **1 GB** storage LFS + **1 GB** bandwidth/tháng
- Nếu hết quota, phải mua thêm hoặc dùng cách khác
- **Giải pháp**: Nén file âm thanh (dùng `.ogg` thay `.wav`), giảm độ phân giải ảnh khi không cần thiết

---

## 10. Phân công công việc trong nhóm

### Nguyên tắc phân chia để tránh conflict

**❌ Tránh làm:** Hai người cùng sửa một Scene, một Prefab, một Script cùng lúc

**✅ Nên làm:** Phân chia theo hệ thống, mỗi người "sở hữu" một phần

### Ví dụ phân công cho Abyss Frontier (nhóm 4-5 người)

| Thành viên | Phần chịu trách nhiệm | Scene/Branch |
|-----------|----------------------|--------------|
| **A** | Player (movement, combat, health, inventory) | `feature/player-system` |
| **B** | Enemy AI (Slime, Skeleton, Boss) | `feature/enemy-system` |
| **C** | Dungeon System + Scene layout | `scene/dungeon-floors` |
| **D** | Base Building + ResourceManager | `feature/base-building` |
| **E** | UI (HP bar, inventory UI, dialogue, build menu) | `feature/ui-system` |

### Quy trình tích hợp vào develop

```
Mỗi thứ 2 và thứ 5:
1. Mỗi người push branch của mình lên GitHub
2. Trưởng nhóm review và merge từng branch vào develop
3. Cả nhóm pull develop về, test chung
4. Log lại bug/vấn đề phát sinh vào GitHub Issues
```

### Dùng GitHub Issues để track công việc

```
Issue #1: [FEAT] Implement PlayerController - Assigned: A
Issue #2: [FEAT] Implement EnemyAI - Assigned: B
Issue #3: [ART] Create dungeon tileset - Assigned: E
Issue #4: [BUG] Player falls through floor - Assigned: A
```

Khi commit, tham chiếu đến issue:
```bash
git commit -m "feat(player): implement WASD movement, closes #1"
```

---

## 11. Các lỗi thường gặp & cách xử lý

### ❗ Lỗi: "Missing script" hoặc reference bị mất sau khi pull

**Nguyên nhân:** Ai đó xóa/đổi tên file mà không commit kèm file `.meta`

**Cách xử lý:**
```bash
# Kiểm tra file .meta đã bị xóa chưa
git log --all --full-history -- "Assets/Scripts/Player/PlayerController.cs.meta"

# Khôi phục file .meta
git checkout <commit-hash> -- "Assets/Scripts/Player/PlayerController.cs.meta"
```

**Cách phòng tránh:**
- Khi đổi tên hoặc xóa file, **luôn làm trong Unity Editor** (không dùng File Explorer)
- Unity sẽ tự cập nhật file `.meta` và các reference

---

### ❗ Lỗi: Unity mở lên nhưng Scene bị trắng xóa sau khi merge

**Nguyên nhân:** Conflict trong file `.unity` không được giải quyết đúng cách

**Cách xử lý:**
```bash
# Xem file unity có marker conflict không
grep -n "<<<<<<" Assets/Scenes/DungeonFloor1.unity

# Nếu có → phải giải quyết trước khi mở Unity
# Lấy toàn bộ version của develop
git checkout --theirs Assets/Scenes/DungeonFloor1.unity
git add Assets/Scenes/DungeonFloor1.unity
git commit -m "fix: resolve scene conflict"
```

---

### ❗ Lỗi: Commit nhầm file Library/ hoặc Temp/

**Cách xử lý:**
```bash
# Xóa khỏi git tracking (không xóa file trên máy)
git rm -r --cached Library/
git rm -r --cached Temp/
git commit -m "chore: remove Library and Temp from tracking"
```

---

### ❗ Lỗi: Push bị reject "non-fast-forward"

```bash
# KHÔNG dùng git push --force (sẽ mất code của người khác!)
# Thay vào đó:
git pull origin develop --rebase
# Giải quyết conflict nếu có
git push origin feature/ten-branch-cua-minh
```

---

### ❗ Lỗi: Lỡ commit trực tiếp vào main

```bash
# Tạo branch mới từ commit đó
git checkout -b feature/ten-tinh-nang

# Quay main về trước commit đó
git checkout main
git reset --hard HEAD~1
git push origin main --force-with-lease  # cẩn thận hơn --force
```

---

## 12. Checklist trước khi Push

Trước mỗi lần `git push`, hãy kiểm tra:

### Code
- [ ] Code build thành công trong Unity (không có lỗi đỏ trong Console)
- [ ] Không còn `Debug.Log()` tạm bợ (hoặc dùng `#if UNITY_EDITOR`)
- [ ] Script đã được assign đúng reference trong Inspector

### File
- [ ] Các file `.meta` đi kèm với file tương ứng đã được `git add`
- [ ] Không có file từ `Library/`, `Temp/`, `Logs/`, `UserSettings/` trong staging
- [ ] Đặt tên file đúng convention (không có tên chung chung như "New Script")

### Commit
- [ ] Commit message theo format chuẩn
- [ ] Mỗi commit chỉ chứa một tính năng/fix cụ thể (không gộp nhiều thứ vào 1 commit)

### Kiểm tra nhanh bằng lệnh
```bash
# Xem những gì sẽ được push
git log origin/develop..HEAD --oneline

# Xem diff của từng file
git diff --stat HEAD origin/develop

# Kiểm tra không có file nhạy cảm nào bị thêm vào
git status
```

---

## 🔧 Bảng tóm tắt lệnh Git hay dùng

```bash
# === Hàng ngày ===
git status                          # Xem trạng thái
git pull origin develop             # Lấy code mới nhất
git add <file>                      # Thêm file vào staging
git commit -m "type(scope): msg"    # Commit
git push origin <branch>            # Push lên GitHub

# === Branch ===
git branch                          # Xem danh sách branch
git checkout -b feature/ten-branch  # Tạo và chuyển sang branch mới
git checkout develop                # Chuyển về develop
git merge origin/develop            # Merge develop vào branch hiện tại

# === Xem lịch sử ===
git log --oneline --graph           # Xem lịch sử dạng đồ thị
git log -5                          # 5 commit gần nhất
git diff HEAD                       # Xem thay đổi chưa staged

# === Hoàn tác ===
git restore <file>                  # Hoàn tác thay đổi chưa staged
git restore --staged <file>         # Bỏ file khỏi staging
git revert <commit-hash>            # Hoàn tác một commit (an toàn)

# === LFS ===
git lfs status                      # Xem file LFS
git lfs pull                        # Tải file LFS
```

---

> 📝 **Ghi chú cuối:** Guide này được tạo dựa trên cấu trúc project **Abyss Frontier** của nhóm. Khi project phát triển thêm, hãy cập nhật guide này cho phù hợp. Chúc nhóm code vui! 🚀
