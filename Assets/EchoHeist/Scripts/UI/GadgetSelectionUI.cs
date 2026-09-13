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
        [SerializeField] private TMP_Text dataCoreScoreText;
        [SerializeField] private TMP_Text intelScoreText;
        [SerializeField] private TMP_Text undetectedBonusText;
        [SerializeField] private TMP_Text timeBonusText;
        [SerializeField] private TMP_Text totalScoreText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private TMP_Text newBestText;
        [SerializeField] private TMP_Text resultActionLabel;
        [SerializeField] private MetaProgression progression;
        [SerializeField] private RunManager runManager;

        private bool _requiresGadgetSelection;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void ShowSelection()
        {
            panel.SetActive(true);
            selectionContent.SetActive(true);
            contractContent.SetActive(false);
            completionContent.SetActive(false);
            titleText.text = "CHOOSE YOUR SPECIAL ABILITY";
            Time.timeScale = 0f;
        }

        public void ShowResults(ScoreBreakdown breakdown, bool requiresGadgetSelection)
        {
            _requiresGadgetSelection = requiresGadgetSelection;
            panel.SetActive(true);
            selectionContent.SetActive(false);
            contractContent.SetActive(false);
            completionContent.SetActive(true);
            titleText.text = "HEIST COMPLETE";
            dataCoreScoreText.text = $"DATA CORE                 +{breakdown.DataCoreScore}";
            intelScoreText.text = $"INTEL COLLECTED          +{breakdown.IntelScore}";
            undetectedBonusText.text = $"UNDETECTED BONUS          +{breakdown.UndetectedBonus}";
            timeBonusText.text = $"TIME BONUS                +{breakdown.TimeBonus}";
            totalScoreText.text = $"TOTAL SCORE               {breakdown.TotalScore}";
            bestScoreText.text = $"BEST SCORE                {breakdown.BestScore}";
            newBestText.gameObject.SetActive(breakdown.IsNewBest);
            newBestText.text = "NEW BEST!";
            resultActionLabel.text = requiresGadgetSelection ? "CONTINUE" : "PLAY AGAIN";
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
            if (_requiresGadgetSelection)
            {
                ShowSelection();
                return;
            }

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
                    return "YOUR SELECTED SPECIAL ABILITY";
            }
        }
    }
}
