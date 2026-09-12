using System.Collections;
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
        [SerializeField, Min(0.05f)] private float pickupPulseDuration = 0.16f;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _available;
        private Vector3 _initialScale;

        private void Awake()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            if (pickupCollider == null) pickupCollider = GetComponent<Collider>();
            if (visual == null) visual = gameObject;
            _initialScale = visual.transform.localScale;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_available || other.GetComponentInParent<PlayerObjectiveState>() == null) return;
            if (!heistScore.AddIntel(scoreValue)) return;

            _available = false;
            pickupCollider.enabled = false;
            StartCoroutine(AnimatePickup());
            hud.ShowScoreGain($"+{scoreValue} INTEL  •  {displayName}");
        }

        public void ResetForRun()
        {
            StopAllCoroutines();
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
            visual.transform.localScale = _initialScale;
            _available = true;
            pickupCollider.enabled = true;
            if (visual != gameObject) visual.SetActive(true);

            SetRenderersEnabled(true);
        }

        private IEnumerator AnimatePickup()
        {
            float elapsed = 0f;
            while (elapsed < pickupPulseDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / pickupPulseDuration);
                visual.transform.localScale = _initialScale * (1f + Mathf.Sin(progress * Mathf.PI) * 0.5f);
                yield return null;
            }

            visual.transform.localScale = _initialScale;
            if (visual != gameObject) visual.SetActive(false);
            else
            {
                SetRenderersEnabled(false);
            }
        }

        private void SetRenderersEnabled(bool isEnabled)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = isEnabled;
        }

        private void OnValidate()
        {
            if (pickupCollider == null) pickupCollider = GetComponent<Collider>();
        }
    }
}
