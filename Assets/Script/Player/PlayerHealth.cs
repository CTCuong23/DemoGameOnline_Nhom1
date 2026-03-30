using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : HealthBase
{
    [Header("World Space UI (Thanh máu trên đầu)")]
    // Kéo Image "HealthBar_Fill" (màu xanh/đỏ) trên đầu nhân vật vào đây
    [SerializeField] private Image healthBarFillImage;
    // Kéo cả cái Canvas chứa thanh máu trên đầu vào đây để game tắt nó đi nếu là bản thân
    [SerializeField] private GameObject overheadCanvas;

    [Header("Screen Space UI (Thanh máu góc trái)")]
    // Không cần kéo, game sẽ tự tìm thanh máu góc trái trên màn hình
    private Image hudHealthFillImage;

    [Header("Cài đặt kháng sát thương")]
    [Tooltip("Thời gian kháng sát thương sau khi bị đánh trúng (tránh bị tụt máu liên tục trong 1 giây)")]
    [SerializeField] private float damageCooldown = 1f;

    private float _lastDamageTime = -999f;
    private float _previousHealth; // MỚI THÊM: Lưu lại máu trước đó để biết lúc nào bị trừ

    private Animator _animator;
    private PlayerMovement _playerMovement;
    private PlayerAttack _playerAttack;
    private CharacterController _characterController;

    private bool _isDead = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAttack = GetComponent<PlayerAttack>();
        _characterController = GetComponent<CharacterController>();
    }

    public override void Spawned()
    {
        base.Spawned();
        _previousHealth = maxHealth; // MỚI THÊM: Gán máu ban đầu

        // Kiểm tra xem đây có phải là nhân vật do BẠN điều khiển không
        if (HasInputAuthority)
        {
            // 1. Tắt thanh máu trên đầu đi vì đây là bản thân
            if (overheadCanvas != null)
            {
                overheadCanvas.SetActive(false);
            }

            // 2. Tìm thanh máu (Image Fill) ở góc trái màn hình
            // BẠN CẦN LÀM BƯỚC NÀY TRÊN SCENE: Chọn cái Image làm thanh máu (cái có chứa Fill Amount) ở góc trái màn hình, đặt Tag cho nó là "HUDHealth"
            GameObject hudObj = GameObject.FindWithTag("HUDHealth");
            if (hudObj != null)
            {
                hudHealthFillImage = hudObj.GetComponent<Image>();
            }
            else
            {
                Debug.LogWarning("Không tìm thấy UI máu có tag HUDHealth trên Scene!");
            }
        }
        else
        {
            // Nếu là người chơi khác, bật thanh máu trên đầu của họ lên
            if (overheadCanvas != null)
            {
                overheadCanvas.SetActive(true);
            }
        }

        // Cập nhật giao diện lần đầu khi vừa spawn
        UpdateHealthBarUI();
    }

    protected override void OnCurrentHealthChanged()
    {
        base.OnCurrentHealthChanged();
        UpdateHealthBarUI();

        // MỚI THÊM: Nếu máu hiện tại nhỏ hơn máu trước đó VÀ chưa chết -> Bị đánh
        if (CurrentHealth < _previousHealth && CurrentHealth > 0 && !_isDead)
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Hit");
            }
        }

        // Cập nhật lại máu trước đó
        _previousHealth = CurrentHealth;

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

        // Chạy animation Die
        if (_animator != null)
        {
            _animator.SetTrigger("Die");
        }

        // Tắt di chuyển và tấn công
        if (_playerMovement != null) _playerMovement.enabled = false;
        if (_playerAttack != null) _playerAttack.enabled = false;
        if (_characterController != null) _characterController.enabled = false;

        Debug.Log("Người chơi đã chết!");
    }

    private void UpdateHealthBarUI()
    {
        if (maxHealth <= 0) return;

        float healthPercent = CurrentHealth / maxHealth;

        // Nếu là nhân vật của BẠN -> Bơm máu vào UI ở góc trái màn hình
        if (HasInputAuthority)
        {
            if (hudHealthFillImage != null)
            {
                hudHealthFillImage.fillAmount = healthPercent;
            }
        }
        // Nếu là nhân vật CỦA NGƯỜI KHÁC -> Bơm máu vào UI trên đầu họ
        else
        {
            if (healthBarFillImage != null)
            {
                healthBarFillImage.fillAmount = healthPercent;
            }
        }
    }

    // Nếu con quái vật có Collider đánh dấu là "Is Trigger" chạm vào nhân vật
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Enemies"))
        {
            var enemyCombat = other.GetComponentInParent<EnemyCombat>();
            if (enemyCombat != null)
            {
                TakeDamageFromMonster(enemyCombat.damageToTarget);
            }
        }
    }

    // Nếu quái vật có Collider vật lí bình thường tông vào nhân vật
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Enemies"))
        {
            var enemyCombat = collision.gameObject.GetComponentInParent<EnemyCombat>();
            if (enemyCombat != null)
            {
                TakeDamageFromMonster(enemyCombat.damageToTarget);
            }
        }
    }

    // Nếu Player dùng CharacterController vô tình đụng trúng con quái
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("Enemies"))
        {
            var enemyCombat = hit.gameObject.GetComponentInParent<EnemyCombat>();
            if (enemyCombat != null)
            {
                TakeDamageFromMonster(enemyCombat.damageToTarget);
            }
        }
    }

    // Hàm công khai...
    public void TakeDamageFromMonster(float damageAmount)
    {
        // Kiểm tra xem đã hết thời gian hồi sát thương chưa
        if (Time.time >= _lastDamageTime + damageCooldown)
        {
            _lastDamageTime = Time.time;
            
            TakeDamage(damageAmount);
        }
    }
}