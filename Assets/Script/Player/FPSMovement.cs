using Fusion;
using UnityEngine;

public class FPSMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    private CharacterController _controller;
    private Animator _animator;
    [Networked] private float _verticalVelocity { get; set; }
    [Networked] public float Speed { get; set; }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<NetworkInputData>(out var input)) return;

        Physics.SyncTransforms();

        // Dùng biến kiểm tra mặt đất ổn định hơn
        bool isGrounded = _controller.isGrounded;

        if (isGrounded)
        {
            _verticalVelocity = -2f;
        }
        else
        {
            // Chỉ áp dụng trọng lực khi thực sự không chạm đất
            _verticalVelocity += Physics.gravity.y * Runner.DeltaTime;
        }

        Vector3 move = input.moveDirection;
        Vector3 finalMove = (move * moveSpeed) + new Vector3(0, _verticalVelocity, 0);

        // Kiểm tra nếu có di chuyển thì mới gọi Move để tránh jitter khi đứng yên
        if (finalMove.sqrMagnitude > 0)
        {
            _controller.Move(finalMove * Runner.DeltaTime);
        }

        Speed = move.magnitude;
        if (_animator != null) _animator.SetFloat("Speed", Speed);
    }
}