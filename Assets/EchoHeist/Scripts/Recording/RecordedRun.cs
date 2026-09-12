using System;

namespace EchoHeist
{
    public sealed class RecordedRun
    {
        public RecordedFrame[] Frames { get; }
        public float Duration => Frames.Length == 0 ? 0f : Frames[Frames.Length - 1].Timestamp;
        public bool IsValid => Frames.Length > 0;

        public RecordedRun(RecordedFrame[] frames)
        {
            Frames = frames ?? Array.Empty<RecordedFrame>();
        }
    }
}
