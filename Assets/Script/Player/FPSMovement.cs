using Fusion;
using UnityEngine;

public class FPSMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    private CharacterController _controller;
    private Animator _animator;
    [Networked] private float _verticalVelocity { get; set; }

    // Đồng bộ biến Speed để mọi người đều thấy animation của nhau
    [Networked, OnChangedRender(nameof(OnSpeedChanged))]
    public float Speed { get; set; }

    private void OnSpeedChanged()
    {
        if (_animator != null)
            _animator.SetFloat("Speed", Speed);
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<NetworkInputData>(out var input)) return;

        Physics.SyncTransforms();

        bool isGrounded = _controller.isGrounded;
        if (isGrounded) _verticalVelocity = -2f;
        else _verticalVelocity += Physics.gravity.y * Runner.DeltaTime;

        Vector3 move = input.moveDirection;
        Vector3 finalMove = (move * moveSpeed) + new Vector3(0, _verticalVelocity, 0);

        if (finalMove.sqrMagnitude > 0.001f)
        {
            _controller.Move(finalMove * Runner.DeltaTime);
        }

        // Cập nhật Speed để Fusion tự đồng bộ qua hàm OnSpeedChanged cho Proxy
        Speed = move.magnitude;
    }
}