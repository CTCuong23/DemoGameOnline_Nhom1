using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : HealthBase
{
    private Animator _animator;
    private NavMeshAgent _navAgent;
    private EnemyAIInitializer _aiInitializer;
    
    // Lưu lại quyền StateMachine để nếu chết thì tắt
    private Unity.VisualScripting.StateMachine _stateMachine;

    // Cờ đánh dấu đã chết chưa (để tránh gọi hàm OnDeath nhiều lần)
    private bool _isDead = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _aiInitializer = GetComponent<EnemyAIInitializer>();
        _stateMachine = GetComponent<Unity.VisualScripting.StateMachine>();
    }

    [Header("Hit Settings")]
    [Tooltip("Số máu bị trừ mỗi khi người chơi chém hoặc tương tác")]
    [SerializeField] private float hitDamageAmount = 10f;

    public override void Spawned()
    {
        base.Spawned();
        // Thiết lập máu quái vật mặc định
    }

    // Hàm gọi khi bị tương tác ngoài Unity (Ví dụ: Sự kiện từ Physics Raycast, OnTriggerEnter, hoặc UI Button)
    public void InteractHit()
    {
        // Gọi thẳng hàm TakeDamage dùng số máu đã khai báo
        TakeDamage(hitDamageAmount);
    }

    public override void TakeDamage(float amount)
    {
        if (_isDead) return;

        float healthBefore = CurrentHealth;
        
        base.TakeDamage(amount); // Hàm này rốt cuộc sẽ trừ máu và tự gọi OnDeath() trên Server

        if (CurrentHealth < healthBefore && CurrentHealth > 0)
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Hit");
            }
        }
    }

    // Sự kiện máu bị thay đổi (Tất cả Host và Client đều nhận được hàm này của Photon Fusion)
    protected override void OnCurrentHealthChanged()
    {
        base.OnCurrentHealthChanged();

        // Kiểm tra ngay: NẾU MÁU BẰNG HOẶC DƯỚI 0 THÌ GỌI ONDEATH
        if (CurrentHealth <= 0 && !_isDead)
        {
            OnDeath();
        }
    }

    protected override void OnDeath()
    {
        if (_isDead) return;
        _isDead = true;

        base.OnDeath();

        // 1. Chết rổi thì phát Animation ngã ra chết
        if (_animator != null)
        {
            _animator.SetTrigger("Die");
        }

        // 2. Không cho di chuyển nữa (Đứng im tại chỗ)
        if (_navAgent != null)
        {
            _navAgent.isStopped = true;
            _navAgent.enabled = false;
        }

        // 3. Tắt não của quái để ngừng chạy rượt đuổi
        if (_stateMachine != null)
        {
            _stateMachine.enabled = false;
        }
        
        var behaviorAgent = GetComponent("BehaviorAgent") as UnityEngine.Behaviour;
        if (behaviorAgent != null)
        {
            behaviorAgent.enabled = false;
        }

        if (_aiInitializer != null)
        {
            _aiInitializer.enabled = false;
        }

        // 4. Báo cho Fusion là hãy xóa bỏ cục thịt này sau 3 giây
        if (HasStateAuthority)
        {
            Invoke(nameof(DespawnEnemy), 3f);
        }
    }

    private void DespawnEnemy()
    {
        if (Runner != null && Object != null)
        {
            Runner.Despawn(Object);
        }
    }
}
