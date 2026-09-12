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

        private InputAction _moveAction;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        
        private PressurePlateActivator _plateActivator;
private bool _movementEnabled;

private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            _plateActivator = GetComponent<PressurePlateActivator>();
            _spawnPosition = body.position;
            _spawnRotation = body.rotation;
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

            Vector2 input = _moveAction.ReadValue<Vector2>();
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 movement = new Vector3(input.x, 0f, input.y);
            body.MovePosition(body.position + movement * (moveSpeed * Time.fixedDeltaTime));

            if (movement.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
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

public void ResetForRun()
        {
            SetMovementEnabled(false);
            _plateActivator?.ReleaseAllPlates();

            // Prevent the physics world from observing an intermediate teleport pose.
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
