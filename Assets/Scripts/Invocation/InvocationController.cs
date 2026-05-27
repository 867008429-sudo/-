using HuanXian.Combat;
using HuanXian.Input;
using HuanXian.StateMachine;
using UnityEngine;

namespace HuanXian.Invocation
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(CombatResourceController))]
    [RequireComponent(typeof(DescentController))]
    public sealed class InvocationController : MonoBehaviour
    {
        [SerializeField] private float descentGaugeCost = 100f;

        private PlayerInputReader _inputReader;
        private PlayerStateMachine _stateMachine;
        private CombatResourceController _resources;
        private DescentController _descentController;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _stateMachine = GetComponent<PlayerStateMachine>();
            _resources = GetComponent<CombatResourceController>();
            _descentController = GetComponent<DescentController>();
        }

        private void Update()
        {
            PlayerInputFrame inputFrame = _inputReader.CurrentFrame;

            if (inputFrame.AssistInvokePressed)
            {
                TryCastAssistInvocation();
            }

            if (inputFrame.DescentInvokePressed)
            {
                TryEnterDescent();
            }
        }

        public bool TryCastAssistInvocation()
        {
            if (_stateMachine.IsBusy)
            {
                return false;
            }

            Debug.Log("辅唤协同技能预留：孙悟空分身/如意追云");
            return true;
        }

        public bool TryEnterDescent()
        {
            if (_stateMachine.IsBusy || _resources.SummonGauge < descentGaugeCost)
            {
                return false;
            }

            _resources.ModifySummonGauge(-descentGaugeCost);
            _stateMachine.ForceState(EPlayerState.Invoke);
            _descentController.BeginDescent();
            return true;
        }
    }
}
