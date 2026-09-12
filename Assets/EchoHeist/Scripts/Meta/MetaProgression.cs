using System;
using UnityEngine;

namespace EchoHeist
{
    public sealed class MetaProgression : MonoBehaviour
    {
        private const string FirstHeistKey = "EchoHeist.HasCompletedFirstHeist";
        private const string SelectedGadgetKey = "EchoHeist.SelectedGadget";
        private const string BestScoreKey = "EchoHeist.BestScore";

        public event Action<GadgetType> SelectedGadgetChanged;

        public bool HasCompletedFirstHeist { get; private set; }
        public GadgetType SelectedGadget { get; private set; }
        public int BestScore { get; private set; }

        private void Awake() => Load();

        public void MarkFirstHeistCompleted()
        {
            if (HasCompletedFirstHeist) return;
            HasCompletedFirstHeist = true;
            PlayerPrefs.SetInt(FirstHeistKey, 1);
            PlayerPrefs.Save();
        }

        public void SelectGadget(GadgetType gadget)
        {
            if (gadget == GadgetType.None) return;
            SelectedGadget = gadget;
            PlayerPrefs.SetInt(SelectedGadgetKey, (int)gadget);
            PlayerPrefs.Save();
            SelectedGadgetChanged?.Invoke(SelectedGadget);
        }

        public bool RecordScore(int score)
        {
            if (score <= BestScore) return false;

            BestScore = score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
            return true;
        }

        public void Load()
        {
            HasCompletedFirstHeist = PlayerPrefs.GetInt(FirstHeistKey, 0) == 1;
            int savedGadget = PlayerPrefs.GetInt(SelectedGadgetKey, 0);
            SelectedGadget = Enum.IsDefined(typeof(GadgetType), savedGadget)
                ? (GadgetType)savedGadget
                : GadgetType.None;
            BestScore = Mathf.Max(0, PlayerPrefs.GetInt(BestScoreKey, 0));
        }

        [ContextMenu("Clear Echo Heist Progression")]
        public void ClearProgressionForTesting()
        {
            PlayerPrefs.DeleteKey(FirstHeistKey);
            PlayerPrefs.DeleteKey(SelectedGadgetKey);
            PlayerPrefs.DeleteKey(BestScoreKey);
            PlayerPrefs.Save();
            HasCompletedFirstHeist = false;
            SelectedGadget = GadgetType.None;
            BestScore = 0;
            SelectedGadgetChanged?.Invoke(SelectedGadget);
        }
    }
}
