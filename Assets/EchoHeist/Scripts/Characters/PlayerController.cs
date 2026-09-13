using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoHeist
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerController : MonoBehaviour, IRunResettable
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string moveActionName = "Gameplay/Move";
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField, Min(0f)] private float turnSpeed = 720f;
        [SerializeField, Min(0f)] private float dashCollisionPadding = 0.08f;

        private InputAction _moveAction;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private Vector3 _lastMovementDirection;
        private PressurePlateActivator _plateActivator;
        private bool _movementEnabled;

        public Vector3 LastMovementDirection => _lastMovementDirection;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            _plateActivator = GetComponent<PressurePlateActivator>();
            _spawnPosition = body.position;
            _spawnRotation = body.rotation;
            _lastMovementDirection = transform.forward;
            _moveAction = inputActions != null ? inputActions.FindAction(moveActionName, false) : null;
            if (_moveAction == null) Debug.LogError($"Missing input action '{moveActionName}'.", this);
        }

        private void OnEnable() => _moveAction?.Enable();

        private void OnDisable()
        {
            _moveAction?.Disable();
            _movementEnabled = false;
        }

private void FixedUpdate()
        {
            if (!_movementEnabled || _moveAction == null) return;

            Vector2 input = ControlModeSelector.IsMobileMode
                ? MobileJoystick.Value
                : _moveAction.ReadValue<Vector2>();

            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 movement = new Vector3(input.x, 0f, input.y);
            body.MovePosition(body.position + movement * (moveSpeed * Time.fixedDeltaTime));

            if (movement.sqrMagnitude > 0.0001f)
            {
                _lastMovementDirection = movement.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(_lastMovementDirection, Vector3.up);
                body.MoveRotation(Quaternion.RotateTowards(body.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
            }
        }

        public void SetMovementEnabled(bool isEnabled)
        {
            _movementEnabled = isEnabled;
            if (!isEnabled)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        public float TryDash(float requestedDistance)
        {
            if (!_movementEnabled || requestedDistance <= 0f) return 0f;

            Vector3 direction = _lastMovementDirection.sqrMagnitude > 0.001f
                ? _lastMovementDirection.normalized
                : transform.forward;

            float allowedDistance = requestedDistance;
            if (body.SweepTest(direction, out RaycastHit hit, requestedDistance, QueryTriggerInteraction.Ignore))
            {
                allowedDistance = Mathf.Max(0f, hit.distance - dashCollisionPadding);
            }

            if (allowedDistance <= 0f) return 0f;
            body.MovePosition(body.position + direction * allowedDistance);
            return allowedDistance;
        }

        public void ResetForRun()
        {
            SetMovementEnabled(false);
            _plateActivator?.ReleaseAllPlates();
            _lastMovementDirection = _spawnRotation * Vector3.forward;

            body.detectCollisions = false;
            body.position = _spawnPosition;
            body.rotation = _spawnRotation;
            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            Physics.SyncTransforms();
            body.detectCollisions = true;
        }

        private void OnValidate()
        {
            if (body == null) body = GetComponent<Rigidbody>();
        }
    }
}
