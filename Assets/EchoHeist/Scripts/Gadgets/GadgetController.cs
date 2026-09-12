using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoHeist
{
    public sealed class GadgetController : MonoBehaviour, IRunResettable
    {
        [Header("References")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string useGadgetActionName = "Gameplay/UseGadget";
        [SerializeField] private MetaProgression progression;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GuardTarget playerTarget;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private TrailRenderer dashTrail;
        [SerializeField] private GuardTarget ghostDecoyPrefab;
        [SerializeField] private Transform spawnedGadgetParent;
        [SerializeField] private PrototypeHUD hud;

        [Header("Phase Dash")]
        [SerializeField, Min(0.1f)] private float dashDistance = 2.5f;
        [SerializeField, Min(0f)] private float dashCooldown = 5f;
        [SerializeField, Min(0.01f)] private float dashTrailDuration = 0.18f;

        [Header("Ghost Decoy")]
        [SerializeField, Min(0.1f)] private float decoyDuration = 4f;
        [SerializeField, Min(0f)] private float decoyCooldown = 6f;
        [SerializeField, Min(0f)] private float decoyForwardOffset = 0.8f;

        [Header("Optical Cloak")]
        [SerializeField, Min(0.1f)] private float cloakDuration = 3f;
        [SerializeField, Min(0f)] private float cloakCooldown = 6f;
        [SerializeField] private Color cloakColor = new Color(0.35f, 0.45f, 0.75f, 1f);

        private InputAction _useGadgetAction;
        private GadgetType _selectedGadget;
        private GuardTarget _activeDecoy;
        private MaterialPropertyBlock _propertyBlock;
        private Color _normalColor = Color.white;
        private float _nextUseTime;
        private int _lastDisplayedCooldown = -1;
        private bool _usageEnabled;

        public GadgetType SelectedGadget => _selectedGadget;
        public bool IsReady => Time.time >= _nextUseTime;

        private void Awake()
        {
            _useGadgetAction = inputActions != null ? inputActions.FindAction(useGadgetActionName, false) : null;
            if (_useGadgetAction == null) Debug.LogError($"Missing input action '{useGadgetActionName}'.", this);

            _propertyBlock = new MaterialPropertyBlock();
            if (playerRenderer != null && playerRenderer.sharedMaterial != null &&
                playerRenderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                _normalColor = playerRenderer.sharedMaterial.GetColor("_BaseColor");
            }

            RefreshSelectedGadget();
        }

        private void OnEnable()
        {
            if (_useGadgetAction != null)
            {
                _useGadgetAction.performed += HandleUseGadget;
                _useGadgetAction.Enable();
            }

            if (progression != null) progression.SelectedGadgetChanged += HandleSelectedGadgetChanged;
        }

        private void Start() => RefreshSelectedGadget();

        private void OnDisable()
        {
            if (_useGadgetAction != null)
            {
                _useGadgetAction.performed -= HandleUseGadget;
                _useGadgetAction.Disable();
            }

            if (progression != null) progression.SelectedGadgetChanged -= HandleSelectedGadgetChanged;
        }

        private void Update()
        {
            if (_selectedGadget == GadgetType.None) return;

            int cooldown = Mathf.CeilToInt(Mathf.Max(0f, _nextUseTime - Time.time));
            if (cooldown == _lastDisplayedCooldown) return;

            _lastDisplayedCooldown = cooldown;
            hud.SetGadgetState(_selectedGadget, cooldown);
        }

        public void SetUsageEnabled(bool isEnabled) => _usageEnabled = isEnabled;

        public void UseSelectedGadget()
        {
            if (!_usageEnabled || _selectedGadget == GadgetType.None || !IsReady) return;

            switch (_selectedGadget)
            {
                case GadgetType.PhaseDash:
                    _nextUseTime = Time.time + dashCooldown;
                    StartCoroutine(PerformPhaseDash());
                    break;
                case GadgetType.GhostDecoy:
                    _nextUseTime = Time.time + decoyCooldown;
                    SpawnGhostDecoy();
                    break;
                case GadgetType.OpticalCloak:
                    _nextUseTime = Time.time + cloakCooldown;
                    StartCoroutine(PerformOpticalCloak());
                    break;
            }

            _lastDisplayedCooldown = -1;
        }

        public void ResetForRun()
        {
            StopAllCoroutines();
            _nextUseTime = 0f;
            _lastDisplayedCooldown = -1;
            if (dashTrail != null) dashTrail.emitting = false;
            if (playerTarget != null) playerTarget.SetDetectable(true);
            SetPlayerColor(_normalColor);

            if (_activeDecoy != null)
            {
                _activeDecoy.gameObject.SetActive(false);
                Destroy(_activeDecoy.gameObject);
                _activeDecoy = null;
            }

            RefreshSelectedGadget();
        }

        private void HandleUseGadget(InputAction.CallbackContext context) => UseSelectedGadget();

        private void HandleSelectedGadgetChanged(GadgetType gadget)
        {
            _selectedGadget = gadget;
            _lastDisplayedCooldown = -1;
            hud.SetGadgetEquipped(_selectedGadget);
        }

        private void RefreshSelectedGadget()
        {
            _selectedGadget = progression != null ? progression.SelectedGadget : GadgetType.None;
            hud?.SetGadgetEquipped(_selectedGadget);
        }

        private IEnumerator PerformPhaseDash()
        {
            if (dashTrail != null) dashTrail.emitting = true;
            float travelled = playerController.TryDash(dashDistance);
            hud.ShowStatus(travelled > 0.05f ? "PHASE DASH" : "DASH BLOCKED");
            yield return new WaitForSeconds(dashTrailDuration);
            if (dashTrail != null) dashTrail.emitting = false;
        }

        private void SpawnGhostDecoy()
        {
            if (_activeDecoy != null)
            {
                _activeDecoy.gameObject.SetActive(false);
                Destroy(_activeDecoy.gameObject);
            }

            Vector3 direction = playerController.LastMovementDirection;
            Vector3 position = playerController.transform.position + direction * decoyForwardOffset;
            _activeDecoy = Instantiate(ghostDecoyPrefab, position, playerController.transform.rotation, spawnedGadgetParent);
            _activeDecoy.name = "GhostDecoy";
            Destroy(_activeDecoy.gameObject, decoyDuration);
            hud.ShowStatus("GHOST DECOY DEPLOYED");
        }

        private IEnumerator PerformOpticalCloak()
        {
            playerTarget.SetDetectable(false);
            SetPlayerColor(cloakColor);
            hud.ShowStatus("OPTICAL CLOAK ACTIVE");
            yield return new WaitForSeconds(cloakDuration);
            playerTarget.SetDetectable(true);
            SetPlayerColor(_normalColor);
            hud.ShowStatus("OPTICAL CLOAK ENDED");
        }

        private void SetPlayerColor(Color color)
        {
            if (playerRenderer == null) return;
            playerRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_BaseColor", color);
            _propertyBlock.SetColor("_Color", color);
            playerRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
