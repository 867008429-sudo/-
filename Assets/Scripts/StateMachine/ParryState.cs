using HuanXian.Combat;
using HuanXian.Input;
using UnityEngine;

namespace HuanXian.StateMachine
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(CombatResourceController))]
    public sealed class ParryState : MonoBehaviour
    {
        [SerializeField] private float sanityCost = 10f;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private string animatorTrigger = "TriggerParry";

        private PlayerInputReader _inputReader;
        private PlayerStateMachine _stateMachine;
        private CombatResourceController _resources;
        private Animator _animator;
        private int _triggerHash;
        private bool _hasTriggerParameter;
        private float _remainingTime;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _stateMachine = GetComponent<PlayerStateMachine>();
            _resources = GetComponent<CombatResourceController>();
            _animator = GetComponent<Animator>();
            _triggerHash = Animator.StringToHash(animatorTrigger);
            _hasTriggerParameter = HasTriggerParameter(_animator, _triggerHash);
        }

        private void Update()
        {
            if (_stateMachine.CurrentState == EPlayerState.Parry)
            {
                TickParry();
                return;
            }

            TryEnterParry();
        }

        private void TryEnterParry()
        {
            PlayerInputFrame inputFrame = _inputReader.CurrentFrame;
            if (!inputFrame.ParryPressed || !_stateMachine.CanMove)
            {
                return;
            }

            if (!_resources.TrySpendSanity(sanityCost))
            {
                return;
            }

            _stateMachine.ForceState(EPlayerState.Parry);
            _remainingTime = duration;

            if (_animator != null && _hasTriggerParameter)
            {
                _animator.SetTrigger(_triggerHash);
            }
        }

        private void TickParry()
        {
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
    }
}
