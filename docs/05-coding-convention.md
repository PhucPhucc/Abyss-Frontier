# 💻 Quy tắc viết code C#

> ← [Về trang chính](../GUIDES.md)

> Thống nhất code style giúp nhóm đọc code của nhau dễ dàng và tránh conflict không cần thiết.

---

## 5.1 Naming Convention

```csharp
// ✅ Class: PascalCase
public class PlayerController : MonoBehaviour { }

// ✅ Method: PascalCase
public void TakeDamage(int amount) { }

// ✅ Property: PascalCase
public int CurrentHealth { get; private set; }

// ✅ Private field: _camelCase (có dấu gạch dưới đầu)
private float _moveSpeed = 5f;
private Rigidbody2D _rb;

// ✅ Public field / Constant: PascalCase
public const int MaxHealth = 100;

// ✅ Parameter / Local variable: camelCase
int damageAmount = 10;
```

---

## 5.2 [SerializeField] vs public

```csharp
// ✅ ĐÚNG: Dùng [SerializeField] cho field cần assign trong Inspector
// Không lộ ra ngoài, component khác không thể sửa trực tiếp
[SerializeField] private float _moveSpeed = 5f;
[SerializeField] private Transform _attackPoint;

// ❌ SAI: Dùng public chỉ để thấy trong Inspector
public float moveSpeed = 5f; // Bất kỳ script nào cũng có thể sửa!

// ✅ Nếu script khác cần ĐỌC: dùng property read-only
public float MoveSpeed => _moveSpeed;

// ✅ Nếu script khác cần GHI: dùng method rõ ràng thay vì field public
public void SetMoveSpeed(float speed) { _moveSpeed = speed; }
```

---

## 5.3 Namespace

Mỗi folder Scripts là một namespace — tuân theo cấu trúc thư mục:

```csharp
namespace AbyssFrontier.Player
{
    public class PlayerController : MonoBehaviour { }
    public class PlayerCombat     : MonoBehaviour { }
    public class PlayerHealth     : MonoBehaviour { }
}

namespace AbyssFrontier.Enemy
{
    public class EnemyAI      : MonoBehaviour { }
    public class EnemyHealth  : MonoBehaviour { }
    public class BossController : MonoBehaviour { }
}

namespace AbyssFrontier.Core
{
    public class GameManager : MonoBehaviour { }
    public class SaveSystem  : MonoBehaviour { }
}
```

**Bảng namespace theo folder:**

| Folder | Namespace |
|--------|-----------|
| `Scripts/Player/` | `AbyssFrontier.Player` |
| `Scripts/Enemy/` | `AbyssFrontier.Enemy` |
| `Scripts/Core/` | `AbyssFrontier.Core` |
| `Scripts/UI/` | `AbyssFrontier.UI` |
| `Scripts/Dungeon/` | `AbyssFrontier.Dungeon` |
| `Scripts/BaseBuilding/` | `AbyssFrontier.BaseBuilding` |
| `Scripts/Inventory/` | `AbyssFrontier.Inventory` |

---

## 5.4 Singleton Pattern

Dùng cho `GameManager`, `UIManager`, `ResourceManager`:

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

// Cách dùng từ script khác:
// GameManager.Instance.DoSomething();
```

---

## 5.5 Giao tiếp giữa các System — Dùng Event

```csharp
// ❌ SAI: Script A gọi thẳng method của Script B → tight coupling
// PlayerHealth.cs
public void Die() {
    UIManager.Instance.ShowDeathScreen(); // PlayerHealth biết UIManager → xấu
    EnemySpawner.Instance.StopSpawning(); // PlayerHealth biết EnemySpawner → xấu
}

// ✅ ĐÚNG: Dùng event/delegate → loose coupling
// PlayerHealth.cs
public static event Action OnPlayerDied;

public void Die() {
    OnPlayerDied?.Invoke(); // Chỉ báo "player chết", không cần biết ai đang lắng nghe
}

// UIManager.cs — tự đăng ký và huỷ đăng ký event
private void OnEnable()  { PlayerHealth.OnPlayerDied += ShowDeathScreen; }
private void OnDisable() { PlayerHealth.OnPlayerDied -= ShowDeathScreen; }

private void ShowDeathScreen() { /* hiện màn hình chết */ }
```

> **Lợi ích:** `PlayerHealth` không cần biết `UIManager` tồn tại. Thêm listener mới không cần sửa `PlayerHealth`.

---

## 5.6 Quy tắc Debug.Log

```csharp
// ❌ KHÔNG để lại Debug.Log trần trong code production
Debug.Log("Player health: " + _currentHealth);

// ✅ Bọc trong #if UNITY_EDITOR — tự động bị xóa khi build
#if UNITY_EDITOR
    Debug.Log($"Player health: {_currentHealth}");
#endif

// ✅ Hoặc dùng Conditional attribute — gọn hơn
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void DebugLog(string message) => Debug.Log(message);

// Dùng: DebugLog($"Speed: {_moveSpeed}");
```

---

## 5.7 Prefab Variant — Không sửa Base Prefab của người khác

- **Không sửa trực tiếp Base Prefab** (`Player.prefab`, `Slime.prefab`) nếu bạn không phải người "sở hữu" hệ thống đó → xem [Phân công](./08-team-assignment.md)
- Nếu cần biến thể nhỏ (đổi màu, scale khác): tạo **Prefab Variant** từ Base Prefab
- Mọi thay đổi vào Base Prefab phải **thông báo cho nhóm trước**

---

## 5.8 ScriptableObject — Không hardcode data

```csharp
// ❌ SAI: Hardcode data trực tiếp trong script
public class EnemyAI : MonoBehaviour {
    private int _maxHealth = 50;    // Muốn thay đổi phải sửa code + recompile
    private float _moveSpeed = 2f;
    private int _attackDamage = 10;
}

// ✅ ĐÚNG: Tách data ra ScriptableObject
[CreateAssetMenu(fileName = "EnemyData", menuName = "AbyssFrontier/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    public int MaxHealth    = 50;
    public float MoveSpeed  = 2f;
    public int AttackDamage = 10;
}

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyDataSO _data; // Assign trong Inspector

    private void Start()
    {
        // Dùng: _data.MaxHealth, _data.MoveSpeed, _data.AttackDamage
    }
}
```

**Lợi ích:** Designer/người làm data có thể chỉnh số liệu trực tiếp trong Inspector mà không cần sửa code.

---

## Tóm tắt nhanh

| Rule | Đúng | Sai |
|------|------|-----|
| Field Inspector | `[SerializeField] private` | `public` |
| Field private | `_camelCase` | `camelCase`, `m_name` |
| Class / Method | `PascalCase` | `camelCase`, `snake_case` |
| Giao tiếp System | Events / Delegate | Gọi thẳng Instance |
| Data enemy/building | ScriptableObject | Hardcode trong script |
| Debug | `#if UNITY_EDITOR` | `Debug.Log` trần |
