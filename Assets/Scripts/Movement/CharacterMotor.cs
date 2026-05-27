using UnityEngine;

namespace HuanXian.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterMotor : MonoBehaviour
    {
        [Header("Gravity")]
        [SerializeField] private float gravity = -15f;
        [SerializeField] private float terminalVelocity = -53f;
        [SerializeField] private float groundedStickVelocity = -2f;

        private CharacterController _controller;
        private Vector3 _lastMoveDelta;
        private float _verticalVelocity;

        public CharacterController Controller => _controller;
        public Vector3 Velocity => _controller != null ? _controller.velocity : Vector3.zero;
        public Vector3 LastMoveDelta => _lastMoveDelta;
        public float VerticalVelocity => _verticalVelocity;
        public bool IsGrounded => _controller != null && _controller.isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public CollisionFlags Move(Vector3 worldDelta)
        {
            _lastMoveDelta = worldDelta;
            return _controller.Move(worldDelta);
        }

        public CollisionFlags MoveHorizontal(Vector3 worldDirection, float speed, float deltaTime)
        {
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
            if (horizontalDirection.sqrMagnitude > 1f)
            {
                horizontalDirection.Normalize();
            }

            Vector3 delta = horizontalDirection * (speed * deltaTime);
            return Move(delta);
        }

        public CollisionFlags MoveWithVerticalVelocity(Vector3 horizontalVelocity, float deltaTime)
        {
            Vector3 horizontalDelta = Vector3.ProjectOnPlane(horizontalVelocity, Vector3.up) * deltaTime;
            Vector3 verticalDelta = Vector3.up * (_verticalVelocity * deltaTime);
            return Move(horizontalDelta + verticalDelta);
        }

        public void SetVerticalVelocity(float velocity)
        {
            _verticalVelocity = velocity;
        }

        public void AddVerticalVelocity(float velocity)
        {
            _verticalVelocity += velocity;
        }

        public void ApplyGravity(float deltaTime)
        {
            if (IsGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = groundedStickVelocity;
                return;
            }

            _verticalVelocity = Mathf.Max(_verticalVelocity + gravity * deltaTime, terminalVelocity);
        }

        public void ResetVerticalVelocity()
        {
            _verticalVelocity = 0f;
        }

        public void Teleport(Vector3 worldPosition)
        {
            bool wasEnabled = _controller.enabled;
            _controller.enabled = false;
            transform.position = worldPosition;
            _controller.enabled = wasEnabled;
            _lastMoveDelta = Vector3.zero;
            _verticalVelocity = 0f;
        }
    }
}
