using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class FPSMouseLook : NetworkBehaviour
{
    [SerializeField] private float mouseSensitivity = 0.05f;
    [SerializeField] private Transform cameraTransform;
    private float _xRotation = 0f;

    public override void Spawned()
    {
        if (!HasInputAuthority) return;
        Cursor.lockState = CursorLockMode.Locked;
        _xRotation = 0f;
    }

    // CHUYỂN SANG DÙNG RENDER ĐỂ HẾT GIẬT
    public override void Render()
    {
        if (!HasInputAuthority || Mouse.current == null) return;

        // 1. Xử lý xoay (như cũ)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        _xRotation -= mouseDelta.y * mouseSensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        transform.Rotate(Vector3.up * (mouseDelta.x * mouseSensitivity));
    }
}