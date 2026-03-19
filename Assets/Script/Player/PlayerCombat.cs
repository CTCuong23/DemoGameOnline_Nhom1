using Fusion;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
    }

    public override void FixedUpdateNetwork()
    {
        // Lấy Input từ NetworkInputData mà file của thầy đã truyền lên
        if (GetInput<NetworkInputData>(out var input))
        {
            // Nếu người chơi nhấn phím "-" và mình đang là Server/Host
            if (input.isMinusKeyPressed && HasStateAuthority)
            {
                // Trừ máu theo thời gian thực (ví dụ 20 máu 1 giây)
                _playerHealth.TakeDamage(20f * Runner.DeltaTime);
            }
        }
    }
}