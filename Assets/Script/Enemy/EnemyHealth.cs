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

    public override void Spawned()
    {
        base.Spawned();
        // Thiết lập máu quái vật mặc định
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

        // 1. Chết rổi thì phát Animation ngã ra chết, phải xoá sạch các lệnh Attack tồn đọng
        if (_animator != null)
        {
            // Tắt Root Motion để tránh việc hẩu hết các animation lỗi lôi quái vật dịch chuyển về tọa độ 0,0,0
            _animator.applyRootMotion = false;
            _animator.ResetTrigger("Attack");
            _animator.ResetTrigger("Hit");
            _animator.SetTrigger("Die");
        }

        // 2. Tắt hẳn hệ thống NavMeshAgent để xác quái vật rớt thẳng xuống đất (không bị lơ lửng trên không)
        if (_navAgent != null)
        {
            _navAgent.enabled = false;
        }

        // 3. Tắt não của quái để ngừng chạy rượt đuổi
        if (_stateMachine != null)
        {
            _stateMachine.enabled = false;
        }
        
        // Quét tất cả các script trên quái vật để tắt mọi thứ liên quan đến não bộ AI (đặc biệt là BehaviorGraph của Unity 6)
        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            if (comp == null) continue;
            string compType = comp.GetType().Name;
            if (compType.Contains("BehaviorAgent") || compType.Contains("BehaviorGraph"))
            {
                comp.enabled = false;
            }
        }

        if (_aiInitializer != null)
        {
            _aiInitializer.enabled = false;
        }

        // Tắt luôn tính năng gây sát thương (EnemyCombat) để xác chết không cắn được người
        var enemyCombat = GetComponent<EnemyCombat>();
        if (enemyCombat != null)
        {
            enemyCombat.enabled = false;
        }

        // Tắt va chạm (Collider) và Khóa Vật lý (Rigidbody) để cái xác nằm im trên sàn 
        // không bị rớt tọt qua gầm trái đất và không cản đường người chơi
        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        //// Ép xác chết dính xuống mặt đất (Khắc phục lỗi Animation có tâm ở giữa nên nằm xoay bị nổi lơ lửng)
        //// Bắn 1 tia từ trên đầu xuống để đo khoảng cách tới mặt đất thực sự
        //if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 5f))
        //{
        //    // Kéo toàn bộ mô hình xuống sát mặt đất (cộng 0.1f cho khỏi bị lún)
        //    transform.position = new Vector3(transform.position.x, hit.point.y + -0.5f, transform.position.z);
        //}

        // 4. Báo cho mạng Fusion xóa bỏ quái vật này sau 4 giây (dùng Coroutine để chắc chắn)
        if (HasStateAuthority)
        {
            Debug.Log("Quái vật chết! Bắt đầu đếm ngược 4 giây để xóa...");
            StartCoroutine(DelayDespawnRoutine(4f));
        }
    }

    // Hàm Coroutine chờ thời gian rồi mới xóa (thay cho Invoke cũ)
    private System.Collections.IEnumerator DelayDespawnRoutine(float delaySeconds)
    {
        // Chờ chạy hết số giây yêu cầu
        yield return new WaitForSeconds(delaySeconds);

        Debug.Log("Thời gian 4 giây đã hết! Tiến hành xóa (Despawn) khỏi mạng.");
        // Xóa khỏi hệ thống Multiplayer
        if (Runner != null && Object != null)
        {
            Runner.Despawn(Object);
        }
    }
}
