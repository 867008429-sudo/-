using System;
using UnityEngine;

namespace HuanXian.StateMachine
{
    public sealed class PlayerStateMachine : MonoBehaviour
    {
        [SerializeField] private PlayerStateContext context = new PlayerStateContext();

        public event Action<EPlayerState, EPlayerState> StateChanged;
        public event Action<EPlayerState> StateEntered;
        public event Action<EPlayerState> StateExited;

        public PlayerStateContext Context => context;
        public EPlayerState CurrentState => context.CurrentState;
        public EPlayerState PreviousState => context.PreviousState;
        public bool CanMove => context.CanMove;
        public bool IsBusy => context.IsBusy;

        private void Awake()
        {
            EnterState(context.CurrentState);
        }

        public bool TryChangeState(EPlayerState nextState)
        {
            if (context.CurrentState == nextState)
            {
                return false;
            }

            ChangeState(nextState);
            return true;
        }

        public void ForceState(EPlayerState nextState)
        {
            if (context.CurrentState == nextState)
            {
                EnterState(nextState);
                return;
            }

            ChangeState(nextState);
        }

        public void ReturnToLocomotion(bool hasMoveInput)
        {
            ChangeState(hasMoveInput ? EPlayerState.Move : EPlayerState.Idle);
        }

        private void ChangeState(EPlayerState nextState)
        {
            EPlayerState previous = context.CurrentState;
            ExitState(previous);

            context.PreviousState = previous;
            context.CurrentState = nextState;

            EnterState(nextState);
            StateChanged?.Invoke(previous, nextState);
        }

        private void EnterState(EPlayerState state)
        {
            StateEntered?.Invoke(state);
        }

        private void ExitState(EPlayerState state)
        {
            StateExited?.Invoke(state);
        }
    }
}
