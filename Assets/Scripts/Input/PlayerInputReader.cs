using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HuanXian.Input
{
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [Header("Fallback Keyboard Bindings")]
        [SerializeField] private KeyCode lockOnKey = KeyCode.Tab;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode parryKey = KeyCode.F;
        [SerializeField] private KeyCode sprintAndDodgeKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode alternateCrouchKey = KeyCode.C;
        [SerializeField] private KeyCode assistInvokeKey = KeyCode.Q;
        [SerializeField] private KeyCode descentInvokeKey = KeyCode.E;
        [SerializeField] private float sprintThreshold = 0.25f;

        [Header("Mouse Bindings")]
        [SerializeField] private int lightAttackMouseButton = 0;
        [SerializeField] private int heavyAttackMouseButton = 1;

        [Header("Look")]
        [SerializeField] private bool readMouseLook = true;
        [SerializeField] private float mouseLookSensitivity = 1f;

        private Vector2 _move;
        private Vector2 _look;
        private PlayerInputFrame _queuedFrame;
        private PlayerInputFrame _currentFrame;
        private bool _shiftHeld;
        private float _shiftHeldTime;
        private bool _sprintHeld;

        public PlayerInputFrame CurrentFrame => _currentFrame;

        private void Update()
        {
            _currentFrame = ReadFrame();
        }

        public PlayerInputFrame ConsumeFrame()
        {
            PlayerInputFrame consumed = _currentFrame;
            _currentFrame.LockOnPressed = false;
            _currentFrame.LightAttackPressed = false;
            _currentFrame.HeavyAttackPressed = false;
            _currentFrame.DodgePressed = false;
            _currentFrame.ParryPressed = false;
            _currentFrame.JumpPressed = false;
            _currentFrame.CrouchPressed = false;
            _currentFrame.AssistInvokePressed = false;
            _currentFrame.DescentInvokePressed = false;
            return consumed;
        }

        public void SetMove(Vector2 move)
        {
            _move = Vector2.ClampMagnitude(move, 1f);
        }

        public void SetLook(Vector2 look)
        {
            _look = look;
        }

        private PlayerInputFrame ReadFrame()
        {
            PlayerInputFrame polledFrame;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null || Mouse.current != null || Gamepad.current != null)
            {
                polledFrame = ReadInputSystemFrame();
                return MergeAndClearQueuedFrame(polledFrame);
            }
#endif
            polledFrame = ReadLegacyFrame();
            return MergeAndClearQueuedFrame(polledFrame);
        }

        private PlayerInputFrame MergeAndClearQueuedFrame(PlayerInputFrame polledFrame)
        {
            PlayerInputFrame merged = new PlayerInputFrame(
                polledFrame.HasMoveInput ? polledFrame.Move : _move,
                polledFrame.HasLookInput ? polledFrame.Look : _look,
                polledFrame.LockOnPressed || _queuedFrame.LockOnPressed,
                polledFrame.LightAttackPressed || _queuedFrame.LightAttackPressed,
                polledFrame.HeavyAttackPressed || _queuedFrame.HeavyAttackPressed,
                polledFrame.JumpPressed || _queuedFrame.JumpPressed,
                polledFrame.DodgePressed || _queuedFrame.DodgePressed,
                polledFrame.ParryPressed || _queuedFrame.ParryPressed,
                polledFrame.SprintHeld || _queuedFrame.SprintHeld,
                polledFrame.CrouchPressed || _queuedFrame.CrouchPressed,
                polledFrame.AssistInvokePressed || _queuedFrame.AssistInvokePressed,
                polledFrame.DescentInvokePressed || _queuedFrame.DescentInvokePressed);

            _queuedFrame = PlayerInputFrame.Empty;
            return merged;
        }

#if ENABLE_INPUT_SYSTEM
        private PlayerInputFrame ReadInputSystemFrame()
        {
            Vector2 move = ReadInputSystemMove();
            Vector2 look = readMouseLook && Mouse.current != null
                ? Mouse.current.delta.ReadValue() * mouseLookSensitivity
                : _look;

            bool lockOnPressed = WasKeyPressed(Key.Tab);
            bool lightAttackPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool heavyAttackPressed = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
            bool jumpPressed = WasKeyPressed(Key.Space);
            bool dodgePressed = UpdateShiftSprintAndDodge(
                Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame,
                Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed,
                Keyboard.current != null && Keyboard.current.leftShiftKey.wasReleasedThisFrame);
            bool parryPressed = WasKeyPressed(Key.F);
            bool crouchPressed = WasKeyPressed(Key.LeftCtrl) || WasKeyPressed(Key.C);
            bool assistInvokePressed = WasKeyPressed(Key.Q);
            bool descentInvokePressed = WasKeyPressed(Key.E);

            return new PlayerInputFrame(
                move,
                look,
                lockOnPressed,
                lightAttackPressed,
                heavyAttackPressed,
                jumpPressed,
                dodgePressed,
                parryPressed,
                _sprintHeld,
                crouchPressed,
                assistInvokePressed,
                descentInvokePressed);
        }

        private static Vector2 ReadInputSystemMove()
        {
            Vector2 move = Vector2.zero;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                {
                    move.x -= 1f;
                }

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                {
                    move.x += 1f;
                }

                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                {
                    move.y -= 1f;
                }

                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                {
                    move.y += 1f;
                }
            }

            if (Gamepad.current != null)
            {
                move += Gamepad.current.leftStick.ReadValue();
            }

            return Vector2.ClampMagnitude(move, 1f);
        }

        private static bool WasKeyPressed(Key key)
        {
            return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
        }

        public void OnMove(InputValue value)
        {
            SetMove(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            SetLook(value.Get<Vector2>());
        }

        public void OnLockOn(InputValue value)
        {
            _queuedFrame.LockOnPressed |= value.isPressed;
        }

        public void OnLightAttack(InputValue value)
        {
            _queuedFrame.LightAttackPressed |= value.isPressed;
        }

        public void OnHeavyAttack(InputValue value)
        {
            _queuedFrame.HeavyAttackPressed |= value.isPressed;
        }

        public void OnDodge(InputValue value)
        {
            _queuedFrame.DodgePressed |= value.isPressed;
        }

        public void OnJump(InputValue value)
        {
            _queuedFrame.JumpPressed |= value.isPressed;
        }

        public void OnParry(InputValue value)
        {
            _queuedFrame.ParryPressed |= value.isPressed;
        }

        public void OnCrouch(InputValue value)
        {
            _queuedFrame.CrouchPressed |= value.isPressed;
        }

        public void OnAssistInvoke(InputValue value)
        {
            _queuedFrame.AssistInvokePressed |= value.isPressed;
        }

        public void OnDescentInvoke(InputValue value)
        {
            _queuedFrame.DescentInvokePressed |= value.isPressed;
        }
#endif

        private PlayerInputFrame ReadLegacyFrame()
        {
            Vector2 move = Vector2.ClampMagnitude(new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical")), 1f);
            Vector2 look = readMouseLook
                ? new Vector2(UnityEngine.Input.GetAxisRaw("Mouse X"), UnityEngine.Input.GetAxisRaw("Mouse Y")) * mouseLookSensitivity
                : _look;

            bool dodgePressed = UpdateShiftSprintAndDodge(
                UnityEngine.Input.GetKeyDown(sprintAndDodgeKey),
                UnityEngine.Input.GetKey(sprintAndDodgeKey),
                UnityEngine.Input.GetKeyUp(sprintAndDodgeKey));

            return new PlayerInputFrame(
                move,
                look,
                UnityEngine.Input.GetKeyDown(lockOnKey),
                UnityEngine.Input.GetMouseButtonDown(lightAttackMouseButton),
                UnityEngine.Input.GetMouseButtonDown(heavyAttackMouseButton),
                UnityEngine.Input.GetKeyDown(jumpKey),
                dodgePressed,
                UnityEngine.Input.GetKeyDown(parryKey),
                _sprintHeld,
                UnityEngine.Input.GetKeyDown(crouchKey) || UnityEngine.Input.GetKeyDown(alternateCrouchKey),
                UnityEngine.Input.GetKeyDown(assistInvokeKey),
                UnityEngine.Input.GetKeyDown(descentInvokeKey));
        }

        private bool UpdateShiftSprintAndDodge(bool shiftWasPressedThisFrame, bool shiftIsPressed, bool shiftWasReleasedThisFrame)
        {
            if (shiftWasPressedThisFrame)
            {
                _shiftHeld = true;
                _shiftHeldTime = 0f;
                _sprintHeld = false;
                return true;
            }

            if (shiftIsPressed && _shiftHeld)
            {
                _shiftHeldTime += Time.deltaTime;
                if (_shiftHeldTime > sprintThreshold)
                {
                    _sprintHeld = true;
                }
            }

            if (shiftWasReleasedThisFrame)
            {
                _shiftHeld = false;
                _shiftHeldTime = 0f;
                _sprintHeld = false;
            }

            return false;
        }
    }
}
