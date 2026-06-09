# 📝 Commit Message & Pull Request

> ← [Về trang chính](../GUIDES.md)

---

## Commit Message chuẩn

Format: **`type(scope): mô tả ngắn gọn bằng tiếng Anh`**

---

### Các `type` thường dùng

| Type | Khi nào dùng |
|------|-------------|
| `feat` | Thêm tính năng mới |
| `fix` | Sửa bug |
| `art` | Thêm/chỉnh sprite, animation, audio |
| `scene` | Chỉnh sửa Scene (layout, tilemap, GameObject) |
| `refactor` | Cải thiện code, không thêm tính năng |
| `docs` | Cập nhật tài liệu (GUIDES.md, base.md, docs/) |
| `chore` | Config, setup, không ảnh hưởng gameplay |
| `data` | Thêm/chỉnh ScriptableObject data |

### Các `scope` trong Abyss Frontier

`player` · `enemy` · `boss` · `dungeon` · `base` · `ui` · `audio` · `inventory` · `build-system` · `tilemap` · `core`

---

### ✅ Commit message tốt

```
feat(player): implement WASD movement with collision
feat(enemy): add Slime chase AI using NavMesh
fix(dungeon): fix RoomTrigger not firing on re-enter
art(sprites): add player attack animation frames
scene(dungeon-f1): place enemies and room layout for floor 1
feat(base): implement ResourceManager singleton
fix(ui): correct HP bar not updating on damage
data(enemy): add SlimeData and SkeletonData ScriptableObjects
chore: update .gitignore to exclude UserSettings
docs: update GUIDES.md with PR workflow section
feat(player): implement WASD movement, closes #1
```

### ❌ Commit message tệ

```
update
fix bug
add stuff
test commit
sửa lỗi
làm xong phần player
aaa
FINAL
FINAL_v2
ok done
```

---

## Pull Request (PR)

### Khi nào mở PR?

- Hoàn thành một tính năng hoàn chỉnh (ít nhất chạy được)
- Muốn merge vào `develop` để test chung với nhóm
- Cần nhờ người khác review/test trước khi tiếp tục

### Quy trình mở PR

```
1. Push branch lên GitHub
   git push origin feature/ten-branch

2. GitHub → Pull requests → New pull request
   Base: develop  ←  Compare: feature/ten-branch

3. Đặt tiêu đề: [FEAT] Implement Player Combat System

4. Điền mô tả theo template bên dưới

5. Assign ít nhất 1 Reviewer (trưởng nhóm hoặc người liên quan)

6. Chờ approval → sau khi approved, Trưởng nhóm merge
```

---

### PR Description Template

Dùng mẫu này khi tạo PR trên GitHub:

```markdown
## Mô tả thay đổi
Ngắn gọn tính năng/fix đã làm.

## Đã làm được gì
- [x] PlayerController: WASD movement
- [x] PlayerCombat: melee attack với animation
- [ ] Chưa làm: combo attack (để sprint sau)

## Cách test
1. Mở Scene DungeonFloor1
2. Nhấn Play
3. Dùng WASD di chuyển
4. Nhấn Z để attack

## Screenshots / GIF (nếu có)
[paste ảnh/gif ở đây]

## Ghi chú cho Reviewer
- File PlayerCombat.cs thay đổi nhiều, cần review kỹ
- Chưa xử lý edge case khi player bị stun, sẽ làm ở issue #Z
```

---

### Quy tắc Review

| Rule | Chi tiết |
|------|----------|
| ✅ **Cần ít nhất 1 approval** | Không merge khi chưa có ai approve |
| ❌ **Không tự merge PR của mình** | Phải có người khác approve |
| ✅ **Reviewer phải tự pull và test** | Không approve chỉ đọc code trên GitHub |
| ✅ **Comment bằng tiếng Việt** | Hoặc tiếng Anh đều được, miễn rõ ràng |
| ⏰ **Không để PR mở quá 3 ngày** | Nhắc nhau trên chat nếu chưa được review |

---

### Merge PR (Trưởng nhóm thực hiện)

```bash
# Trên GitHub UI:
# 1. Kiểm tra CI/checks đều xanh
# 2. Ít nhất 1 approval
# 3. Chọn merge strategy:
#    - "Squash and merge"  → nếu có nhiều commit nhỏ vụn
#    - "Merge commit"      → nếu muốn giữ nguyên lịch sử commit
# 4. Merge → GitHub hỏi "Delete branch?" → Chọn YES

# Sau khi merge, thông báo nhóm:
# "✅ Đã merge feature/player-combat vào develop. Pull về nhé!"
```

Cả nhóm sau đó:
```bash
git checkout develop
git pull origin develop
```

---

### Tham chiếu Issue trong commit

```bash
# Dùng keyword "closes" hoặc "fixes" + số issue
git commit -m "feat(player): implement WASD movement, closes #1"
git commit -m "fix(dungeon): fix room trigger bug, fixes #7"

# GitHub tự động đóng Issue khi PR được merge vào main/develop
```

### Ví dụ GitHub Issues

```
Issue #1: [FEAT] Implement PlayerController - Assigned: A
Issue #2: [FEAT] Implement EnemyAI - Assigned: B
Issue #3: [SCENE] Build DungeonFloor1 layout - Assigned: C
Issue #4: [FEAT] Implement BuildSystem - Assigned: D
Issue #5: [UI] Implement HP bar and Inventory UI - Assigned: E
Issue #6: [ART] Create dungeon tileset - Assigned: C
Issue #7: [BUG] Player falls through floor - Assigned: A
```
