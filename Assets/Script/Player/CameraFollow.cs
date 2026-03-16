using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    private CinemachineCamera _cinemachineCamera;
    private CinemachineThirdPersonFollow _thirdPersonFollow;
    private CinemachinePositionComposer _positionComposer;

    public void AssignCamera(Transform target)
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();
        _cinemachineCamera.Target.TrackingTarget = target;
        _thirdPersonFollow = GetComponent<CinemachineThirdPersonFollow>();
        _positionComposer = GetComponent<CinemachinePositionComposer>();
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        float scrollValue = Mouse.current.scroll.y.ReadValue();
        if (scrollValue == 0f) return;

        float normalizedScroll = scrollValue / 120f;

        if(_thirdPersonFollow != null)
        {
            _thirdPersonFollow.CameraDistance -= normalizedScroll;
            _thirdPersonFollow.CameraDistance = Mathf.Clamp(_thirdPersonFollow.CameraDistance, 2f, 15f);
        }
        else if(_positionComposer != null)
        {
            _positionComposer.CameraDistance -= normalizedScroll;
            _positionComposer.CameraDistance = Mathf.Clamp(_positionComposer.CameraDistance, 2f, 15f);
        }
    }
}
