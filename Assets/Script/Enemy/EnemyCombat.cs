using Fusion;
using UnityEngine;

// Đặt làm class thường nhưng có các hàm "virtual" để các con quái xịn hơn có thể Override (chép đè) lên
public class EnemyCombat : NetworkBehaviour
{
    [Header("Combat Settings")]
    [Tooltip("Lượng sát thương con quái vật này gây ra cho người chơi")]
    [SerializeField] public float damageToTarget = 20f;

    // Hàm gọi khi quái vật gây sát thương thành công lên Player
    // Được khai báo `virtual` để Boss sau này có thể kế thừa và viết lại: ví dụ gọi hiệu ứng cháy/độc, hú hét...
    public virtual void DealDamageTo(HealthBase targetHealth)
    {
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damageToTarget);
        }
    }

    // Các biến phụ trợ thêm nếu bạn muốn sau này quái vật có thêm thuộc tính
    // [SerializeField] public float attackRange = 1.5f;
    // [SerializeField] public float attackSpeed = 1f;
}
