using Fusion;
using UnityEngine;

public class HealthBase : NetworkBehaviour
{
    [SerializeField] protected float maxHealth = 100f;

    // Biến đồng bộ mạng, tự gọi hàm OnCurrentHealthChanged khi giá trị thay đổi
    [Networked, OnChangedRender(nameof(OnCurrentHealthChanged))]
    public float CurrentHealth { get; set; }

    public override void Spawned()
    {
        // Chỉ Server mới có quyền khởi tạo máu
        if (HasStateAuthority)
        {
            CurrentHealth = maxHealth;
        }
    }

    public virtual void TakeDamage(float amount)
    {
        // Khóa bảo mật: Chỉ Server mới có quyền trừ máu
        if (!HasStateAuthority) return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0f, maxHealth);

        if (CurrentHealth <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath() { }

    protected virtual void OnCurrentHealthChanged() { }
}