using System;
using UnityEngine;

namespace EchoHeist
{
    [Serializable]
    public readonly struct RecordedFrame
    {
        public readonly float Timestamp;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public RecordedFrame(float timestamp, Vector3 position, Quaternion rotation)
        {
            Timestamp = timestamp;
            Position = position;
            Rotation = rotation;
        }
    }
}
