using HuanXian.Combat;
using HuanXian.Invocation;
using HuanXian.StateMachine;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HuanXian.Core
{
    public sealed class QA_DiagnosticBootstrapper : MonoBehaviour
    {
        private const int PanelWidth = 330;
        private const int PanelHeight = 180;

        private static QA_DiagnosticBootstrapper _instance;

        [SerializeField] private GameObject player;

        private PlayerStateMachine _stateMachine;
        private CombatResourceController _resources;
        private InvocationController _invocationController;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            ResolveReferences();
        }

        private void Update()
        {
            if (_stateMachine == null || _resources == null || _invocationController == null)
            {
                ResolveReferences();
            }

            if (WasDebugInvokePressed())
            {
                ForceChargeAndInvoke();
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, PanelWidth, PanelHeight), "HuanXian QA Diagnostics");

            string state = _stateMachine != null ? _stateMachine.CurrentState.ToString() : "Missing PlayerStateMachine";
            string sanity = _resources != null ? FormatValue(_resources.Sanity, _resources.MaxSanity) : "Missing CombatResourceController";
            string summon = _resources != null ? FormatValue(_resources.SummonGauge, _resources.MaxSummonGauge) : "Missing CombatResourceController";

            GUI.Label(new Rect(20, 40, PanelWidth - 20, 22), "State: " + state);
            GUI.Label(new Rect(20, 65, PanelWidth - 20, 22), "Sanity: " + sanity);
            GUI.Label(new Rect(20, 90, PanelWidth - 20, 22), "SummonGauge: " + summon);
            GUI.Label(new Rect(20, 115, PanelWidth - 20, 22), "Hotkey: E = charge + invoke");

            if (GUI.Button(new Rect(20, 140, PanelWidth - 40, 28), "Charge 100 and Invoke"))
            {
                ForceChargeAndInvoke();
            }
        }

        public void ForceChargeAndInvoke()
        {
            ResolveReferences();

            if (_resources == null || _invocationController == null)
            {
                Debug.LogWarning("QA invoke failed: missing CombatResourceController or InvocationController.");
                return;
            }

            _resources.ModifySummonGauge(_resources.MaxSummonGauge);

            if (!_invocationController.TryEnterDescent() && _stateMachine != null && _stateMachine.CurrentState != EPlayerState.Invoke)
            {
                _stateMachine.ForceState(EPlayerState.Idle);
                _invocationController.TryEnterDescent();
            }
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                CharacterController characterController = FindObjectOfType<CharacterController>();
                if (characterController != null)
                {
                    player = characterController.gameObject;
                }
            }

            if (player == null)
            {
                return;
            }

            if (_stateMachine == null)
            {
                _stateMachine = player.GetComponent<PlayerStateMachine>();
            }

            if (_resources == null)
            {
                _resources = player.GetComponent<CombatResourceController>();
            }

            if (_invocationController == null)
            {
                _invocationController = player.GetComponent<InvocationController>();
            }
        }

        private static string FormatValue(float current, float max)
        {
            return Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max);
        }

        private static bool WasDebugInvokePressed()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                return true;
            }
#endif

#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return false;
#endif
        }
    }
}
