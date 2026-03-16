using UnityEngine;
using Fusion;

public class PlayerCameraSetup : NetworkBehaviour
{
    public override void Spawned()
    {
        if (!HasInputAuthority) return;

        var cameraFollow = FindAnyObjectByType<CameraFollow>();

        if (cameraFollow != null)
        {
            cameraFollow.AssignCamera(transform);
        }
    }
}
