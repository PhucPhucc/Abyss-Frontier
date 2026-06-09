# 👥 Phân công công việc & Scene Ownership

> ← [Về trang chính](../GUIDES.md)

---

## Nguyên tắc phân chia để tránh conflict

**❌ Tránh làm:** Hai người cùng sửa một Scene, một Prefab, một Script cùng lúc

**✅ Nên làm:** Phân chia theo hệ thống — mỗi người "sở hữu" một phần rõ ràng

---

## Bảng phân công (nhóm 4–5 người)

| Thành viên | Hệ thống | Files sở hữu | Branch |
|-----------|----------|--------------|--------|
| **A** | Player — movement, combat, health, inventory | `Scripts/Player/` · `Prefabs/Player/` · `Animations/Player/` | `feature/player-system` |
| **B** | Enemy AI — Slime, Skeleton, Boss | `Scripts/Enemy/` · `Prefabs/Enemies/` · `Animations/Enemy/` | `feature/enemy-system` |
| **C** | Dungeon System + Scene layout + Tilemap | `Scripts/Dungeon/` · `Scenes/*.unity` · `Tilemaps/` | `scene/dungeon-floors` |
| **D** | Base Building + ResourceManager | `Scripts/BaseBuilding/` · `Prefabs/Buildings/` · `Scenes/Surface.unity` | `feature/base-building` |
| **E** | UI + Audio + ScriptableObject data | `Scripts/UI/` · `Prefabs/UI/` · `Audio/` · `ScriptableObjects/` | `feature/ui-system` |

---

## Scene Ownership — Quan trọng nhất!

> **Quy tắc vàng:** Chỉ 1 người được sửa 1 Scene tại một thời điểm.

| Scene | Người sở hữu | Người khác muốn sửa → |
|-------|-------------|----------------------|
| `MainMenu.unity` | **E** | Hỏi E trước, đợi E unlock |
| `Surface.unity` | **D** | Hỏi D trước |
| `DungeonFloor1.unity` | **C** | Hỏi C trước |
| `DungeonFloor2.unity` | **C** | Hỏi C trước |
| `DungeonFloor3.unity` | **C** | Hỏi C trước |

**Quy trình khi cần sửa Scene không phải của mình:**
1. Nhắn người sở hữu trên chat: _"Mình cần thêm 1 trigger vào DungeonFloor1, được không?"_
2. Đợi họ xác nhận **không đang sửa**
3. Nhắn `🔒 [LOCK] Mình đang sửa DungeonFloor1.unity`
4. Làm xong → commit → push → nhắn `✅ [UNLOCK] Xong rồi, mọi người pull về`

---

## Prefab Ownership

> Không sửa Base Prefab của người khác — tạo Prefab Variant nếu cần thay đổi nhỏ.

| Prefab | Người sở hữu |
|--------|-------------|
| `Player.prefab` | A |
| `Slime.prefab` · `Skeleton.prefab` · `Boss_Floor1.prefab` | B |
| `House.prefab` · `FarmPlot.prefab` · `Forge.prefab` | D |
| `HPBar.prefab` · `DialogueBox.prefab` | E |

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
- Giao task cho từng người
- Track tiến độ
- Log bug cần fix

**Format tiêu đề Issue:**
```
[FEAT] Implement PlayerController    → Assigned: A
[FEAT] Implement EnemyAI             → Assigned: B
[SCENE] Build DungeonFloor1 layout   → Assigned: C
[FEAT] Implement BuildSystem         → Assigned: D
[UI] Implement HP bar & Inventory UI → Assigned: E
[ART] Create dungeon tileset         → Assigned: C
[BUG] Player falls through floor     → Assigned: A
```

**Tham chiếu Issue khi commit:**
```bash
git commit -m "feat(player): implement WASD movement, closes #1"
# → GitHub tự đóng Issue #1 khi PR được merge
```

---

## Khi cần làm việc liên quan đến hệ thống của người khác

Ví dụ: B (Enemy) cần dùng `PlayerHealth` của A để tính damage

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
