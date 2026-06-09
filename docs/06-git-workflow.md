# 🌿 Chiến lược Git & Quy trình làm việc hàng ngày

> ← [Về trang chính](../GUIDES.md)

---

## Mô hình Branch

```
main                    ← Branch ổn định, luôn chạy được.
│                         Chỉ merge khi xong milestone. KHÔNG push trực tiếp!
└── develop             ← Branch tích hợp. Merge feature vào đây để test chung.
      ├── feature/player-combat       ← Thành viên A
      ├── feature/dungeon-floor1      ← Thành viên B
      ├── feature/base-building       ← Thành viên C
      ├── feature/enemy-ai            ← Thành viên D
      ├── feature/ui-system           ← Thành viên E
      └── fix/boss-spawn-bug          ← Hotfix khi có bug
```

---

## Quy tắc bắt buộc về Branch

| Rule | Chi tiết |
|------|----------|
| ❌ **Không push thẳng vào `main`** | `main` chỉ merge qua PR sau khi milestone hoàn thành |
| ❌ **Không push thẳng vào `develop`** | `develop` chỉ nhận merge từ feature branch qua PR |
| ✅ **Mỗi tính năng = 1 branch** | Không làm nhiều tính năng trên 1 branch |
| ✅ **Branch đã merge = xóa** | Xóa branch sau khi PR được merge để tránh rối |

---

## Đặt tên Branch

| Loại | Pattern | Ví dụ |
|------|---------|-------|
| Tính năng mới | `feature/tên-tính-năng` | `feature/inventory-system` |
| Sửa bug | `fix/mô-tả-bug` | `fix/player-fall-through-floor` |
| Cải thiện code | `refactor/tên-phần` | `refactor/enemy-ai-cleanup` |
| Asset / Art | `art/loại-asset` | `art/dungeon-tileset` |
| Scene | `scene/tên-scene` | `scene/dungeon-floor2` |
| Hotfix khẩn cấp | `hotfix/mô-tả` | `hotfix/build-crash-on-start` |

---

## Tạo Branch & Bắt đầu làm việc

```bash
# Lấy code mới nhất từ develop trước
git checkout develop
git pull origin develop

# Tạo branch mới
git checkout -b feature/player-combat

# ... làm việc, commit thường xuyên ...

# Push branch lên GitHub
git push origin feature/player-combat

# Khi xong → mở Pull Request vào develop (xem docs/07-commit-and-pr.md)
```

---

## Quy trình làm việc hàng ngày

### 🌅 Buổi sáng — Bắt đầu làm việc

```bash
# 1. Kiểm tra đang ở đúng branch
git branch

# 2. Lấy code mới nhất — QUAN TRỌNG: làm TRƯỚC khi mở Unity!
git fetch origin
git pull origin develop        # hoặc pull branch của mình nếu teammate có push

# 3. Thông báo nhóm (Discord/Zalo):
#    "Mình bắt đầu làm [tính năng X], đang dùng branch feature/xxx"
#    "🔒 [LOCK] Mình đang sửa DungeonFloor1.unity — chưa ai vào nhé!"

# 4. Mở Unity SAU KHI đã pull xong
```

> ⚠️ **KHÔNG bao giờ** pull khi Unity đang mở!
> Unity có thể ghi đè file hoặc gây conflict ảo.

---

### ☀️ Trong ngày — Commit thường xuyên

```bash
# Xem những gì đã thay đổi
git status

# Thêm file vào staging — CHỈ thêm file liên quan, đừng dùng "git add ." bừa bãi!
git add Assets/Scripts/Player/PlayerCombat.cs
git add Assets/Scripts/Player/PlayerCombat.cs.meta   # đừng quên .meta!
git add Assets/Prefabs/Player/Player.prefab
git add Assets/Prefabs/Player/Player.prefab.meta

# Commit với message chuẩn
git commit -m "feat(combat): add basic melee attack for player"

# Push lên remote
git push origin feature/player-combat
```

> ⚠️ **Không dùng `git add .`** trong Unity project —
> có thể add nhầm file tạm, `UserSettings/`, file build, v.v.

**Commit thường xuyên** (mỗi 30–60 phút khi có thay đổi đáng kể), đừng để cuối ngày mới commit một lần.

---

### 🌙 Cuối ngày — Sync với nhóm

```bash
# Xem nhóm đã làm gì hôm nay
git fetch origin
git log --oneline --graph origin/develop

# Nếu develop có thay đổi mới → merge vào branch của mình ngay
# để tránh conflict lớn tích lũy
git merge origin/develop

# Nếu có conflict → xem docs/09-troubleshooting.md

# Push lại sau khi merge
git push origin feature/ten-branch-cua-minh

# Thông báo nhóm nếu đã xong Scene:
# "✅ [UNLOCK] Đã push DungeonFloor1 xong, mọi người pull về nhé"
```

---

## Protocol khi làm với Scene

Bắt buộc thông báo nhóm khi đụng vào Scene file:

| Thời điểm | Nhắn gì trên chat |
|-----------|------------------|
| Bắt đầu sửa | `🔒 [LOCK] Mình đang sửa DungeonFloor1.unity — chưa ai vào nhé!` |
| Đã push xong | `✅ [UNLOCK] Đã push DungeonFloor1 xong, mọi người pull về nhé` |
| Cần người khác review | `👀 [REVIEW] PR #12 cần review, mình sửa PlayerCombat + Scene Floor1` |

Xem thêm [Phân công Scene Ownership](./08-team-assignment.md).

---

## .gitignore & .gitattributes

Project đã có sẵn. Kiểm tra nhanh:

**.gitignore phải có:**
```gitignore
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Ll]ogs/
[Uu]ser[Ss]ettings/
*.csproj
*.sln
```

**.gitattributes phải có:**
```
*.unity  merge=unityyamlmerge eol=lf
*.prefab merge=unityyamlmerge eol=lf
*.asset  merge=unityyamlmerge eol=lf
*.png    filter=lfs diff=lfs merge=lfs -text
*.ogg    filter=lfs diff=lfs merge=lfs -text
*.wav    filter=lfs diff=lfs merge=lfs -text
*.mp3    filter=lfs diff=lfs merge=lfs -text
*.fbx    filter=lfs diff=lfs merge=lfs -text
```
