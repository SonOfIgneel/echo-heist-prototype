using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoHeist
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        private const string BaseControls =
            "WASD / ARROWS — MOVE\nSPACE — SAVE ECHO\nR — RESTART RUN";

        [SerializeField] private TMP_Text runText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text controlsText;
        [SerializeField] private TMP_Text gadgetText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text intelText;
        [SerializeField] private Image screenPulse;
        [SerializeField] private PlayerObjectiveState objectiveState;
        [SerializeField] private PressurePlate pressurePlate;
        [SerializeField] private HeistScore heistScore;

        private int _currentRunNumber;
        private Coroutine _pulseRoutine;

        private void OnEnable()
        {
            if (objectiveState != null) objectiveState.DataCoreStateChanged += HandleDataCoreStateChanged;
            if (pressurePlate != null) pressurePlate.ActivationChanged += HandlePlateActivationChanged;
            if (heistScore != null) heistScore.ScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            if (objectiveState != null) objectiveState.DataCoreStateChanged -= HandleDataCoreStateChanged;
            if (pressurePlate != null) pressurePlate.ActivationChanged -= HandlePlateActivationChanged;
            if (heistScore != null) heistScore.ScoreChanged -= HandleScoreChanged;
        }

        public void BeginRun(int runNumber, float duration, bool echoActive, bool timelineRecorded)
        {
            _currentRunNumber = runNumber;
            runText.text = $"RUN {runNumber}";
            controlsText.gameObject.SetActive(true);
            SetTimer(duration);
            RefreshObjective();
            RefreshScore();

            if (timelineRecorded) ShowEmphasis("TIMELINE RECORDED\nECHO ACTIVE",
                new Color(0.15f, 0.95f, 1f, 1f), 0.15f);
            else
            {
                statusText.color = Color.white;
                statusText.text = echoActive ? "ECHO ACTIVE" : string.Empty;
            }
        }

        public void SetTimer(float remainingSeconds)
        {
            timerText.text = $"TIME: {Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds)):00}";
        }

        public void ShowStatus(string message)
        {
            statusText.color = Color.white;
            statusText.text = message;
        }

        public void ShowDetectionStatus(string message, bool playerDetected)
        {
            ShowEmphasis(message, playerDetected
                ? new Color(1f, 0.12f, 0.08f, 1f)
                : new Color(1f, 0.62f, 0.12f, 1f), playerDetected ? 0.2f : 0.12f);
        }

        public void ShowScoreGain(string message)
        {
            ShowEmphasis(message, new Color(1f, 0.78f, 0.18f, 1f), 0.1f);
        }

public void SetGadgetEquipped(GadgetType gadget)
        {
            bool equipped = gadget != GadgetType.None;
            controlsText.text = equipped
                ? BaseControls + "\nE — USE " + FormatGadgetName(gadget)
                : BaseControls;
            gadgetText.gameObject.SetActive(equipped);
            if (equipped) SetGadgetState(gadget, 0);
        }

public void SetGadgetState(GadgetType gadget, int cooldownSeconds)
        {
            gadgetText.textWrappingMode = TextWrappingModes.NoWrap;
            gadgetText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 600f);
            gadgetText.text = cooldownSeconds > 0
                ? $"SPECIAL ABILITY: {FormatGadgetName(gadget)} - {cooldownSeconds}s"
                : $"SPECIAL ABILITY: {FormatGadgetName(gadget)} - READY";
        }

        private void HandleScoreChanged(int score, int intelCollected, int totalIntel)
        {
            scoreText.text = $"SCORE: {score:0000}";
            intelText.text = $"INTEL: {intelCollected}/{totalIntel}";
            RefreshObjective();
        }

        private void RefreshScore()
        {
            if (heistScore == null) return;
            HandleScoreChanged(heistScore.CurrentScore, heistScore.IntelCollected, heistScore.TotalIntelCount);
        }

        private void HandleDataCoreStateChanged(bool hasDataCore)
        {
            RefreshObjective();
            if (hasDataCore) ShowEmphasis("DATA CORE ACQUIRED",
                new Color(0.2f, 0.9f, 1f, 1f), 0.14f);
        }

        private void HandlePlateActivationChanged(bool isActivated)
        {
            RefreshObjective();
        }

private void RefreshObjective()
        {
            if (objectiveState != null && objectiveState.HasDataCore)
            {
                objectiveText.text = heistScore != null && heistScore.IntelCollected < heistScore.TotalIntelCount
                    ? "REACH EXTRACTION  •  OPTIONAL INTEL FOR SCORE"
                    : "REACH EXTRACTION";
                return;
            }

            if (_currentRunNumber <= 1)
            {
                objectiveText.text = pressurePlate != null && pressurePlate.IsActivated
                    ? (ControlModeSelector.IsMobileMode
                        ? "TAP SAVE ECHO TO RECORD THIS RUN"
                        : "PRESS SPACE TO SAVE THIS RUN AS AN ECHO")
                    : "STEP ON THE PRESSURE PLATE";
                return;
            }

            objectiveText.text = pressurePlate != null && pressurePlate.IsActivated
                ? "STEAL THE DATA CORE  •  INTEL IS OPTIONAL"
                : "USE YOUR ECHO TO OPEN THE SECURITY DOOR";
        }

        private void ShowEmphasis(string message, Color color, float pulseAlpha)
        {
            statusText.text = message;
            statusText.color = color;
            if (screenPulse == null) return;
            if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
            _pulseRoutine = StartCoroutine(PulseScreen(message, color, pulseAlpha));
        }

        private IEnumerator PulseScreen(string message, Color color, float peakAlpha)
        {
            screenPulse.gameObject.SetActive(true);
            float duration = 0.28f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(peakAlpha, 0f, Mathf.Clamp01(elapsed / duration));
                screenPulse.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            screenPulse.color = new Color(color.r, color.g, color.b, 0f);
            screenPulse.gameObject.SetActive(false);
            yield return new WaitForSecondsRealtime(1.05f);
            if (statusText.text == message)
            {
                statusText.text = string.Empty;
                statusText.color = Color.white;
            }
            _pulseRoutine = null;
        }

private static string FormatGadgetName(GadgetType gadget)
        {
            switch (gadget)
            {
                case GadgetType.PhaseDash:
                    return "PHASE DASH";
                case GadgetType.GhostDecoy:
                    return "GHOST DECOY";
                case GadgetType.OpticalCloak:
                    return "OPTICAL CLOAK";
                default:
                    return "NO SPECIAL ABILITY";
            }
        }
    }
}
