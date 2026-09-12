using UnityEngine;

namespace EchoHeist
{
    public sealed class SecurityDoor : MonoBehaviour, IRunResettable
    {
        [SerializeField] private PressurePlate pressurePlate;
        [SerializeField] private Vector3 openLocalOffset = new Vector3(0f, 3f, 0f);
        [SerializeField, Min(0.01f)] private float movementSpeed = 6f;
        [SerializeField] private Renderer indicatorRenderer;
        [SerializeField] private Color closedIndicatorColor = new Color(1f, 0.12f, 0.08f, 1f);
        [SerializeField] private Color openIndicatorColor = new Color(0.2f, 1f, 0.5f, 1f);

        private Vector3 _closedLocalPosition;
        private Vector3 _targetLocalPosition;
        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            _closedLocalPosition = transform.localPosition;
            _targetLocalPosition = _closedLocalPosition;
            _propertyBlock = new MaterialPropertyBlock();
            SetIndicatorColor(closedIndicatorColor);
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
            SetIndicatorColor(closedIndicatorColor);
        }

        private void HandlePlateActivationChanged(bool isActivated)
        {
            _targetLocalPosition = _closedLocalPosition + (isActivated ? openLocalOffset : Vector3.zero);
            SetIndicatorColor(isActivated ? openIndicatorColor : closedIndicatorColor);
        }

        private void SetIndicatorColor(Color color)
        {
            if (indicatorRenderer == null) return;
            indicatorRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_BaseColor", color);
            _propertyBlock.SetColor("_Color", color);
            indicatorRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
