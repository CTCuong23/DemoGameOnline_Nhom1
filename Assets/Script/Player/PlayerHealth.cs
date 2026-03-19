using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : HealthBase
{
    // Kéo Image "HealthBar_Fill" (màu xanh/đỏ) vào đây trong Inspector
    [SerializeField] private Image healthBarFillImage;

    public override void Spawned()
    {
        base.Spawned();
        UpdateHealthBarUI();
    }

    protected override void OnCurrentHealthChanged()
    {
        base.OnCurrentHealthChanged();
        UpdateHealthBarUI();
    }

    private void UpdateHealthBarUI()
    {
        if (healthBarFillImage != null && maxHealth > 0)
        {
            healthBarFillImage.fillAmount = CurrentHealth / maxHealth;
        }
    }
}