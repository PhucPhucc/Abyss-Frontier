# 🛠️ Setup máy mới — Bắt đầu từ A đến Z

> ← [Về trang chính](../GUIDES.md)

> Làm theo **đúng thứ tự**. Bỏ qua bước nào cũng có thể gây lỗi sau.

---

## Bước 1: Cài đặt công cụ

| Công cụ | Phiên bản | Link |
|---------|-----------|------|
| **Unity Hub** | Mới nhất | https://unity.com/download |
| **Unity Editor** | **6000.3.16f1** ⚠️ bắt buộc đúng version | Cài qua Unity Hub |
| **Git** | 2.40+ | https://git-scm.com/download/win |
| **Git LFS** | Mới nhất | https://git-lfs.com/ |
| **Visual Studio / VS Code** | Tùy chọn | IDE để viết C# |

> ⚠️ **Bắt buộc dùng Unity `6000.3.16f1`**.
> Dùng khác version sẽ gây conflict file `.unity` và `.prefab` mà **không thể merge** được.

---

## Bước 2: Cài Git LFS (chỉ làm 1 lần trên máy)

```bash
git lfs install
```

Kiểm tra thành công:
```bash
git lfs version
# Output: git-lfs/3.x.x (...)
```

---

## Bước 3: Clone project

```bash
git clone https://github.com/PhucPhucc/Abyss-Frontier.git
cd Abyss-Frontier

# Tải tất cả file binary từ LFS storage
git lfs pull
```

---

## Bước 4: Cài UnityYAMLMerge (xử lý conflict Scene/Prefab)

Thêm vào file `~/.gitconfig` (toàn cục) — mở bằng lệnh:

```bash
git config --global --edit
```

Dán vào cuối file:

```ini
[merge]
    tool = unityyamlmerge

[mergetool "unityyamlmerge"]
    trustExitCode = false
    cmd = 'C:/Program Files/Unity/Hub/Editor/6000.3.16f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
```

---

## Bước 5: Mở project trong Unity

1. Mở **Unity Hub**
2. Click **Open** → chọn thư mục `Abyss-Frontier`
3. Đợi Unity import asset (lần đầu có thể mất **5–10 phút**)
4. Vào **Edit → Project Settings → Editor** → kiểm tra 2 mục:

| Setting | Giá trị bắt buộc |
|---------|-----------------|
| Version Control Mode | **Visible Meta Files** |
| Asset Serialization Mode | **Force Text** |

> **Tại sao?** Nếu để Binary, file `.unity` và `.prefab` sẽ không thể merge —
> mọi conflict đều phải chọn "lấy của mình" hoặc "lấy của người kia", mất công của người còn lại.

---

## Bước 6: Tạo branch cá nhân

```bash
git checkout develop
git pull origin develop
git checkout -b feature/ten-tinh-nang-cua-ban
```

✅ **Xong! Bạn đã sẵn sàng làm việc.**

---

## Checklist setup ✅

- [ ] Đã cài Unity Hub + Unity **6000.3.16f1**
- [ ] Đã cài Git 2.40+
- [ ] Đã chạy `git lfs install` thành công
- [ ] Đã clone repo và chạy `git lfs pull`
- [ ] Đã cài UnityYAMLMerge vào `.gitconfig`
- [ ] Đã bật **Visible Meta Files** + **Force Text** trong Project Settings
- [ ] Đã tạo branch cá nhân từ `develop`
