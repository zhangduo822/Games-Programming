using System.Collections.Generic;

[System.Serializable]
public class RecordedTimeline
{
    public readonly List<RecordedFrame> Frames = new List<RecordedFrame>();
    public float Duration;
}
