using TMPro;
using UnityEngine;

namespace EchoHeist
{
    public sealed class GadgetSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject selectionContent;
        [SerializeField] private GameObject contractContent;
        [SerializeField] private GameObject completionContent;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contractMessageText;
        [SerializeField] private MetaProgression progression;
        [SerializeField] private RunManager runManager;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void ShowSelection()
        {
            panel.SetActive(true);
            selectionContent.SetActive(true);
            contractContent.SetActive(false);
            completionContent.SetActive(false);
            titleText.text = "CHOOSE YOUR NEXT GADGET";
            Time.timeScale = 0f;
        }

        public void ShowCompletion()
        {
            panel.SetActive(true);
            selectionContent.SetActive(false);
            contractContent.SetActive(false);
            completionContent.SetActive(true);
            titleText.text = "HEIST COMPLETE";
            Time.timeScale = 0f;
        }

        public void SelectPhaseDash() => SelectGadget(GadgetType.PhaseDash);
        public void SelectGhostDecoy() => SelectGadget(GadgetType.GhostDecoy);
        public void SelectOpticalCloak() => SelectGadget(GadgetType.OpticalCloak);

        public void ContinueWithGadget()
        {
            ClosePanel();
            runManager.ContinueAfterGadgetSelection();
        }

        public void PlayAgain()
        {
            ClosePanel();
            runManager.PlayAgainAfterSuccess();
        }

        private void OnDisable()
        {
            if (Time.timeScale == 0f) Time.timeScale = 1f;
        }

        private void SelectGadget(GadgetType gadget)
        {
            progression.SelectGadget(gadget);
            selectionContent.SetActive(false);
            contractContent.SetActive(true);
            completionContent.SetActive(false);
            titleText.text = "NEW CONTRACT UNLOCKED";
            contractMessageText.text = $"Your next heist will begin with {FormatName(gadget)}.";
        }

        private void ClosePanel()
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private static string FormatName(GadgetType gadget)
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
                    return "YOUR SELECTED GADGET";
            }
        }
    }
}
