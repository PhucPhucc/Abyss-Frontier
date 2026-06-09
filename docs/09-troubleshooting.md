# 🚨 Troubleshooting — Conflict, Lỗi thường gặp & Emergency

> ← [Về trang chính](../GUIDES.md)

---

## Xử lý Conflict trong Unity

### a) Conflict trong `.cs` (Script) — Dễ nhất

```
<<<<<<< HEAD (của mình)
    private float _moveSpeed = 5f;
=======
    private float _moveSpeed = 3.5f;
>>>>>>> origin/develop (của người kia)
```

**Cách xử lý:**
1. Đọc cả 2 phần
2. Giữ giá trị đúng (hoặc thảo luận với người kia)
3. Xóa hết marker `<<<<`, `====`, `>>>>`
4. Build thử trong Unity → rồi commit

---

### b) Conflict trong `.unity` (Scene) hoặc `.prefab` — Nguy hiểm nhất!

**Cách tốt nhất: Phòng tránh** — xem [Scene Ownership](./08-team-assignment.md).

**Khi đã bị conflict:**

```bash
# Dùng UnityYAMLMerge (nếu đã cài ở bước setup)
git mergetool

# Hoặc chọn hẳn 1 bên (bên còn lại mất thay đổi!)
git checkout --ours   Assets/Scenes/DungeonFloor1.unity  # giữ của mình
git checkout --theirs Assets/Scenes/DungeonFloor1.unity  # lấy của người kia
git add Assets/Scenes/DungeonFloor1.unity
git commit -m "fix: resolve scene conflict, keep develop version"
```

> ⚠️ Khi dùng `--ours` hoặc `--theirs`, **một bên mất toàn bộ thay đổi**.
> Thảo luận với nhóm trước khi quyết định!

---

### c) Conflict trong `.meta`

```bash
# .meta conflict thường do xóa/thêm cùng 1 file ở 2 branch
# Thường nên giữ của develop (theirs)
git checkout --theirs Assets/Sprites/Characters/player_idle.png.meta
git add Assets/Sprites/Characters/player_idle.png.meta
git commit -m "fix: resolve meta conflict"
```

---

## Git LFS — Quản lý file lớn

```bash
# Kiểm tra file đang được LFS track
git lfs ls-files

# Khi clone project lần đầu
git clone https://github.com/PhucPhucc/Abyss-Frontier.git
git lfs pull  # Tải tất cả file binary từ LFS storage

# Khi thêm asset mới
git add Assets/Sprites/Characters/Player/player_idle.png
git add Assets/Sprites/Characters/Player/player_idle.png.meta
git lfs status   # kiểm tra file có vào LFS không
git commit -m "art(sprites): add player idle sprite sheet"
```

**Giới hạn LFS GitHub Free:** 1 GB storage + 1 GB bandwidth/tháng

**Tiết kiệm quota:**
- Dùng `.ogg` thay `.wav` (nhỏ hơn ~10x)
- Giảm độ phân giải ảnh khi không cần thiết
- Không để file test/tạm trong LFS

---

## Các lỗi thường gặp

### ❗ "Missing script" / Reference bị mất sau khi pull

**Nguyên nhân:** Ai đó xóa/đổi tên file mà không commit kèm file `.meta`

```bash
# Tìm commit cuối cùng có file .meta đó
git log --all --full-history -- "Assets/Scripts/Player/PlayerController.cs.meta"

# Khôi phục file .meta
git checkout <commit-hash> -- "Assets/Scripts/Player/PlayerController.cs.meta"
git add "Assets/Scripts/Player/PlayerController.cs.meta"
git commit -m "fix: restore missing meta file"
```

**Phòng tránh:** Luôn đổi tên/xóa file **trong Unity Editor** — không dùng File Explorer hay terminal.

---

### ❗ Scene bị trắng xóa sau khi merge

**Nguyên nhân:** File `.unity` còn marker conflict chưa giải quyết khi mở Unity

```bash
# Kiểm tra marker conflict
grep -n "<<<<<<" Assets/Scenes/DungeonFloor1.unity

# Nếu có → giải quyết TRƯỚC khi mở Unity
git checkout --theirs Assets/Scenes/DungeonFloor1.unity
git add Assets/Scenes/DungeonFloor1.unity
git commit -m "fix: resolve scene conflict"
```

---

### ❗ Lỡ commit file Library/ hoặc Temp/

```bash
# Xóa khỏi git tracking (không xóa file trên máy)
git rm -r --cached Library/
git rm -r --cached Temp/
git commit -m "chore: remove Library and Temp from tracking"
```

---

### ❗ Push bị reject "non-fast-forward"

```bash
# ❌ KHÔNG dùng git push --force (sẽ mất code của người khác!)

# ✅ Thay vào đó:
git pull origin develop --rebase
# Giải quyết conflict nếu có
git push origin feature/ten-branch-cua-minh
```

---

### ❗ Lỡ commit thẳng vào main hoặc develop

```bash
# Tạo branch mới từ commit vừa push nhầm
git checkout -b feature/ten-tinh-nang

# Quay main về trước commit đó
git checkout main
git reset --hard HEAD~1
git push origin main --force-with-lease  # cẩn thận hơn --force
```

---

### ❗ File `.unity` liên tục bị sửa dù không thay đổi gì

**Nguyên nhân:** Thành viên dùng Unity version khác nhau

**Cách xử lý:**
1. Tất cả phải cài đúng **Unity 6000.3.16f1** (xem [Setup](./02-setup.md))
2. Unity Hub → Projects → kiểm tra version đang dùng
3. Nếu đã lỡ commit với version khác → thông báo nhóm và rollback

---

## 🆘 Emergency Protocol — Khi mọi thứ hỏng

> **Bình tĩnh.** Hầu hết mọi thứ trong Git đều **có thể phục hồi**.

### Cấp 1: Branch của mình bị lỗi (chỉ ảnh hưởng mình)

```bash
# Xem lịch sử, tìm commit tốt cuối cùng
git log --oneline -10

# Quay về commit đó (mất thay đổi sau commit này)
git reset --hard <commit-hash>

# Hoặc: tạo branch backup từ commit tốt, bỏ branch hỏng
git checkout -b feature/ten-branch-backup <commit-hash>
```

---

### Cấp 2: develop bị broken (ảnh hưởng cả nhóm)

**Ai phát hiện trước → báo ngay cho nhóm trên chat!**

```bash
# Bước 1: Tìm commit tốt cuối cùng trên develop
git log origin/develop --oneline -20

# Bước 2: Trưởng nhóm rollback develop
git checkout develop
git reset --hard <commit-hash-tot>
git push origin develop --force-with-lease

# Bước 3: Thông báo nhóm
# "⚠️ Đã rollback develop về <hash>. Mọi người pull lại nhé!"

# Bước 4: Từng thành viên sync lại
git fetch origin
git checkout feature/ten-branch-cua-minh
git rebase origin/develop
```

---

### Cấp 3: Lỡ xóa file quan trọng

```bash
# Tìm commit cuối cùng có file đó
git log --all --full-history -- "Assets/Scripts/Player/PlayerController.cs"

# Khôi phục file từ commit đó
git checkout <commit-hash> -- "Assets/Scripts/Player/PlayerController.cs"
git add "Assets/Scripts/Player/PlayerController.cs"
git commit -m "fix: restore accidentally deleted file"
```

---

### Cấp 4: Lỡ force push ghi đè code của người khác

```bash
# Tìm hash của commit bị mất trong reflog
git reflog

# Tạo branch mới từ commit bị mất để phục hồi
git checkout -b hotfix/recover-lost-commits <commit-hash>
```

> 💡 **Nguyên tắc:** Khi không chắc chắn, hãy hỏi trưởng nhóm **TRƯỚC** khi chạy lệnh có `--force` hay `reset --hard`.

---

## ✅ Checklist trước khi Push

### Code
- [ ] Build thành công trong Unity (không có lỗi đỏ trong Console)
- [ ] Không còn `Debug.Log()` tạm bợ (hoặc đã bọc trong `#if UNITY_EDITOR`)
- [ ] Script đã assign đúng reference trong Inspector
- [ ] Không có `public` field thừa — dùng `[SerializeField] private` thay thế
- [ ] Đã dùng namespace đúng theo folder

### File
- [ ] Các file `.meta` đi kèm với file tương ứng đã được `git add`
- [ ] Không có file từ `Library/`, `Temp/`, `Logs/`, `UserSettings/` trong staging
- [ ] Đặt tên file đúng convention (không có `New Script`, `SampleScene`...)
- [ ] File audio dùng `.ogg`, không phải `.wav`

### Commit
- [ ] Commit message theo format chuẩn (`type(scope): description`)
- [ ] Mỗi commit chỉ chứa một tính năng/fix cụ thể
- [ ] Đã tham chiếu issue liên quan (nếu có): `closes #X`

### Kiểm tra nhanh
```bash
git log origin/develop..HEAD --oneline  # xem những gì sẽ được push
git diff --stat HEAD origin/develop     # xem diff từng file
git status                              # kiểm tra không có file nhạy cảm
```

---

## 📋 Git Cheatsheet

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
git branch -d feature/ten-branch    # Xóa branch đã merge

# === Xem lịch sử ===
git log --oneline --graph           # Xem lịch sử dạng đồ thị
git log -5                          # 5 commit gần nhất
git diff HEAD                       # Xem thay đổi chưa staged
git reflog                          # Lịch sử đầy đủ (kể cả đã xóa)

# === Hoàn tác ===
git restore <file>                  # Hoàn tác thay đổi chưa staged
git restore --staged <file>         # Bỏ file khỏi staging
git revert <commit-hash>            # Hoàn tác một commit (an toàn, tạo commit mới)
git reset --hard <commit-hash>      # Quay về commit (NGUY HIỂM, mất thay đổi sau)

# === LFS ===
git lfs status                      # Xem file LFS đang pending
git lfs pull                        # Tải file LFS
git lfs ls-files                    # Liệt kê file đang được LFS track
```
