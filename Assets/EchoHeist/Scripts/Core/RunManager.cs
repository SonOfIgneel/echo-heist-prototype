using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoHeist
{
    public sealed class RunManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string commitTimelineActionName = "Gameplay/CommitTimeline";
        [SerializeField] private RunRecorder runRecorder;
        [SerializeField] private EchoManager echoManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PrototypeHUD hud;
        [SerializeField] private MonoBehaviour[] resettableBehaviours;
        [SerializeField, Min(1f)] private float runDuration = 60f;

        private InputAction _commitTimelineAction;
        private IRunResettable[] _resettables;
        private RecordedRun _completedRecording;
        private double _runStartTime;
        private int _currentRunNumber;
        private bool _runActive;
        private bool _isTransitioning;

        public int CurrentRunNumber => _currentRunNumber;
        public float ElapsedTime => _runActive ? (float)(Time.timeAsDouble - _runStartTime) : 0f;

        private void Awake()
        {
            _commitTimelineAction = inputActions != null ? inputActions.FindAction(commitTimelineActionName, false) : null;
            if (_commitTimelineAction == null) Debug.LogError($"Missing input action '{commitTimelineActionName}'.", this);

            _resettables = new IRunResettable[resettableBehaviours?.Length ?? 0];
            for (int i = 0; i < _resettables.Length; i++)
            {
                _resettables[i] = resettableBehaviours[i] as IRunResettable;
                if (_resettables[i] == null)
                {
                    Debug.LogError($"Resettable entry {i} does not implement {nameof(IRunResettable)}.", resettableBehaviours[i]);
                }
            }
        }

        private void OnEnable()
        {
            if (_commitTimelineAction != null)
            {
                _commitTimelineAction.performed += HandleCommitTimeline;
                _commitTimelineAction.Enable();
            }
        }

        private void Start() => BeginNextRun(null);

        private void OnDisable()
        {
            if (_commitTimelineAction != null)
            {
                _commitTimelineAction.performed -= HandleCommitTimeline;
                _commitTimelineAction.Disable();
            }
        }

        private void Update()
        {
            if (!_runActive) return;

            float remaining = runDuration - ElapsedTime;
            hud.SetTimer(remaining);
            if (remaining <= 0f) CommitTimeline();
        }

        private void HandleCommitTimeline(InputAction.CallbackContext context) => CommitTimeline();

        public void CommitTimeline()
        {
            if (_isTransitioning) return;

            if (_runActive)
            {
                _completedRecording = runRecorder.EndRecording();
                _runActive = false;
            }

            BeginNextRun(_completedRecording);
        }

        public void CompleteRun()
        {
            if (!_runActive || _isTransitioning) return;

            _completedRecording = runRecorder.EndRecording();
            _runActive = false;
            playerController.SetMovementEnabled(false);
            hud.ShowStatus("HEIST COMPLETE");
        }

        public void NotifyStatus(string message) => hud.ShowStatus(message);

        private void BeginNextRun(RecordedRun echoRecording)
        {
            _isTransitioning = true;
            playerController.SetMovementEnabled(false);
            echoManager.ClearPlayback();

            for (int i = 0; i < _resettables.Length; i++)
            {
                _resettables[i]?.ResetForRun();
            }

            Physics.SyncTransforms();

            _currentRunNumber++;
            bool createdEcho = echoRecording != null && echoRecording.IsValid;
            if (createdEcho) echoManager.BeginPlayback(echoRecording, _currentRunNumber - 1);

            runRecorder.BeginRecording(runDuration);
            _runStartTime = Time.timeAsDouble;
            _runActive = true;
            hud.BeginRun(_currentRunNumber, runDuration, createdEcho);
            playerController.SetMovementEnabled(true);
            _isTransitioning = false;
        }
    }
}
