using UnityEngine;

namespace EchoHeist
{
    [RequireComponent(typeof(Collider))]
    public sealed class DataCore : MonoBehaviour, IRunResettable
    {
        [SerializeField] private GameObject visual;
        [SerializeField] private Collider pickupCollider;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _available;

        private void Awake()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            if (pickupCollider == null) pickupCollider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_available) return;

            PlayerObjectiveState objectiveState = other.GetComponentInParent<PlayerObjectiveState>();
            if (objectiveState == null || !objectiveState.CollectDataCore()) return;

            _available = false;
            pickupCollider.enabled = false;
            if (visual != null) visual.SetActive(false);
        }

        public void ResetForRun()
        {
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
            _available = true;
            pickupCollider.enabled = true;
            if (visual != null) visual.SetActive(true);
        }
    }
}
