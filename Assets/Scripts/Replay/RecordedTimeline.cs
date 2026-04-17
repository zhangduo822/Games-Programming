using System.Collections.Generic;

[System.Serializable]
public class RecordedTimeline
{
    public List<FrameInput> Frames = new List<FrameInput>();
    public float Duration;
}
