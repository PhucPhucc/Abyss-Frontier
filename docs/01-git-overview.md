# 📖 Tại sao làm game với Git khó hơn làm web?

> ← [Về trang chính](../GUIDES.md)

---

Khi làm web, hầu hết file đều là **text** (HTML, CSS, JS) — Git merge rất dễ dàng.

Với Unity, bạn gặp thêm nhiều vấn đề:

| Vấn đề | Giải thích |
|--------|-----------|
| **File nhị phân (binary)** | Ảnh PNG, audio WAV, file FBX không thể merge — ai push trước thì thắng |
| **File `.unity` (Scene)** | Scene là YAML nhưng rất dài, conflict cực kỳ khó đọc và sửa |
| **File `.prefab`** | Tương tự Scene, dễ conflict khi nhiều người cùng sửa một Prefab |
| **Thư mục `Library/`** | Unity tự tạo ra, nặng hàng GB, **không được commit** |
| **File `.meta`** | Unity dùng để track GUID của asset — **phải commit**, nếu mất thì mất hết references |

---

## ✅ Nguyên tắc vàng — Học thuộc lòng!

| # | Nguyên tắc | Chi tiết |
|---|-----------|---------|
| 1 | **Không bao giờ xóa `.meta` thủ công** | Xóa = mất hết reference trong Unity |
| 2 | **Không commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`** | File tự sinh ra, nặng hàng GB |
| 3 | **Mỗi người sở hữu một Scene/hệ thống tại một thời điểm** | Tránh conflict Scene |
| 4 | **Không push thẳng vào `main` hoặc `develop`** | Luôn dùng branch riêng + PR |
| 5 | **Đổi tên/xóa file phải làm trong Unity Editor** | Không dùng File Explorer hay Terminal |
| 6 | **Kéo code mới (pull) trước khi mở Unity** | Tránh conflict ảo do Unity ghi đè |
| 7 | **Thông báo nhóm khi bắt đầu/xong sửa Scene** | `🔒 [LOCK]` và `✅ [UNLOCK]` trên chat |

---

## 🗂️ Các loại file Unity cần biết

### File PHẢI commit
- `Assets/**/*.cs` — Script C#
- `Assets/**/*.unity` — Scene
- `Assets/**/*.prefab` — Prefab
- `Assets/**/*.asset` — ScriptableObject, Tile, Material...
- `Assets/**/*.meta` — **Quan trọng!** Mọi file/folder đều có `.meta` đi kèm
- `Assets/**/*.anim`, `*.controller` — Animation
- `Assets/**/*.png`, `*.ogg`... — Asset binary (qua LFS)
- `ProjectSettings/**` — Cài đặt project Unity

### File KHÔNG được commit (đã có trong .gitignore)
- `Library/` — Cache Unity tự sinh (~GB)
- `Temp/` — File tạm khi build/run
- `Logs/` — Log Unity
- `UserSettings/` — Cài đặt cá nhân (layout editor...)
- `*.sln`, `*.csproj` — File Visual Studio tự sinh
