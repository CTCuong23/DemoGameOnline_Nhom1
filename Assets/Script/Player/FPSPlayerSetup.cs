using UnityEngine;
using Fusion;

public class FPSPlayerSetup : NetworkBehaviour
{
    [SerializeField] private Transform cameraSocket;

    public override void Spawned()
    {
        if (!HasInputAuthority) return;

        // Tìm Main Camera trong Scene
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            // Đưa camera về làm con của CameraSocket
            mainCam.transform.SetParent(cameraSocket);
            mainCam.transform.localPosition = Vector3.zero;
            mainCam.transform.localRotation = Quaternion.identity;

            // Vô hiệu hóa CameraFollow cũ của thầy để không bị xung đột
            var oldFollow = FindAnyObjectByType<CameraFollow>();
            if (oldFollow != null) oldFollow.enabled = false;
        }
    }
}