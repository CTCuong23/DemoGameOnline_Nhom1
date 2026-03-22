using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class FPSMouseLook : NetworkBehaviour
{
    [SerializeField] private float mouseSensitivity = 0.05f;
    [SerializeField] private Transform cameraTransform;

    // Biến mạng đồng bộ góc nhìn lên/xuống cho bản clone
    [Networked, OnChangedRender(nameof(OnPitchChanged))]
    private float NetworkedPitch { get; set; }

    private float _localXRotation = 0f;
    private float _localYRotation = 0f;

    public override void Spawned()
    {
        if (!HasInputAuthority) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Khởi tạo hướng nhìn hiện tại của nhân vật
        _localYRotation = transform.localEulerAngles.y;
        _localXRotation = 0f;
    }

    public override void Render()
    {
        // 1. Chỉ xử lý chuột trên máy của chính người chơi đó
        if (HasInputAuthority && Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

            // Tính toán xoay cục bộ để máy mình mượt ngay lập tức (Prediction)
            _localXRotation -= mouseDelta.y;
            _localXRotation = Mathf.Clamp(_localXRotation, -80f, 80f);
            _localYRotation += mouseDelta.x;

            // Áp dụng ngay cho mình
            cameraTransform.localRotation = Quaternion.Euler(_localXRotation, 0, 0);
            transform.localRotation = Quaternion.Euler(0, _localYRotation, 0);

            // 2. PHẦN QUAN TRỌNG: Gửi góc quay lên Server bằng RPC
            // Vì file NetworkInputData của thầy không có chỗ chứa dữ liệu chuột
            RPC_SetRotation(_localYRotation, _localXRotation);
        }
    }

    // RPC: Gửi từ máy người chơi lên Server
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetRotation(float yaw, float pitch)
    {
        // Server cập nhật vòng xoay thật của nhân vật (Yaw - trái/phải)
        // Network Transform của thầy sẽ tự động đồng bộ hướng này cho mọi người
        transform.localRotation = Quaternion.Euler(0, yaw, 0);

        // Server cập nhật biến mạng để đồng bộ Pitch (lên/xuống)
        NetworkedPitch = pitch;
    }

    private void OnPitchChanged()
    {
        // Hàm này chạy trên máy của người chơi khác (Clone)
        // Giúp cái "đầu" (camera) của bản clone gật lên/xuống theo người chơi thật
        if (!HasInputAuthority && cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(NetworkedPitch, 0, 0);
        }
    }
}