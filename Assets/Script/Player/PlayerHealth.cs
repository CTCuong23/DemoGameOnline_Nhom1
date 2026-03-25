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

    public override void Spawned()
    {
        base.Spawned();

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
}