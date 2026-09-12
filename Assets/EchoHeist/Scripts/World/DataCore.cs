using System.Collections;
using UnityEngine;

namespace EchoHeist
{
    [RequireComponent(typeof(Collider))]
    public sealed class DataCore : MonoBehaviour, IRunResettable
    {
        [SerializeField] private GameObject visual;
        [SerializeField] private Collider pickupCollider;
        [SerializeField, Min(0.05f)] private float pickupPulseDuration = 0.18f;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _available;
        private Vector3 _visualInitialScale;

        private void Awake()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            if (pickupCollider == null) pickupCollider = GetComponent<Collider>();
            if (visual != null) _visualInitialScale = visual.transform.localScale;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_available) return;

            PlayerObjectiveState objectiveState = other.GetComponentInParent<PlayerObjectiveState>();
            if (objectiveState == null || !objectiveState.CollectDataCore()) return;

            _available = false;
            pickupCollider.enabled = false;
            if (visual != null) StartCoroutine(AnimatePickup());
        }

        public void ResetForRun()
        {
            StopAllCoroutines();
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
            _available = true;
            pickupCollider.enabled = true;
            if (visual != null)
            {
                visual.transform.localScale = _visualInitialScale;
                visual.SetActive(true);
                Renderer visualRenderer = visual.GetComponentInChildren<Renderer>(true);
                if (visualRenderer != null) visualRenderer.enabled = true;
            }
        }

        private IEnumerator AnimatePickup()
        {
            float elapsed = 0f;
            while (elapsed < pickupPulseDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / pickupPulseDuration);
                float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.45f;
                visual.transform.localScale = _visualInitialScale * scale;
                yield return null;
            }

            visual.transform.localScale = _visualInitialScale;
            if (visual != gameObject) visual.SetActive(false);
            else
            {
                Renderer visualRenderer = GetComponentInChildren<Renderer>();
                if (visualRenderer != null) visualRenderer.enabled = false;
            }
        }
    }
}
