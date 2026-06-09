# 🎮 ABYSS FRONTIER — Tài liệu nhóm

> Unity · Git · C# · Pixel Art 2D

---

## 📖 Đọc ngay nếu bạn là thành viên mới

👉 **[Setup máy mới từ A đến Z](./docs/02-setup.md)** — cài Unity, Git LFS, clone repo, tạo branch

---

## 📋 Mục lục tài liệu

| # | Tài liệu | Nội dung |
|---|----------|---------|
| 01 | [Git Overview](./docs/01-git-overview.md) | Tại sao Unity + Git khó, Nguyên tắc vàng, loại file cần commit |
| 02 | [Setup máy mới](./docs/02-setup.md) | Cài đặt công cụ, clone repo, cấu hình Unity — từng bước |
| 03 | [Cấu trúc thư mục](./docs/03-folder-structure.md) | Cây thư mục đầy đủ, giải thích Tilemap |
| 04 | [Đặt tên file & folder](./docs/04-naming-convention.md) | Convention cho Scripts, Scene, Prefab, Audio, Sprite... |
| 05 | [Quy tắc viết code C#](./docs/05-coding-convention.md) | Naming, SerializeField, Namespace, Event, ScriptableObject... |
| 06 | [Git Workflow](./docs/06-git-workflow.md) | Branching strategy, Daily workflow, Protocol khi sửa Scene |
| 07 | [Commit & Pull Request](./docs/07-commit-and-pr.md) | Commit message format, PR template, Quy tắc review |
| 08 | [Phân công & Scene Ownership](./docs/08-team-assignment.md) | Ai làm gì, ai sở hữu Scene nào, Quy trình tích hợp |
| 09 | [Troubleshooting](./docs/09-troubleshooting.md) | Conflict, Lỗi thường gặp, Emergency Protocol, Checklist, Cheatsheet |
| 10 | [Quy chuẩn Game Design](./docs/10-game-design-rules.md) | PPU, Sorting Layer, Physics, Grid Layout, Camera cho 2D Pixel Art |

---

## ⚡ Quick Reference

### Bắt đầu ngày làm việc
```bash
git fetch origin
git pull origin develop       # trước khi mở Unity!
# → mở Unity
```

### Commit & Push
```bash
git add <file> <file.meta>
git commit -m "feat(player): implement attack"
git push origin feature/ten-branch
```

### Khi xong → mở Pull Request
```
GitHub → Pull requests → New pull request
Base: develop  ←  Compare: feature/ten-branch
```

### Thông báo bắt buộc khi sửa Scene
```
🔒 [LOCK]   Mình đang sửa DungeonFloor1.unity — chưa ai vào nhé!
✅ [UNLOCK] Đã push DungeonFloor1 xong, mọi người pull về nhé
```

---

## 🗂️ Tài liệu khác

- [Game Design Document](./base.md) — Gameplay loop, story, features, art direction
- [.gitignore](./.gitignore) — File bị ignore
- [.gitattributes](./.gitattributes) — LFS và merge tool config

---

> Unity version bắt buộc: **6000.3.16f1**
> Repo: https://github.com/PhucPhucc/Abyss-Frontier
