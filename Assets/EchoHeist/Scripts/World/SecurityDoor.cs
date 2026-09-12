using UnityEngine;

namespace EchoHeist
{
    public sealed class SecurityDoor : MonoBehaviour, IRunResettable
    {
        [SerializeField] private PressurePlate pressurePlate;
        [SerializeField] private Vector3 openLocalOffset = new Vector3(0f, 3f, 0f);
        [SerializeField, Min(0.01f)] private float movementSpeed = 6f;

        private Vector3 _closedLocalPosition;
        private Vector3 _targetLocalPosition;

        private void Awake()
        {
            _closedLocalPosition = transform.localPosition;
            _targetLocalPosition = _closedLocalPosition;
        }

        private void OnEnable()
        {
            if (pressurePlate != null) pressurePlate.ActivationChanged += HandlePlateActivationChanged;
        }

        private void OnDisable()
        {
            if (pressurePlate != null) pressurePlate.ActivationChanged -= HandlePlateActivationChanged;
        }

        private void Update()
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetLocalPosition,
                movementSpeed * Time.deltaTime);
        }

        public void ResetForRun()
        {
            _targetLocalPosition = _closedLocalPosition;
            transform.localPosition = _closedLocalPosition;
        }

        private void HandlePlateActivationChanged(bool isActivated)
        {
            _targetLocalPosition = _closedLocalPosition + (isActivated ? openLocalOffset : Vector3.zero);
        }
    }
}
