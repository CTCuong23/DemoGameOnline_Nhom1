using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    private CharacterController _controller;
    private float _verticalVelocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData input))
            return;

        var moveDirection = new Vector3(input.moveInput.x, 0f, input.moveInput.y).normalized;

        if (_controller.isGrounded)
        {
            _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += Physics.gravity.y * Runner.DeltaTime;
        }

        var finalMove = moveDirection * moveSpeed + new Vector3(0f, _verticalVelocity, 0f);
        _controller.Move(finalMove * Runner.DeltaTime);
    }
}
