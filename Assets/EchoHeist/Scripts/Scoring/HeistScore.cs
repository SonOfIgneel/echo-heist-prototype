using System;
using UnityEngine;

namespace EchoHeist
{
    public readonly struct ScoreBreakdown
    {
        public int DataCoreScore { get; }
        public int IntelScore { get; }
        public int IntelCollected { get; }
        public int UndetectedBonus { get; }
        public int TimeBonus { get; }
        public int TotalScore { get; }
        public int BestScore { get; }
        public bool IsNewBest { get; }

        public ScoreBreakdown(int dataCoreScore, int intelScore, int intelCollected,
            int undetectedBonus, int timeBonus, int totalScore, int bestScore, bool isNewBest)
        {
            DataCoreScore = dataCoreScore;
            IntelScore = intelScore;
            IntelCollected = intelCollected;
            UndetectedBonus = undetectedBonus;
            TimeBonus = timeBonus;
            TotalScore = totalScore;
            BestScore = bestScore;
            IsNewBest = isNewBest;
        }
    }

    public sealed class HeistScore : MonoBehaviour, IRunResettable
    {
        [SerializeField] private MetaProgression progression;
        [SerializeField, Min(0)] private int dataCoreScore = 1000;
        [SerializeField, Min(0)] private int undetectedBonus = 500;
        [SerializeField, Min(0)] private int timeBonusPerSecond = 10;
        [SerializeField, Min(1)] private int totalIntelCount = 2;

        private bool _completed;

        public event Action<int, int, int> ScoreChanged;
        public int CurrentScore { get; private set; }
        public int IntelScore { get; private set; }
        public int IntelCollected { get; private set; }
        public int TotalIntelCount => totalIntelCount;
        public bool PlayerWasDetected { get; private set; }
        public ScoreBreakdown LastBreakdown { get; private set; }

        public bool AddIntel(int scoreValue)
        {
            if (_completed || scoreValue <= 0) return false;

            IntelCollected++;
            IntelScore += scoreValue;
            CurrentScore = IntelScore;
            ScoreChanged?.Invoke(CurrentScore, IntelCollected, totalIntelCount);
            return true;
        }

        public void MarkPlayerDetected()
        {
            if (_completed) return;
            PlayerWasDetected = true;
        }

        public ScoreBreakdown CompleteRun(float remainingSeconds)
        {
            if (_completed) return LastBreakdown;

            _completed = true;
            int detectionBonus = PlayerWasDetected ? 0 : undetectedBonus;
            int timeBonus = Mathf.Max(0, Mathf.FloorToInt(remainingSeconds)) * timeBonusPerSecond;
            int total = dataCoreScore + IntelScore + detectionBonus + timeBonus;
            bool isNewBest = progression.RecordScore(total);

            CurrentScore = total;
            LastBreakdown = new ScoreBreakdown(dataCoreScore, IntelScore, IntelCollected,
                detectionBonus, timeBonus, total, progression.BestScore, isNewBest);
            ScoreChanged?.Invoke(CurrentScore, IntelCollected, totalIntelCount);
            return LastBreakdown;
        }

        public void ResetForRun()
        {
            _completed = false;
            CurrentScore = 0;
            IntelScore = 0;
            IntelCollected = 0;
            PlayerWasDetected = false;
            LastBreakdown = default;
            ScoreChanged?.Invoke(CurrentScore, IntelCollected, totalIntelCount);
        }
    }
}
