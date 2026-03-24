using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // Tìm Camera chính (main camera) trong màn chơi của client hiện tại
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    // Phải dùng LateUpdate để đảm bảo Camera đã cập nhật vị trí/góc xoay xong xuôi, 
    // sau đó UI mới xoay theo. Nếu dùng Update bình thường có thể bị giật (jitter).
    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // Ép góc xoay của thanh máu luôn bằng với góc xoay của Camera
            transform.rotation = mainCameraTransform.rotation;
        }
    }
}