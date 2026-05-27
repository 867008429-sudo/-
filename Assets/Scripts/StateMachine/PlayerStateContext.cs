using System;
using UnityEngine;

namespace HuanXian.StateMachine
{
    public enum EPlayerState
    {
        Idle,
        Move,
        Dodge,
        Parry,
        Attack,
        Invoke
    }

    [Serializable]
    public sealed class PlayerStateContext
    {
        [SerializeField] private EPlayerState currentState = EPlayerState.Idle;
        [SerializeField] private EPlayerState previousState = EPlayerState.Idle;

        public EPlayerState CurrentState
        {
            get => currentState;
            internal set => currentState = value;
        }

        public EPlayerState PreviousState
        {
            get => previousState;
            internal set => previousState = value;
        }

        public bool CanMove => currentState == EPlayerState.Idle || currentState == EPlayerState.Move;
        public bool IsBusy => currentState == EPlayerState.Dodge || currentState == EPlayerState.Parry || currentState == EPlayerState.Attack || currentState == EPlayerState.Invoke;
    }
}
