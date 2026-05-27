using HuanXian.Input;
using HuanXian.StateMachine;
using UnityEngine;

namespace HuanXian.Invocation
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerStateMachine))]
    public sealed class DescentController : MonoBehaviour
    {
        [SerializeField] private float previewDuration = 1.5f;
        [SerializeField] private string descentMessage = "齐天大圣降临！切换动作模组";

        private PlayerInputReader _inputReader;
        private PlayerStateMachine _stateMachine;
        private float _remainingTime;
        private bool _isDescending;

        public bool IsDescending => _isDescending;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _stateMachine = GetComponent<PlayerStateMachine>();
        }

        private void Update()
        {
            if (!_isDescending)
            {
                return;
            }

            _remainingTime -= Time.deltaTime;
            if (_remainingTime > 0f)
            {
                return;
            }

            EndDescent();
        }

        public void BeginDescent()
        {
            _remainingTime = previewDuration;
            _isDescending = true;
            Debug.Log(descentMessage);
        }

        public void EndDescent()
        {
            if (!_isDescending)
            {
                return;
            }

            _isDescending = false;
            _stateMachine.ReturnToLocomotion(_inputReader.CurrentFrame.HasMoveInput);
        }
    }
}
