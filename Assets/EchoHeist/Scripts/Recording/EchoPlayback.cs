using UnityEngine;
using TMPro;

namespace EchoHeist
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EchoPlayback : MonoBehaviour
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private Renderer echoRenderer;
        [SerializeField] private TrailRenderer echoTrail;
        [SerializeField] private TMP_Text identityText;

        private RecordedRun _recording;
        private double _startTime;
        private int _frameIndex;
        private bool _isPlaying;
        private MaterialPropertyBlock _propertyBlock;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            if (echoRenderer == null) echoRenderer = GetComponentInChildren<Renderer>();
            if (echoTrail == null) echoTrail = GetComponentInChildren<TrailRenderer>();
            if (identityText == null) identityText = GetComponentInChildren<TMP_Text>(true);
            _propertyBlock = new MaterialPropertyBlock();
        }

        public void Initialize(RecordedRun recording)
        {
            _recording = recording;
            _frameIndex = 0;
            _startTime = Time.timeAsDouble;
            _isPlaying = recording != null && recording.IsValid;
            if (!_isPlaying) return;

            RecordedFrame firstFrame = recording.Frames[0];
            body.position = firstFrame.Position;
            body.rotation = firstFrame.Rotation;
            transform.SetPositionAndRotation(firstFrame.Position, firstFrame.Rotation);
            if (echoTrail != null)
            {
                echoTrail.Clear();
                echoTrail.emitting = true;
            }
        }

        public void SetVisualTint(Color color, int echoNumber)
        {
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();

            if (echoRenderer != null)
            {
                echoRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_BaseColor", color);
                _propertyBlock.SetColor("_Color", color);
                echoRenderer.SetPropertyBlock(_propertyBlock);
            }

            if (echoTrail != null)
            {
                echoTrail.startColor = new Color(color.r, color.g, color.b, Mathf.Min(0.72f, color.a));
                echoTrail.endColor = new Color(color.r, color.g, color.b, 0f);
            }

            if (identityText != null)
            {
                identityText.text = $"ECHO {echoNumber}";
                identityText.color = new Color(color.r, color.g, color.b, 1f);
            }
        }

        private void FixedUpdate()
        {
            if (!_isPlaying) return;

            RecordedFrame[] frames = _recording.Frames;
            float elapsed = (float)(Time.timeAsDouble - _startTime);

            if (elapsed >= _recording.Duration || _frameIndex >= frames.Length - 1)
            {
                MoveTo(frames[frames.Length - 1].Position, frames[frames.Length - 1].Rotation);
                _isPlaying = false;
                return;
            }

            while (_frameIndex < frames.Length - 2 && frames[_frameIndex + 1].Timestamp <= elapsed)
            {
                _frameIndex++;
            }

            RecordedFrame from = frames[_frameIndex];
            RecordedFrame to = frames[_frameIndex + 1];
            float segmentDuration = Mathf.Max(0.0001f, to.Timestamp - from.Timestamp);
            float interpolation = Mathf.Clamp01((elapsed - from.Timestamp) / segmentDuration);
            MoveTo(Vector3.LerpUnclamped(from.Position, to.Position, interpolation),
                Quaternion.SlerpUnclamped(from.Rotation, to.Rotation, interpolation));
        }

        private void MoveTo(Vector3 position, Quaternion rotation)
        {
            body.MovePosition(position);
            body.MoveRotation(rotation);
        }

        private void OnValidate()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            if (echoRenderer == null) echoRenderer = GetComponentInChildren<Renderer>();
            if (echoTrail == null) echoTrail = GetComponentInChildren<TrailRenderer>();
            if (identityText == null) identityText = GetComponentInChildren<TMP_Text>(true);
        }
    }
}
