using UnityEngine;

namespace EchoHeist
{
    [RequireComponent(typeof(Collider))]
    public sealed class IntelPickup : MonoBehaviour, IRunResettable
    {
        [SerializeField] private HeistScore heistScore;
        [SerializeField] private PrototypeHUD hud;
        [SerializeField] private GameObject visual;
        [SerializeField] private Collider pickupCollider;
        [SerializeField] private string displayName = "INTEL";
        [SerializeField, Min(1)] private int scoreValue = 500;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _available;

        private void Awake()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            if (pickupCollider == null) pickupCollider = GetComponent<Collider>();
            if (visual == null) visual = gameObject;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_available || other.GetComponentInParent<PlayerObjectiveState>() == null) return;
            if (!heistScore.AddIntel(scoreValue)) return;

            _available = false;
            pickupCollider.enabled = false;
            if (visual != gameObject) visual.SetActive(false);
            else
            {
                Renderer pickupRenderer = GetComponentInChildren<Renderer>();
                if (pickupRenderer != null) pickupRenderer.enabled = false;
            }

            hud.ShowStatus($"{displayName} ACQUIRED  +{scoreValue}");
        }

        public void ResetForRun()
        {
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
            _available = true;
            pickupCollider.enabled = true;
            if (visual != gameObject) visual.SetActive(true);

            Renderer pickupRenderer = GetComponentInChildren<Renderer>();
            if (pickupRenderer != null) pickupRenderer.enabled = true;
        }

        private void OnValidate()
        {
            if (pickupCollider == null) pickupCollider = GetComponent<Collider>();
        }
    }
}
