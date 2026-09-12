using TMPro;
using UnityEngine;

namespace EchoHeist
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        private const string BaseControls =
            "WASD / ARROWS — MOVE\nSPACE — COMMIT TIMELINE\nR — RESET RUN";

        [SerializeField] private TMP_Text runText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text controlsText;
        [SerializeField] private TMP_Text gadgetText;
        [SerializeField] private PlayerObjectiveState objectiveState;
        [SerializeField] private PressurePlate pressurePlate;

        private int _currentRunNumber;

        private void OnEnable()
        {
            if (objectiveState != null) objectiveState.DataCoreStateChanged += HandleDataCoreStateChanged;
            if (pressurePlate != null) pressurePlate.ActivationChanged += HandlePlateActivationChanged;
        }

        private void OnDisable()
        {
            if (objectiveState != null) objectiveState.DataCoreStateChanged -= HandleDataCoreStateChanged;
            if (pressurePlate != null) pressurePlate.ActivationChanged -= HandlePlateActivationChanged;
        }

        public void BeginRun(int runNumber, float duration, bool echoActive, bool timelineRecorded)
        {
            _currentRunNumber = runNumber;
            runText.text = $"RUN {runNumber}";
            controlsText.gameObject.SetActive(true);
            SetTimer(duration);
            RefreshObjective();

            if (timelineRecorded) statusText.text = "TIMELINE RECORDED\nECHO ACTIVE";
            else statusText.text = echoActive ? "ECHO ACTIVE" : string.Empty;
        }

        public void SetTimer(float remainingSeconds)
        {
            timerText.text = $"TIME: {Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds)):00}";
        }

        public void ShowStatus(string message) => statusText.text = message;

        public void SetGadgetEquipped(GadgetType gadget)
        {
            bool equipped = gadget != GadgetType.None;
            controlsText.text = equipped ? BaseControls + "\nE — USE GADGET" : BaseControls;
            gadgetText.gameObject.SetActive(equipped);
            if (equipped) SetGadgetState(gadget, 0);
        }

        public void SetGadgetState(GadgetType gadget, int cooldownSeconds)
        {
            gadgetText.text = cooldownSeconds > 0
                ? $"{FormatGadgetName(gadget)} — {cooldownSeconds}s"
                : $"{FormatGadgetName(gadget)} — READY";
        }

        private void HandleDataCoreStateChanged(bool hasDataCore)
        {
            RefreshObjective();
            if (hasDataCore) ShowStatus("DATA CORE ACQUIRED");
        }

        private void HandlePlateActivationChanged(bool isActivated)
        {
            RefreshObjective();
        }

        private void RefreshObjective()
        {
            if (objectiveState != null && objectiveState.HasDataCore)
            {
                objectiveText.text = "OBJECTIVE: REACH EXTRACTION";
                return;
            }

            if (_currentRunNumber <= 1)
            {
                objectiveText.text = pressurePlate != null && pressurePlate.IsActivated
                    ? "PRESS SPACE TO COMMIT THIS TIMELINE"
                    : "STEP ON THE PRESSURE PLATE";
                return;
            }

            objectiveText.text = "USE YOUR ECHO TO OPEN THE SECURITY DOOR";
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
                    return "NO GADGET";
            }
        }
    }
}
