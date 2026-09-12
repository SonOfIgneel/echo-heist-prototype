using System.Collections.Generic;
using UnityEngine;

namespace EchoHeist
{
    public sealed class EchoManager : MonoBehaviour
    {
        private readonly struct RetainedEcho
        {
            public readonly RecordedRun Recording;
            public readonly int SourceRunNumber;

            public RetainedEcho(RecordedRun recording, int sourceRunNumber)
            {
                Recording = recording;
                SourceRunNumber = sourceRunNumber;
            }
        }

        [SerializeField] private EchoPlayback echoPrefab;
        [SerializeField] private Transform echoParent;
        [SerializeField, Range(1, 2)] private int maximumRetainedEchoes = 2;
        [SerializeField] private Color firstEchoTint = new Color(0.1f, 0.95f, 1f, 0.72f);
        [SerializeField] private Color secondEchoTint = new Color(0.05f, 0.55f, 0.8f, 0.62f);

        private readonly List<RetainedEcho> _retainedEchoes = new List<RetainedEcho>(2);
        private readonly EchoPlayback[] _activeEchoes = new EchoPlayback[2];
        private int _activeEchoCount;

        public int RetainedEchoCount => _retainedEchoes.Count;
        public int ActiveEchoCount => _activeEchoCount;

        public void RetainRecording(RecordedRun recording, int sourceRunNumber)
        {
            if (recording == null || !recording.IsValid) return;

            int capacity = Mathf.Clamp(maximumRetainedEchoes, 1, _activeEchoes.Length);
            while (_retainedEchoes.Count >= capacity) _retainedEchoes.RemoveAt(0);
            _retainedEchoes.Add(new RetainedEcho(recording, sourceRunNumber));
        }

        public void BeginPlayback()
        {
            ClearPlayback();

            int count = Mathf.Min(_retainedEchoes.Count, _activeEchoes.Length);
            for (int i = 0; i < count; i++)
            {
                RetainedEcho retained = _retainedEchoes[i];
                EchoPlayback echo = Instantiate(echoPrefab, echoParent);
                echo.name = $"Echo_Run_{retained.SourceRunNumber}";
                echo.Initialize(retained.Recording);
                echo.SetVisualTint(i == 0 ? firstEchoTint : secondEchoTint, i + 1);
                _activeEchoes[_activeEchoCount++] = echo;
            }
        }

        public void ClearPlayback()
        {
            for (int i = 0; i < _activeEchoCount; i++)
            {
                EchoPlayback echo = _activeEchoes[i];
                if (echo != null)
                {
                    echo.gameObject.SetActive(false);
                    Destroy(echo.gameObject);
                }

                _activeEchoes[i] = null;
            }

            _activeEchoCount = 0;
        }

        public void ClearRetainedRecordings()
        {
            ClearPlayback();
            _retainedEchoes.Clear();
        }

        public EchoPlayback GetActiveEcho(int index)
        {
            return index >= 0 && index < _activeEchoCount ? _activeEchoes[index] : null;
        }
    }
}
