using HuanXian.Combat;
using HuanXian.Input;
using HuanXian.Movement;
using UnityEngine;

namespace HuanXian.StateMachine
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(CombatResourceController))]
    [RequireComponent(typeof(CharacterMotor))]
    public sealed class DodgeState : MonoBehaviour
    {
        [SerializeField] private float sanityCost = 15f;
        [SerializeField] private float duration = 0.55f;
        [SerializeField] private float dodgeSpeed = 6.5f;
        [SerializeField] private string animatorTrigger = "TriggerDodge";
        [SerializeField] private string dodgeLeftStateName = "Dodge_Left";
        [SerializeField] private string dodgeRightStateName = "Dodge_Right";
        [SerializeField] private string dodgeBackwardStateName = "Dodge_Backward";
        [SerializeField] private float dodgeDirectionThreshold = 0.35f;

        private PlayerInputReader _inputReader;
        private PlayerStateMachine _stateMachine;
        private CombatResourceController _resources;
        private CharacterMotor _motor;
        private Animator _animator;
        private int _triggerHash;
        private bool _hasTriggerParameter;
        private float _remainingTime;
        private Vector3 _dodgeDirection;
        private string _currentDodgeAnimationStateName;

        public string CurrentDodgeAnimationStateName => _currentDodgeAnimationStateName;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _stateMachine = GetComponent<PlayerStateMachine>();
            _resources = GetComponent<CombatResourceController>();
            _motor = GetComponent<CharacterMotor>();
            _animator = GetComponent<Animator>();
            _triggerHash = Animator.StringToHash(animatorTrigger);
            _hasTriggerParameter = HasTriggerParameter(_animator, _triggerHash);
        }

        private void Update()
        {
            if (_stateMachine.CurrentState == EPlayerState.Dodge)
            {
                TickDodge();
                return;
            }

            TryEnterDodge();
        }

        private void TryEnterDodge()
        {
            PlayerInputFrame inputFrame = _inputReader.CurrentFrame;
            if (!inputFrame.DodgePressed || !_stateMachine.CanMove)
            {
                return;
            }

            if (!_resources.TrySpendSanity(sanityCost))
            {
                return;
            }

            _remainingTime = duration;
            _dodgeDirection = ResolveDodgeDirection(inputFrame.Move);
            _currentDodgeAnimationStateName = ResolveDodgeAnimationState(inputFrame.Move);
            _stateMachine.ForceState(EPlayerState.Dodge);

            if (_animator != null && _hasTriggerParameter)
            {
                _animator.SetTrigger(_triggerHash);
            }
        }

        private void TickDodge()
        {
            _motor.MoveHorizontal(_dodgeDirection, dodgeSpeed, Time.deltaTime);
            _remainingTime -= Time.deltaTime;
            if (_remainingTime > 0f)
            {
                return;
            }

            _stateMachine.ReturnToLocomotion(_inputReader.CurrentFrame.HasMoveInput);
        }

        private static bool HasTriggerParameter(Animator animator, int parameterHash)
        {
            if (animator == null)
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.nameHash == parameterHash && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector3 ResolveDodgeDirection(Vector2 moveInput)
        {
            Vector3 direction = Vector3.zero;
            Camera mainCamera = Camera.main;
            Vector3 camForward = mainCamera != null ? mainCamera.transform.forward : transform.forward;
            Vector3 camRight = mainCamera != null ? mainCamera.transform.right : transform.right;

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

            if (moveInput.sqrMagnitude > 0.0001f)
            {
                direction = camForward * moveInput.y + camRight * moveInput.x;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = transform.forward;
            }

            direction.y = 0f;
            return direction.normalized;
        }

        private string ResolveDodgeAnimationState(Vector2 moveInput)
        {
            if (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y) && Mathf.Abs(moveInput.x) >= dodgeDirectionThreshold)
            {
                return moveInput.x < 0f ? dodgeLeftStateName : dodgeRightStateName;
            }

            if (moveInput.y <= -dodgeDirectionThreshold)
            {
                return dodgeBackwardStateName;
            }

            return dodgeBackwardStateName;
        }
    }
}
