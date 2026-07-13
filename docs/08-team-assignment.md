# 👥 Phân công công việc & Scene Ownership

> ← [Về trang chính](../GUIDES.md)

> Cập nhật theo [`Backlog_PRU213_Group01.xlsx`](../Backlog_PRU213_Group01.xlsx) — sheet **👥 Phân công TV**.

---

## Nguyên tắc phân chia để tránh conflict

**❌ Tránh làm:** Hai người cùng sửa một Scene, một Prefab, một Script cùng lúc

**✅ Nên làm:** Phân chia theo hệ thống — mỗi người "sở hữu" một phần rõ ràng

---

## Bảng phân công (nhóm 5 người)

| Thành viên | Scene / Hệ thống chính | Character | Nhiệm vụ phụ | Branch gợi ý |
|-----------|------------------------|-----------|--------------|--------------|
| **Duy Phúc** | Tầng 1, Core Setup, Player | Player | Network lead, Firestore/DB (T-84/T-86), QA & Build | `feature/player-system` |
| **Trung Nguyên** | Tầng 2, Stat/EXP, HUD, Auth scene | Plant | Auth UI (T-68), Google login (T-83), Host lobby (T-85) | `feature/ui-system` |
| **Bảo Nguyên** | Tầng 3, Audio | Slime | Map decoration (T-82), Lighting, Meep variants | `feature/enemy-system` |
| **Khải Toàn** | Tầng 4, Boss fight | Orc | Puzzle wiring, Door/lever, Mimic boss | `feature/boss-system` |
| **Đức Hải** | Tầng 5, Hub | Vampire | Wave spawn (T-81), Save client (T-67), Hub respawn (T-76) | `feature/hub-save` |

---

## Scene Ownership — Quan trọng nhất!

> **Quy tắc vàng:** Chỉ 1 người được sửa 1 Scene tại một thời điểm.

| Scene | Người sở hữu | Người khác muốn sửa → |
|-------|-------------|----------------------|
| `floor1.unity` | **Duy Phúc** | Hỏi Duy Phúc trước |
| `floor2.unity` | **Trung Nguyên** | Hỏi Trung Nguyên trước |
| `floor3.unity` | **Bảo Nguyên** | Hỏi Bảo Nguyên trước |
| `floor4.unity` | **Khải Toàn** | Hỏi Khải Toàn trước |
| `floor5.unity` | **Đức Hải** | Hỏi Đức Hải trước |
| `Scene_Menu.unity` | **Trung Nguyên** | Hỏi Trung Nguyên trước |
| `Authenticaion.unity` | **Trung Nguyên** | Hỏi Trung Nguyên trước |
| `Scene-Server.unity` | **Duy Phúc** | Hỏi Duy Phúc trước |

**Quy trình khi cần sửa Scene không phải của mình:**
1. Nhắn người sở hữu trên chat: _"Mình cần thêm 1 trigger vào floor1, được không?"_
2. Đợi họ xác nhận **không đang sửa**
3. Nhắn `🔒 [LOCK] Mình đang sửa floor1.unity`
4. Làm xong → commit → push → nhắn `✅ [UNLOCK] Xong rồi, mọi người pull về`

---

## Script / Folder Ownership

| Folder / Hệ thống | Người sở hữu |
|-------------------|-------------|
| `Scripts/Player/` · `Prefabs/Player/` | Duy Phúc |
| `Scripts/Network/` · `Scene-Server.unity` | Duy Phúc |
| `Scripts/Enemy/` (Slime) · `floor3.unity` | Bảo Nguyên |
| `Scripts/Enemy/` (Orc, Boss) · `floor4.unity` | Khải Toàn |
| `Scripts/Enemy/` (Vampire) · `floor5.unity` | Đức Hải |
| `Scripts/UI/` · `Scripts/Menu/` · `Scene_Menu.unity` · `Authenticaion.unity` | Trung Nguyên |
| `Scripts/Save/` · `Scripts/Base_camp/` | Đức Hải |
| `Scripts/Cloud/` (client integration) | Đức Hải |
| `Scripts/Cloud/` (schema, security rules) | Duy Phúc |
| `Scripts/Object/` (Puzzle) · `Scripts/Door/` | Khải Toàn |
| `Scripts/Audio/` · `Scripts/Light/` | Bảo Nguyên |
| `Tilemaps/` (theo tầng) | Owner tầng tương ứng |

---

## Prefab Ownership

> Không sửa Base Prefab của người khác — tạo Prefab Variant nếu cần thay đổi nhỏ.

| Prefab | Người sở hữu |
|--------|-------------|
| Player prefabs (SPUM / Hero) | Duy Phúc |
| Plant / Slime prefabs | Trung Nguyên / Bảo Nguyên |
| Orc / Boss / Mimic prefabs | Khải Toàn |
| Vampire prefabs | Đức Hải |
| UI runtime (HUD, StatScreen) | Trung Nguyên |

---

## Quy trình tích hợp vào develop

```
📅 Mỗi thứ 2 và thứ 5:

1. Mỗi người đảm bảo branch của mình đã push và game chạy được
2. Mỗi người mở PR vào develop trên GitHub
3. Reviewer (1 người khác) tự pull về, test, rồi approve
4. Trưởng nhóm merge PR
5. Cả nhóm pull develop về, test chung
6. Log lại bug / vấn đề phát sinh vào GitHub Issues
```

---

## GitHub Issues — Track công việc

**Dùng Issues để:**
- Giao task cho từng người (tham chiếu ID backlog: T-01, T-64, …)
- Track tiến độ
- Log bug cần fix

**Format tiêu đề Issue:**
```
[FEAT] T-76 Hub-only enemy respawn     → Assigned: Đức Hải
[FEAT] T-77 Wire puzzle to boss door   → Assigned: Khải Toàn
[BUG] Player falls through floor       → Assigned: Duy Phúc
```

**Tham chiếu Issue khi commit:**
```bash
git commit -m "fix(enemy): disable auto-respawn, closes #12"
```

---

## Sprint Timeline

Xem sheet **🗓 Sprint Timeline** trong backlog Excel:
- **Sprint 1–2**: Foundation, core systems, enemies
- **Sprint 3–4**: Endgame, polish, QA, build exe (T-63)
- **Sprint 5 (tuần 9–10)**: Chat scope — wave spawn (T-81), Firestore schema (T-84), Google auth (T-83), map decor (T-82), hub respawn (T-76)
- **Sprint 6 (tuần 11–12)**: Host lobby polish (T-85), Vampire/boss gaps, Rubric deliverable fill

## Rubric deliverable tối thiểu

Xem sheet **Rubric_TV** trong [`Backlog_PRU213_Group01.xlsx`](../Backlog_PRU213_Group01.xlsx):

Mỗi thành viên cần tối thiểu: **1 map, 1 nhân vật, 3 quái (3 level), 1 boss, 1 popup, 1 canvas, 2 cơ chế đặc biệt**.

Trạng thái theo màu: DONE (xanh) · PROGRESS (vàng) · TODO (đỏ).

---

## Khi cần làm việc liên quan đến hệ thống của người khác

1. **Không sửa file của người kia** — chỉ dùng public API (property, method)
2. **Thảo luận trước** — thống nhất interface/contract
3. **Dùng Event** nếu cần giao tiếp ngược lại (xem [Coding Convention](./05-coding-convention.md#55-giao-tiếp-giữa-các-system--dùng-event))
4. **Nếu cần sửa file của người kia** → tạo PR và tag họ để review

---

## Lịch sync & milestone gợi ý

| Thời gian | Hoạt động |
|-----------|-----------|
| **Hàng ngày** | Pull develop về, merge vào branch của mình |
| **Thứ 2 & Thứ 5** | Mở PR, review, merge vào develop |
| **Cuối tuần** | Test tích hợp toàn bộ, log Issues |
| **Mỗi milestone** | Merge develop vào main, tag release |
