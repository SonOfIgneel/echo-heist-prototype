using TMPro;
using UnityEngine;

namespace EchoHeist
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text runText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text controlsText;
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

        public void BeginRun(int runNumber, float duration, bool afterimageCreated)
        {
            _currentRunNumber = runNumber;
            runText.text = $"RUN {runNumber}";
            controlsText.gameObject.SetActive(true);
            SetTimer(duration);
            RefreshObjective();
            statusText.text = afterimageCreated ? "AFTERIMAGE CREATED" : string.Empty;
        }

        public void SetTimer(float remainingSeconds)
        {
            timerText.text = $"TIME: {Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds)):00}";
        }

        public void ShowStatus(string message) => statusText.text = message;

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
                    ? "PRESS R TO RECORD THIS TIMELINE"
                    : "STEP ON THE PRESSURE PLATE";
                return;
            }

            objectiveText.text = "USE YOUR ECHO TO OPEN THE SECURITY DOOR";
        }
    }
}
