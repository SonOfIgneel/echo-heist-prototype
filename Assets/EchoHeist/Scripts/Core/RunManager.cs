using System.Collections;
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
        [SerializeField, Min(0f)] private float failureResetDelay = 0.75f;

        private InputAction _commitTimelineAction;
        private IRunResettable[] _resettables;
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

        private void Start() => BeginRun(true);

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
            if (!_runActive || _isTransitioning) return;

            RecordedRun completedRecording = runRecorder.EndRecording();
            _runActive = false;
            if (completedRecording.IsValid)
            {
                echoManager.RetainRecording(completedRecording, _currentRunNumber);
            }

            BeginRun(true);
        }

        public void FailCurrentRun()
        {
            if (!_runActive || _isTransitioning) return;

            _runActive = false;
            _isTransitioning = true;
            runRecorder.EndRecording();
            playerController.SetMovementEnabled(false);
            hud.ShowStatus("CAUGHT BY GUARD");
            StartCoroutine(RestartAfterFailure());
        }

        public void CompleteRun()
        {
            if (!_runActive || _isTransitioning) return;

            runRecorder.EndRecording();
            _runActive = false;
            playerController.SetMovementEnabled(false);
            hud.ShowStatus("HEIST COMPLETE");
        }

        public void NotifyStatus(string message) => hud.ShowStatus(message);

        private IEnumerator RestartAfterFailure()
        {
            yield return new WaitForSeconds(failureResetDelay);
            BeginRun(false);
        }

        private void BeginRun(bool advanceRunNumber)
        {
            _isTransitioning = true;
            playerController.SetMovementEnabled(false);
            echoManager.ClearPlayback();

            for (int i = 0; i < _resettables.Length; i++)
            {
                _resettables[i]?.ResetForRun();
            }

            Physics.SyncTransforms();

            if (advanceRunNumber || _currentRunNumber == 0) _currentRunNumber++;
            echoManager.BeginPlayback();

            runRecorder.BeginRecording(runDuration);
            _runStartTime = Time.timeAsDouble;
            _runActive = true;
            hud.BeginRun(_currentRunNumber, runDuration, echoManager.ActiveEchoCount > 0);
            playerController.SetMovementEnabled(true);
            _isTransitioning = false;
        }
    }
}
