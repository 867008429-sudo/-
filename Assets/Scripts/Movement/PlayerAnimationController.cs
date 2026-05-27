using HuanXian.Input;
using HuanXian.StateMachine;
using UnityEngine;

namespace HuanXian.Movement
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        [Header("Locomotion")]
        [SerializeField] private float walkAnimationScale = 1f;
        [SerializeField] private float sprintAnimationScale = 1.35f;
        [SerializeField] private float speedDampTime = 0.12f;

        [Header("Animator Parameters")]
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string motionSpeedParameter = "MotionSpeed";
        [SerializeField] private string groundedParameter = "Grounded";
        [SerializeField] private string jumpParameter = "Jump";
        [SerializeField] private string freeFallParameter = "FreeFall";
        [SerializeField] private string transformTriggerParameter = "TriggerTransform";

        [Header("Dodge States")]
        [SerializeField] private string dodgeLeftStateName = "Dodge_Left";
        [SerializeField] private string dodgeRightStateName = "Dodge_Right";
        [SerializeField] private string dodgeBackwardStateName = "Dodge_Backward";
        [SerializeField] private float dodgeDirectionThreshold = 0.35f;

        [Header("Invoke Feedback")]
        [SerializeField] private Color invokeColor = Color.yellow;

        private Animator _animator;
        private PlayerInputReader _inputReader;
        private CharacterMotor _motor;
        private PlayerStateMachine _stateMachine;
        private Renderer[] _renderers;

        private int _speedHash;
        private int _motionSpeedHash;
        private int _groundedHash;
        private int _jumpHash;
        private int _freeFallHash;
        private int _transformTriggerHash;
        private bool _hasSpeedParameter;
        private bool _hasMotionSpeedParameter;
        private bool _hasGroundedParameter;
        private bool _hasJumpParameter;
        private bool _hasFreeFallParameter;
        private bool _hasTransformTriggerParameter;
        private EPlayerState _lastState;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _inputReader = GetComponent<PlayerInputReader>();
            _motor = GetComponent<CharacterMotor>();
            _stateMachine = GetComponent<PlayerStateMachine>();
            _renderers = GetComponentsInChildren<Renderer>(true);

            CacheAnimatorParameters();
            _lastState = _stateMachine.CurrentState;
        }

        private void OnEnable()
        {
            if (_stateMachine != null)
            {
                _stateMachine.StateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_stateMachine != null)
            {
                _stateMachine.StateChanged -= HandleStateChanged;
            }
        }

        private void Update()
        {
            UpdateLocomotionParameters();

            if (_stateMachine.CurrentState != _lastState)
            {
                HandleStateChanged(_lastState, _stateMachine.CurrentState);
            }
        }

        private void UpdateLocomotionParameters()
        {
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(_motor.Velocity, Vector3.up);
            PlayerInputFrame inputFrame = _inputReader.CurrentFrame;
            float speedScale = inputFrame.SprintHeld ? sprintAnimationScale : walkAnimationScale;
            float animationSpeed = horizontalVelocity.magnitude * speedScale;

            if (_hasSpeedParameter)
            {
                _animator.SetFloat(_speedHash, animationSpeed, speedDampTime, Time.deltaTime);
            }

            if (_hasMotionSpeedParameter)
            {
                _animator.SetFloat(_motionSpeedHash, inputFrame.HasMoveInput ? 1f : 0f);
            }

            if (_hasGroundedParameter)
            {
                _animator.SetBool(_groundedHash, _motor.IsGrounded);
            }

            if (_hasJumpParameter)
            {
                _animator.SetBool(_jumpHash, inputFrame.JumpPressed && _motor.IsGrounded);
            }

            if (_hasFreeFallParameter)
            {
                _animator.SetBool(_freeFallHash, !_motor.IsGrounded && _motor.VerticalVelocity < 0f);
            }
        }

        private void HandleStateChanged(EPlayerState previousState, EPlayerState nextState)
        {
            _lastState = nextState;

            if (nextState != EPlayerState.Invoke)
            {
                if (nextState == EPlayerState.Dodge)
                {
                    TryPlayDodgeAnimation();
                }

                return;
            }

            ApplyInvokeMaterialFeedback();

            if (_hasTransformTriggerParameter)
            {
                _animator.SetTrigger(_transformTriggerHash);
            }
        }

        private void TryPlayDodgeAnimation()
        {
            Vector2 moveInput = _inputReader.CurrentFrame.Move;
            if (moveInput.x <= -dodgeDirectionThreshold && TryCrossFadeState(dodgeLeftStateName))
            {
                return;
            }

            if (moveInput.x >= dodgeDirectionThreshold && TryCrossFadeState(dodgeRightStateName))
            {
                return;
            }

            if (TryCrossFadeState(dodgeBackwardStateName))
            {
                return;
            }

            if (TryCrossFadeState("Dodge"))
            {
                return;
            }

            TryCrossFadeState("Roll");
        }

        private bool TryCrossFadeState(string stateName)
        {
            int stateHash = Animator.StringToHash(stateName);
            if (!_animator.HasState(0, stateHash))
            {
                return false;
            }

            _animator.CrossFade(stateHash, 0.1f);
            return true;
        }

        private void ApplyInvokeMaterialFeedback()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer targetRenderer = _renderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                Material[] materials = targetRenderer.materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    ApplyColor(materials[j], invokeColor);
                }
            }
        }

        private static void ApplyColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private void CacheAnimatorParameters()
        {
            _speedHash = Animator.StringToHash(speedParameter);
            _motionSpeedHash = Animator.StringToHash(motionSpeedParameter);
            _groundedHash = Animator.StringToHash(groundedParameter);
            _jumpHash = Animator.StringToHash(jumpParameter);
            _freeFallHash = Animator.StringToHash(freeFallParameter);
            _transformTriggerHash = Animator.StringToHash(transformTriggerParameter);

            AnimatorControllerParameter[] parameters = _animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.nameHash == _speedHash && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasSpeedParameter = true;
                }
                else if (parameter.nameHash == _motionSpeedHash && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasMotionSpeedParameter = true;
                }
                else if (parameter.nameHash == _groundedHash && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    _hasGroundedParameter = true;
                }
                else if (parameter.nameHash == _jumpHash && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    _hasJumpParameter = true;
                }
                else if (parameter.nameHash == _freeFallHash && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    _hasFreeFallParameter = true;
                }
                else if (parameter.nameHash == _transformTriggerHash && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    _hasTransformTriggerParameter = true;
                }
            }
        }
    }
}
