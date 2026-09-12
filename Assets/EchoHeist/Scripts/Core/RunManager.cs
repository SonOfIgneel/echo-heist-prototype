using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoHeist
{
    public sealed class RunManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string commitTimelineActionName = "Gameplay/CommitTimeline";
        [SerializeField] private string resetRunActionName = "Gameplay/ResetRun";
        [SerializeField] private RunRecorder runRecorder;
        [SerializeField] private EchoManager echoManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GadgetController gadgetController;
        [SerializeField] private GuardAI guardAI;
        [SerializeField] private HeistScore heistScore;
        [SerializeField] private MetaProgression progression;
        [SerializeField] private GadgetSelectionUI gadgetSelectionUI;
        [SerializeField] private PrototypeHUD hud;
        [SerializeField] private MonoBehaviour[] resettableBehaviours;
        [SerializeField, Min(1f)] private float runDuration = 60f;
        [SerializeField, Min(0f)] private float failureResetDelay = 0.75f;

        private InputAction _commitTimelineAction;
        private InputAction _resetRunAction;
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
            _resetRunAction = inputActions != null ? inputActions.FindAction(resetRunActionName, false) : null;
            if (_commitTimelineAction == null) Debug.LogError($"Missing input action '{commitTimelineActionName}'.", this);
            if (_resetRunAction == null) Debug.LogError($"Missing input action '{resetRunActionName}'.", this);

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

            if (_resetRunAction != null)
            {
                _resetRunAction.performed += HandleResetRun;
                _resetRunAction.Enable();
            }
        }

        private void Start()
        {
            if (progression.HasCompletedFirstHeist && progression.SelectedGadget == GadgetType.None)
            {
                _currentRunNumber = 1;
                hud.BeginRun(_currentRunNumber, runDuration, false, false);
                playerController.SetMovementEnabled(false);
                gadgetController.SetUsageEnabled(false);
                SuspendGameplayForPostRun();
                gadgetSelectionUI.ShowSelection();
                return;
            }

            BeginRun(true, false);
        }

        private void OnDisable()
        {
            if (_commitTimelineAction != null)
            {
                _commitTimelineAction.performed -= HandleCommitTimeline;
                _commitTimelineAction.Disable();
            }

            if (_resetRunAction != null)
            {
                _resetRunAction.performed -= HandleResetRun;
                _resetRunAction.Disable();
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
        private void HandleResetRun(InputAction.CallbackContext context) => ResetCurrentRun();

        public void CommitTimeline()
        {
            if (!_runActive || _isTransitioning) return;

            RecordedRun completedRecording = runRecorder.EndRecording();
            _runActive = false;
            if (completedRecording.IsValid)
            {
                echoManager.RetainRecording(completedRecording, _currentRunNumber);
            }

            BeginRun(true, true);
        }

        public void ResetCurrentRun()
        {
            if (!_runActive || _isTransitioning) return;

            runRecorder.EndRecording();
            _runActive = false;
            BeginRun(false, false);
        }

        public void FailCurrentRun()
        {
            if (!_runActive || _isTransitioning) return;

            _runActive = false;
            _isTransitioning = true;
            runRecorder.EndRecording();
            playerController.SetMovementEnabled(false);
            gadgetController.SetUsageEnabled(false);
            hud.ShowStatus("CAUGHT BY GUARD");
            StartCoroutine(RestartAfterFailure());
        }

        public void CompleteRun()
        {
            if (!_runActive || _isTransitioning) return;

            float remainingSeconds = Mathf.Max(0f, runDuration - ElapsedTime);
            ScoreBreakdown breakdown = heistScore.CompleteRun(remainingSeconds);
            runRecorder.EndRecording();
            _runActive = false;
            _isTransitioning = true;
            playerController.SetMovementEnabled(false);
            gadgetController.SetUsageEnabled(false);
            hud.ShowStatus("HEIST COMPLETE");
            SuspendGameplayForPostRun();
            progression.MarkFirstHeistCompleted();
            gadgetSelectionUI.ShowResults(breakdown, progression.SelectedGadget == GadgetType.None);
        }

        public void ContinueAfterGadgetSelection()
        {
            if (progression.SelectedGadget == GadgetType.None) return;

            StartFreshRun();
        }

        public void PlayAgainAfterSuccess()
        {
            if (_runActive) return;

            StartFreshRun();
        }

        private void StartFreshRun()
        {
            echoManager.ClearRetainedRecordings();
            _currentRunNumber = 0;
            BeginRun(true, false);
        }

        private void SuspendGameplayForPostRun()
        {
            guardAI.SetGameplayEnabled(false);
            echoManager.ClearPlayback();
        }

        public void NotifyStatus(string message) => hud.ShowStatus(message);

        private IEnumerator RestartAfterFailure()
        {
            yield return new WaitForSeconds(failureResetDelay);
            BeginRun(false, false);
        }

        private void BeginRun(bool advanceRunNumber, bool timelineRecorded)
        {
            _isTransitioning = true;
            playerController.SetMovementEnabled(false);
            gadgetController.SetUsageEnabled(false);
            guardAI.SetGameplayEnabled(false);
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
            hud.BeginRun(_currentRunNumber, runDuration, echoManager.ActiveEchoCount > 0, timelineRecorded);
            playerController.SetMovementEnabled(true);
            gadgetController.SetUsageEnabled(true);
            guardAI.SetGameplayEnabled(true);
            _isTransitioning = false;
        }
    }
}
