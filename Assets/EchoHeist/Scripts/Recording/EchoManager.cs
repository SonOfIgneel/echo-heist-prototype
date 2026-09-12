using UnityEngine;

namespace EchoHeist
{
    public sealed class EchoManager : MonoBehaviour
    {
        [SerializeField] private EchoPlayback echoPrefab;
        [SerializeField] private Transform echoParent;
        private EchoPlayback _activeEcho;

        public void BeginPlayback(RecordedRun recording, int sourceRunNumber)
        {
            ClearPlayback();
            if (recording == null || !recording.IsValid || echoPrefab == null) return;

            _activeEcho = Instantiate(echoPrefab, echoParent);
            _activeEcho.name = $"Echo_Run_{sourceRunNumber}";
            _activeEcho.Initialize(recording);
        }

        public void ClearPlayback()
        {
            if (_activeEcho == null) return;

            // Disable first so trigger occupants release themselves before deferred destruction.
            _activeEcho.gameObject.SetActive(false);
            Destroy(_activeEcho.gameObject);
            _activeEcho = null;
        }
    }
}
