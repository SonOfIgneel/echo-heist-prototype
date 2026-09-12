using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoHeist
{
    public sealed class RunRecorder : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0.01f)] private float sampleInterval = 0.05f;

        private readonly List<RecordedFrame> _frames = new List<RecordedFrame>(1280);
        private double _startTime;
        private double _nextSampleTime;
        private bool _isRecording;

        public void BeginRecording(float maximumDuration)
        {
            int requiredCapacity = Mathf.CeilToInt(maximumDuration / sampleInterval) + 2;
            if (_frames.Capacity < requiredCapacity) _frames.Capacity = requiredCapacity;

            _frames.Clear();
            _startTime = Time.timeAsDouble;
            _nextSampleTime = _startTime;
            _isRecording = true;
            Capture(0f);
            _nextSampleTime = _startTime + sampleInterval;
        }

        private void FixedUpdate()
        {
            if (!_isRecording || target == null) return;

            double now = Time.timeAsDouble;
            if (now < _nextSampleTime) return;

            Capture((float)(now - _startTime));
            _nextSampleTime = now + sampleInterval;
        }

        public RecordedRun EndRecording()
        {
            if (!_isRecording) return new RecordedRun(Array.Empty<RecordedFrame>());

            _isRecording = false;
            float timestamp = (float)(Time.timeAsDouble - _startTime);
            if (_frames.Count == 0 || timestamp - _frames[_frames.Count - 1].Timestamp > 0.001f)
            {
                Capture(timestamp);
            }

            return new RecordedRun(_frames.ToArray());
        }

        private void Capture(float timestamp)
        {
            if (target != null) _frames.Add(new RecordedFrame(timestamp, target.position, target.rotation));
        }
    }
}
