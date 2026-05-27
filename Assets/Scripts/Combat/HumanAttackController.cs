using HuanXian.Input;
using HuanXian.Movement;
using HuanXian.StateMachine;
using UnityEngine;

namespace HuanXian.Combat
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(CombatResourceController))]
    [RequireComponent(typeof(CharacterMotor))]
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

        [Header("Feel")]
        [SerializeField] private float inputBufferDuration = 0.22f;
        [SerializeField] private float lightChainWindowStart = 0.42f;
        [SerializeField] private float heavyChainWindowStart = 0.68f;
        [SerializeField] private float lightLungeSpeed = 2.6f;
        [SerializeField] private float heavyLungeSpeed = 1.85f;
        [SerializeField] private float lungeDuration = 0.18f;
        [SerializeField] private float lightAttackDriftSpeed = 1.8f;
        [SerializeField] private float heavyAttackDriftSpeed = 1.15f;
        [SerializeField] private float attackTurnSpeed = 900f;

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
        private CharacterMotor _motor;
        private Animator _animator;

        private float _remainingTime;
        private float _hitMoment;
        private float _attackDuration;
        private float _pendingDamage;
        private float _pendingSummonGain;
        private float _queuedAttackTimer;
        private bool _queuedHeavyAttack;
        private bool _hitResolved;
        private bool _heavyAttack;
        private Vector3 _attackLungeDirection;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _stateMachine = GetComponent<PlayerStateMachine>();
            _resources = GetComponent<CombatResourceController>();
            _motor = GetComponent<CharacterMotor>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            PlayerInputFrame inputFrame = _inputReader.CurrentFrame;
            if (_stateMachine.CurrentState == EPlayerState.Attack)
            {
                BufferAttackInput(inputFrame);
                TickAttack();
                return;
            }

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
            _attackDuration = heavyAttack ? heavyDuration : lightDuration;
            _remainingTime = _attackDuration;
            _hitMoment = heavyAttack ? heavyHitTime : lightHitTime;
            _pendingDamage = heavyAttack ? heavyDamage : lightDamage;
            _pendingSummonGain = heavyAttack ? heavySummonGain : lightSummonGain;
            _queuedAttackTimer = 0f;
            _hitResolved = false;
            _attackLungeDirection = ResolveAttackDirection(_inputReader.CurrentFrame.Move);
            RotateToAttackDirection(_attackLungeDirection);

            _stateMachine.ForceState(EPlayerState.Attack);
            TryPlayAttackAnimation(heavyAttack);
            return true;
        }

        private void TickAttack()
        {
            float elapsed = _attackDuration - _remainingTime;
            if (!_hitResolved && elapsed >= _hitMoment)
            {
                ResolveHit();
                _hitResolved = true;
            }

            ApplyAttackMovement(elapsed);
            TickBufferedAttack(elapsed);

            _remainingTime -= Time.deltaTime;
            if (_remainingTime > 0f)
            {
                return;
            }

            _stateMachine.ReturnToLocomotion(_inputReader.CurrentFrame.HasMoveInput);
        }

        private void BufferAttackInput(PlayerInputFrame inputFrame)
        {
            if (inputFrame.LightAttackPressed)
            {
                _queuedAttackTimer = inputBufferDuration;
                _queuedHeavyAttack = false;
            }
            else if (inputFrame.HeavyAttackPressed)
            {
                _queuedAttackTimer = inputBufferDuration;
                _queuedHeavyAttack = true;
            }
        }

        private void TickBufferedAttack(float elapsed)
        {
            if (_queuedAttackTimer <= 0f)
            {
                return;
            }

            _queuedAttackTimer -= Time.deltaTime;
            float chainWindowStart = _heavyAttack ? heavyChainWindowStart : lightChainWindowStart;
            if (elapsed < chainWindowStart)
            {
                return;
            }

            bool queuedHeavyAttack = _queuedHeavyAttack;
            _stateMachine.ReturnToLocomotion(false);
            TryBeginAttack(queuedHeavyAttack);
        }

        private void ApplyAttackMovement(float elapsed)
        {
            if (elapsed <= lungeDuration && _attackLungeDirection.sqrMagnitude > 0.0001f)
            {
                float lungeSpeed = _heavyAttack ? heavyLungeSpeed : lightLungeSpeed;
                _motor.MoveHorizontal(_attackLungeDirection, lungeSpeed, Time.deltaTime);
                return;
            }

            Vector3 driftDirection = ResolveAttackDirection(_inputReader.CurrentFrame.Move);
            if (_inputReader.CurrentFrame.HasMoveInput)
            {
                RotateSmoothlyToAttackDirection(driftDirection);
                float driftSpeed = _heavyAttack ? heavyAttackDriftSpeed : lightAttackDriftSpeed;
                _motor.MoveHorizontal(driftDirection, driftSpeed, Time.deltaTime);
            }
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

        private Vector3 ResolveAttackDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude <= 0.0001f)
            {
                return transform.forward;
            }

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

            Vector3 direction = camForward.normalized * moveInput.y + camRight.normalized * moveInput.x;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
        }

        private void RotateToAttackDirection(Vector3 attackDirection)
        {
            if (attackDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(attackDirection, Vector3.up);
        }

        private void RotateSmoothlyToAttackDirection(Vector3 attackDirection)
        {
            if (attackDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(attackDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                attackTurnSpeed * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _heavyAttack ? Color.red : Color.cyan;
            Vector3 center = transform.position + Vector3.up * hitHeight + transform.forward * hitForwardOffset;
            Gizmos.DrawWireSphere(center, hitRadius);
        }
    }
}
