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

        var cameraFL = FindAnyObjectByType<CameraFL>();
        if (cameraFL != null) cameraFL.AssignCamera(transform);
    }
}
