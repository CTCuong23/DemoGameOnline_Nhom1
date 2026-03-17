using Fusion;
using static Unity.Collections.Unicode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

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

        Vector3 finalMove =
            (input.moveDirection * moveSpeed) +
            new Vector3(0f, _verticalVelocity, 0f);

        _controller.Move(finalMove * Runner.DeltaTime);

        Speed = input.moveDirection.magnitude;
    }
}