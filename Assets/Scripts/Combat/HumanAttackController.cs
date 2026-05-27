using HuanXian.Input;
using HuanXian.StateMachine;
using UnityEngine;

namespace HuanXian.Combat
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(CombatResourceController))]
    public sealed class HumanAttackController : MonoBehaviour
    {
        [Header("Light Attack")]
        [SerializeField] private float lightSanityCost = 6f;
        [SerializeField] private float lightDamage = 12f;
        [SerializeField] private float lightDuration = 0.82f;
        [SerializeField] private float lightHitTime = 0.32f;
        [SerializeField] private float lightSummonGain = 8f;

        [Header("Heavy Attack")]
        [SerializeField] private float heavySanityCost = 14f;
        [SerializeField] private float heavyDamage = 28f;
        [SerializeField] private float heavyDuration = 1.12f;
        [SerializeField] private float heavyHitTime = 0.52f;
        [SerializeField] private float heavySummonGain = 18f;

        [Header("Hit Detection")]
        [SerializeField] private float hitRadius = 0.65f;
        [SerializeField] private float hitForwardOffset = 1.05f;
        [SerializeField] private float hitHeight = 1f;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Animator Fallback")]
        [SerializeField] private string lightAttackStateName = "Attack_Light";
        [SerializeField] private string heavyAttackStateName = "Attack_Heavy";

        private PlayerInputReader _inputReader;
        private PlayerStateMachine _stateMachine;
        private CombatResourceController _resources;
        private Animator _animator;

        private float _remainingTime;
        private float _hitMoment;
        private float _pendingDamage;
        private float _pendingSummonGain;
        private bool _hitResolved;
        private bool _heavyAttack;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _stateMachine = GetComponent<PlayerStateMachine>();
            _resources = GetComponent<CombatResourceController>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_stateMachine.CurrentState == EPlayerState.Attack)
            {
                TickAttack();
                return;
            }

            PlayerInputFrame inputFrame = _inputReader.CurrentFrame;
            if (inputFrame.LightAttackPressed)
            {
                TryBeginAttack(false);
            }
            else if (inputFrame.HeavyAttackPressed)
            {
                TryBeginAttack(true);
            }
        }

        public bool TryBeginAttack(bool heavyAttack)
        {
            if (!_stateMachine.CanMove)
            {
                return false;
            }

            float sanityCost = heavyAttack ? heavySanityCost : lightSanityCost;
            if (!_resources.TrySpendSanity(sanityCost))
            {
                return false;
            }

            _heavyAttack = heavyAttack;
            _remainingTime = heavyAttack ? heavyDuration : lightDuration;
            _hitMoment = heavyAttack ? heavyHitTime : lightHitTime;
            _pendingDamage = heavyAttack ? heavyDamage : lightDamage;
            _pendingSummonGain = heavyAttack ? heavySummonGain : lightSummonGain;
            _hitResolved = false;

            _stateMachine.ForceState(EPlayerState.Attack);
            TryPlayAttackAnimation(heavyAttack);
            return true;
        }

        private void TickAttack()
        {
            float elapsed = (_heavyAttack ? heavyDuration : lightDuration) - _remainingTime;
            if (!_hitResolved && elapsed >= _hitMoment)
            {
                ResolveHit();
                _hitResolved = true;
            }

            _remainingTime -= Time.deltaTime;
            if (_remainingTime > 0f)
            {
                return;
            }

            _stateMachine.ReturnToLocomotion(_inputReader.CurrentFrame.HasMoveInput);
        }

        private void ResolveHit()
        {
            Vector3 center = transform.position + Vector3.up * hitHeight + transform.forward * hitForwardOffset;
            Collider[] hits = Physics.OverlapSphere(center, hitRadius, hitLayers, QueryTriggerInteraction.Ignore);
            bool hitAnyDamageReceiver = false;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (hit == null || hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                DamageReceiver receiver = hit.GetComponentInParent<DamageReceiver>();
                if (receiver == null || !receiver.IsAlive)
                {
                    continue;
                }

                receiver.ApplyDamage(_pendingDamage);
                hitAnyDamageReceiver = true;
            }

            if (hitAnyDamageReceiver)
            {
                _resources.ModifySummonGauge(_pendingSummonGain);
            }
        }

        private void TryPlayAttackAnimation(bool heavyAttack)
        {
            if (_animator == null)
            {
                return;
            }

            string stateName = heavyAttack ? heavyAttackStateName : lightAttackStateName;
            int stateHash = Animator.StringToHash(stateName);
            if (_animator.HasState(0, stateHash))
            {
                _animator.CrossFade(stateHash, 0.05f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _heavyAttack ? Color.red : Color.cyan;
            Vector3 center = transform.position + Vector3.up * hitHeight + transform.forward * hitForwardOffset;
            Gizmos.DrawWireSphere(center, hitRadius);
        }
    }
}
