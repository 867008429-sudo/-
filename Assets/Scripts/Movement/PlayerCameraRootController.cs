using HuanXian.Input;
using UnityEngine;

namespace HuanXian.Movement
{
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerCameraRootController : MonoBehaviour
    {
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private string cameraRootName = "PlayerCameraRoot";
        [SerializeField] private float mouseSensitivity = 0.12f;
        [SerializeField] private float gamepadSensitivity = 45f;
        [SerializeField] private float topClamp = 70f;
        [SerializeField] private float bottomClamp = -30f;
        [SerializeField] private bool lockCameraPosition;

        private PlayerInputReader _inputReader;
        private float _yaw;
        private float _pitch;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();

            if (cameraRoot == null)
            {
                Transform found = transform.Find(cameraRootName);
                if (found != null)
                {
                    cameraRoot = found;
                }
            }
        }

        private void Start()
        {
            if (cameraRoot == null)
            {
                return;
            }

            Vector3 euler = cameraRoot.rotation.eulerAngles;
            _yaw = euler.y;
            _pitch = NormalizeAngle(euler.x);
        }

        private void LateUpdate()
        {
            if (cameraRoot == null || lockCameraPosition)
            {
                return;
            }

            Vector2 look = _inputReader.CurrentFrame.Look;
            if (look.sqrMagnitude > 0.0001f)
            {
                float deltaTimeMultiplier = IsMouseLook(look) ? mouseSensitivity : gamepadSensitivity * Time.deltaTime;
                _yaw += look.x * deltaTimeMultiplier;
                _pitch -= look.y * deltaTimeMultiplier;
            }

            _yaw = ClampAngle(_yaw, float.MinValue, float.MaxValue);
            _pitch = ClampAngle(_pitch, bottomClamp, topClamp);

            cameraRoot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private static bool IsMouseLook(Vector2 look)
        {
            return Mathf.Abs(look.x) > 1f || Mathf.Abs(look.y) > 1f;
        }

        private static float NormalizeAngle(float angle)
        {
            if (angle > 180f)
            {
                angle -= 360f;
            }

            return angle;
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
            {
                angle += 360f;
            }

            if (angle > 360f)
            {
                angle -= 360f;
            }

            return Mathf.Clamp(angle, min, max);
        }
    }
}
