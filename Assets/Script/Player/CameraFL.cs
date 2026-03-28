using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFL : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Camera Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, -4.5f);
    [SerializeField] private float smoothSpeed = 20f; 

    [Header("Collision Settings (Chống lọt tường)")]
    [SerializeField] private LayerMask wallLayer = 1; 
    [SerializeField] private float collisionRadius = 0.3f; 
    [SerializeField] private float heightOffset = 1.5f; 

    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool isActive = false;

    public void AssignCamera(Transform targetPlayer)
    {
        target = targetPlayer;
        isActive = true; 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        rotationX = angles.x;
        rotationY = angles.y;
    }

    void Update()
    {
        if (!isActive || target == null) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void LateUpdate()
    {
        if (!isActive || target == null) return;

        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            rotationY += mouseDelta.x * mouseSensitivity * 0.1f;
            rotationX -= mouseDelta.y * mouseSensitivity * 0.1f;
            rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        }

        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 targetEyePos = target.position + Vector3.up * heightOffset;
        Vector3 desiredPosition = target.position + rotation * offset;

        Vector3 direction = desiredPosition - targetEyePos;
        float distance = direction.magnitude;
        direction.Normalize();

        if (Physics.SphereCast(targetEyePos, collisionRadius, direction, out RaycastHit hit, distance, wallLayer))
        {
            desiredPosition = targetEyePos + direction * hit.distance;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(targetEyePos);
    }
}
