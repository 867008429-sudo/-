using HuanXian.Input;
using UnityEngine;

namespace HuanXian.Movement
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(CharacterController))]
    public sealed class CrouchController : MonoBehaviour
    {
        [SerializeField] private float crouchHeight = 1.05f;
        [SerializeField] private float crouchCenterY = 0.55f;
        [SerializeField] private float transitionSpeed = 10f;

        private PlayerInputReader _inputReader;
        private CharacterController _controller;
        private float _standHeight;
        private Vector3 _standCenter;
        private Vector3 _crouchCenter;
        private bool _isCrouching;

        public bool IsCrouching => _isCrouching;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _controller = GetComponent<CharacterController>();
            _standHeight = _controller.height;
            _standCenter = _controller.center;
            _crouchCenter = new Vector3(_standCenter.x, crouchCenterY, _standCenter.z);
        }

        private void Update()
        {
            if (_inputReader.CurrentFrame.CrouchPressed)
            {
                _isCrouching = !_isCrouching;
            }

            float targetHeight = _isCrouching ? crouchHeight : _standHeight;
            Vector3 targetCenter = _isCrouching ? _crouchCenter : _standCenter;
            float t = Mathf.Clamp01(transitionSpeed * Time.deltaTime);

            _controller.height = Mathf.Lerp(_controller.height, targetHeight, t);
            _controller.center = Vector3.Lerp(_controller.center, targetCenter, t);
        }
    }
}
