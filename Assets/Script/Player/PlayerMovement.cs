using Fusion;
using static Unity.Collections.Unicode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 10f;

    private Animator _animator;
    private CharacterController _controller;

    [Networked] private float _verticalVelocity { get; set; }

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
        if (!GetInput<NetworkInputData>(out var input))
            return;

        Physics.SyncTransforms();

        if (input.moveDirection.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(input.moveDirection),
                Runner.DeltaTime * 15f
            );

        if (_controller.isGrounded)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += Physics.gravity.y * Runner.DeltaTime;

        // Lựa chọn tốc độ tùy theo việc có đang đè phím chạy không
        float currentSpeed = input.isSprintPressed ? sprintSpeed : walkSpeed;

        Vector3 finalMove =
            (input.moveDirection * currentSpeed) +
            new Vector3(0f, _verticalVelocity, 0f);

        _controller.Move(finalMove * Runner.DeltaTime);

        // Quy chuẩn cho Animator Animator: 0 = Đứng yên, 1 = Đi bộ, 2 = Chạy nhanh
        float targetAnimSpeed = 0f;
        if (input.moveDirection.sqrMagnitude > 0.01f)
        {
            targetAnimSpeed = input.isSprintPressed ? 2f : 1f;
        }
        Speed = targetAnimSpeed;
    }
}