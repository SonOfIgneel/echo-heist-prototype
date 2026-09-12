using UnityEngine;

namespace EchoHeist
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EchoPlayback : MonoBehaviour
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private Renderer echoRenderer;

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
        }

        public void SetVisualTint(Color color)
        {
            if (echoRenderer == null) return;
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();

            echoRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_BaseColor", color);
            _propertyBlock.SetColor("_Color", color);
            echoRenderer.SetPropertyBlock(_propertyBlock);
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
        }
    }
}
