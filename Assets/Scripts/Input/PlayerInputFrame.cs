using System;
using UnityEngine;

namespace HuanXian.Input
{
    [Serializable]
    public struct PlayerInputFrame
    {
        public Vector2 Move;
        public Vector2 Look;

        public bool LockOnPressed;
        public bool LightAttackPressed;
        public bool HeavyAttackPressed;
        public bool JumpPressed;
        public bool DodgePressed;
        public bool ParryPressed;
        public bool SprintHeld;
        public bool CrouchPressed;
        public bool AssistInvokePressed;
        public bool DescentInvokePressed;

        public bool HasMoveInput => Move.sqrMagnitude > 0.0001f;
        public bool HasLookInput => Look.sqrMagnitude > 0.0001f;
        public bool HasCombatInput => LightAttackPressed || HeavyAttackPressed || DodgePressed || ParryPressed;
        public bool HasInvocationInput => AssistInvokePressed || DescentInvokePressed;

        public static PlayerInputFrame Empty => default;

        public PlayerInputFrame(
            Vector2 move,
            Vector2 look,
            bool lockOnPressed,
            bool lightAttackPressed,
            bool heavyAttackPressed,
            bool jumpPressed,
            bool dodgePressed,
            bool parryPressed,
            bool sprintHeld,
            bool crouchPressed,
            bool assistInvokePressed,
            bool descentInvokePressed)
        {
            Move = move;
            Look = look;
            LockOnPressed = lockOnPressed;
            LightAttackPressed = lightAttackPressed;
            HeavyAttackPressed = heavyAttackPressed;
            JumpPressed = jumpPressed;
            DodgePressed = dodgePressed;
            ParryPressed = parryPressed;
            SprintHeld = sprintHeld;
            CrouchPressed = crouchPressed;
            AssistInvokePressed = assistInvokePressed;
            DescentInvokePressed = descentInvokePressed;
        }
    }
}
