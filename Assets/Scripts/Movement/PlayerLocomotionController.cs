using HuanXian.Input;
using HuanXian.StateMachine;
using UnityEngine;

namespace HuanXian.Movement
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(PlayerStateMachine))]
    public sealed class PlayerLocomotionController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float sprintMultiplier = 1.35f;
        [SerializeField] private float crouchMultiplier = 0.45f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -15f;
        [SerializeField] private float rotationSpeed = 720f;

        [Header("Camera")]
        [SerializeField] private Camera movementCamera;

        private PlayerInputReader _inputReader;
        private CharacterMotor _motor;
        private PlayerStateMachine _stateMachine;
        private CrouchController _crouchController;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _motor = GetComponent<CharacterMotor>();
            _stateMachine = GetComponent<PlayerStateMachine>();
            _crouchController = GetComponent<CrouchController>();
        }

        private void Start()
        {
            if (movementCamera == null)
            {
                movementCamera = Camera.main;
            }
        }

        private void Update()
        {
            PlayerInputFrame inputFrame = _inputReader.CurrentFrame;
            Vector3 moveDirection = _stateMachine.CanMove
                ? GetCameraRelativeMoveDirection(inputFrame.Move)
                : Vector3.zero;

            RotateTowardsMoveDirection(moveDirection, Time.deltaTime);
            UpdateLocomotionState(inputFrame, moveDirection);
            TryJump(inputFrame);

            _motor.ApplyGravity(Time.deltaTime);
            float currentSpeed = GetCurrentMoveSpeed(inputFrame);
            Vector3 horizontalVelocity = moveDirection * currentSpeed;
            _motor.MoveWithVerticalVelocity(horizontalVelocity, Time.deltaTime);
        }

        public Vector3 GetCameraRelativeMoveDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            if (movementCamera == null)
            {
                movementCamera = Camera.main;
            }

            Transform cameraTransform = movementCamera != null ? movementCamera.transform : null;
            Vector3 camForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 camRight = cameraTransform != null ? cameraTransform.right : Vector3.right;

            camForward.y = 0f;
            camRight.y = 0f;

            if (camForward.sqrMagnitude <= 0.0001f)
            {
                camForward = transform.forward;
                camForward.y = 0f;
            }

            if (camRight.sqrMagnitude <= 0.0001f)
            {
                camRight = transform.right;
                camRight.y = 0f;
            }

            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;
            moveDirection.y = 0f;

            return moveDirection.sqrMagnitude > 1f ? moveDirection.normalized : moveDirection;
        }

        private void RotateTowardsMoveDirection(Vector3 moveDirection, float deltaTime)
        {
            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * deltaTime);
        }

        private void UpdateLocomotionState(PlayerInputFrame inputFrame, Vector3 moveDirection)
        {
            if (!_stateMachine.CanMove)
            {
                return;
            }

            bool hasMoveInput = inputFrame.Move.sqrMagnitude > 0.0001f && moveDirection.sqrMagnitude > 0.0001f;
            _stateMachine.TryChangeState(hasMoveInput ? EPlayerState.Move : EPlayerState.Idle);
        }

        private void TryJump(PlayerInputFrame inputFrame)
        {
            if (!inputFrame.JumpPressed || !_stateMachine.CanMove || !_motor.IsGrounded)
            {
                return;
            }

            _motor.SetVerticalVelocity(Mathf.Sqrt(jumpHeight * -2f * gravity));
        }

        private float GetCurrentMoveSpeed(PlayerInputFrame inputFrame)
        {
            float currentSpeed = inputFrame.SprintHeld ? moveSpeed * sprintMultiplier : moveSpeed;
            if (_crouchController != null && _crouchController.IsCrouching)
            {
                currentSpeed *= crouchMultiplier;
            }

            return currentSpeed;
        }
    }
}
