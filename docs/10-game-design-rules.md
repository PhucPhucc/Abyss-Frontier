# 📏 Quy chuẩn thiết kế game & Unity Settings (Game Design Rules)

> ← [Về trang chính](../GUIDES.md)

> Dành riêng cho Unity 2D (Pixel Art Top-down). Đây là những cấu hình **bắt buộc phải thống nhất** để tránh lỗi xuyên tường, hình ảnh bị vỡ, hoặc các Layer đè lên nhau lộn xộn.

---

## 1. Môi trường 2D & Pixel Art

Để game Pixel Art không bị nhòe, mờ hoặc sai tỉ lệ, toàn bộ nhóm phải tuân thủ:

| Yếu tố | Cấu hình chuẩn | Giải thích |
|--------|---------------|------------|
| **Pixels Per Unit (PPU)** | Bắt buộc chọn 1 số cố định cho toàn dự án: **`16`** hoặc **`32`** | Đảm bảo kích thước vật thể đồng đều. Mọi file ảnh khi import phải set PPU giống nhau. |
| **Filter Mode** | `Point (no filter)` | Giữ cho pixel sắc nét, không bị mờ (blur). |
| **Compression** | `None` (hoặc `High Quality`) | Chống vỡ hạt ở mép nhân vật. |

---

## 2. Quy chuẩn kích thước (Grid & Scale)

Tuyệt đối **KHÔNG** dùng công cụ Scale (phím R) để thay đổi kích thước nhân vật hay tile trên Scene một cách bừa bãi.

- **Tilemap Grid:** Mặc định là `1x1` (tương ứng với PPU = 16 thì 1 ô tile = 16x16 pixel).
- **Scale mặc định:** Tất cả Prefab Character, Enemy, Building khi thả vào Scene phải có `Scale = (1, 1, 1)`.
- Nếu Sprite quá to/nhỏ: Chỉnh bằng cách vẽ lại ảnh hoặc điều chỉnh **PPU**, KHÔNG chỉnh Scale trên Transform.

---

## 3. Hệ thống Sorting Layers (Hiển thị trước/sau)

Để không xảy ra tình trạng "Quái vật đứng trên ngọn cây" hay "Người đi chìm dưới mặt đất", thống nhất thứ tự Sorting Layer (từ dưới lên trên) như sau:

| Tên Sorting Layer | Dùng cho |
|-------------------|----------|
| `Background` | Bầu trời, nền xa (nếu có) |
| **`Ground`** | Tilemap sàn nhà, đất, cỏ, nước |
| `GroundDecor` | Vết máu, sỏi đá nhỏ, rác trên nền |
| **`Obstacles`** | Tilemap tường, cây cối, đá tảng, hố sâu |
| `Interactables` | Rương, bảng hiệu, cửa |
| **`Characters`** | Player, NPC, Enemy |
| `Foreground` | Mây, tán cây che đầu, mái nhà che khuất |
| **`UI`** | Nút bấm, thanh máu, chữ... (trên Canvas) |

> 💡 **Mẹo:** Dùng `Order in Layer` kết hợp với trục Y (Transparency Sort Axis = Y) để xử lý việc nhân vật đi ra trước/sau cái cây.

---

## 4. Hệ thống Physics Layers & Collision

Chỉ định rõ ai được va chạm với ai để giảm tải xử lý vật lý và tránh bug (vd: Player tự bắn trúng đạn của mình).

### Danh sách Physics Layers:

| Layer (Int) | Tên Layer | Mô tả |
|-------------|-----------|-------|
| 0 | `Default` | Đất, tường, chướng ngại vật cơ bản |
| 6 | `Player` | Player character |
| 7 | `Enemy` | Quái vật, Boss |
| 8 | `PlayerHitbox` | Vùng nhận sát thương của Player |
| 9 | `EnemyHitbox` | Vùng nhận sát thương của Enemy |
| 10| `Projectile` | Đạn bay, mũi tên, bùa phép |
| 11| `TriggerArea` | Vùng chuyển cảnh, phát hiện va chạm ẩn |

### Cấu hình Physics2D Collision Matrix (Edit > Project Settings > Physics 2D)
- `Player` KHÔNG va chạm với `Player` (nếu có multiplayer/clone).
- `Enemy` va chạm với `Enemy` (để quái không đứng chồng lên nhau).
- `Projectile` từ Player chỉ va chạm với `EnemyHitbox` và `Default` (tường).

---

## 5. Quy chuẩn về Tags

Không tạo Tag bừa bãi. Dùng enum hoặc các Tag chuẩn sau:

- `Player`
- `Enemy`
- `MainCamera`
- `Loot` (Vàng, đá, nguyên liệu rớt ra)
- `Interactable` (Rương, NPC, Cửa)

> ⚠️ Thay vì dùng `CompareTag("Enemy")`, ưu tiên dùng `TryGetComponent<EnemyHealth>()` sẽ an toàn và đúng nguyên tắc lập trình hơn.

---

## 6. Quy chuẩn thiết kế Scene & Layout

### Dungeon Floor
- **Kích thước phòng (Room size):** Ví dụ 15x15 hoặc 20x20 tiles (thống nhất để dễ tạo hệ thống random dungeon hoặc thiết kế tay).
- **Cửa (Doors):** Luôn đặt ở giữa các cạnh (Bắc, Nam, Đông, Tây) để dễ ghép nối.
- **Tường chắn:** Xung quanh rìa map phải có ranh giới rõ ràng (Collider chặn lại) không cho Camera rớt ra ngoài.

### Base / Surface
- Có vùng không gian rộng ở giữa để xây dựng (`BuildArea`).
- Đánh dấu ô lưới (Grid) rõ ràng để hệ thống BuildSystem biết chỗ nào được đặt, chỗ nào không.

---

## 7. Quy chuẩn Camera (Cinemachine)
- Luôn sử dụng **Cinemachine** + **Pixel Perfect Camera** (cài từ Package Manager) để theo dõi Player mượt mà.
- Bật **Confiner 2D** để Camera không quay văng ra ngoài khoảng đen của Scene.
- Aspect Ratio (Tỉ lệ màn hình) target là **16:9** (1920x1080).

---

## Bảng Checklist duyệt Game Design & Art
Mỗi khi thêm một Asset mới vào game, tự hỏi:
- [ ] Ảnh đã set PPU chuẩn (16 hoặc 32) chưa?
- [ ] Filter mode đã là Point (no filter) chưa?
- [ ] Đã gán đúng Sorting Layer chưa?
- [ ] Có collider chưa? Gán đúng Physics Layer chưa?
- [ ] Đã đưa thành Prefab trước khi thả hàng loạt vào map chưa?
