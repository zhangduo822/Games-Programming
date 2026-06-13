using UnityEngine;

[RequireComponent(typeof(PlayerController2D))]
public class ReplayClone2D : MonoBehaviour
{
    private PlayerController2D controller;
    private RecordedTimeline timeline;
    private int frameIndex;
    private float elapsed;
    private bool playing;
    private bool paused;

    public bool IsActive => playing && !paused;
    public float CurrentTime => elapsed;
    public float TotalDuration => timeline != null ? timeline.Duration : 0f;
    public float Progress => TotalDuration > 0f ? elapsed / TotalDuration : 0f;

    // Sets this object up as a replay-controlled clone.
    private void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        controller.SetReplayMode(true);
        gameObject.tag = "Clone";
    }

    // Advances the clone through its recorded timeline.
    private void Update()
    {
        if (!playing || paused || timeline == null)
        {
            return;
        }

        elapsed += Time.deltaTime;
        while (frameIndex < timeline.Frames.Count && timeline.Frames[frameIndex].Time <= elapsed)
        {
            RecordedFrame frame = timeline.Frames[frameIndex];
            controller.ForceState(frame.Position, frame.Velocity);
            controller.ApplyReplayFrame(frame);
            frameIndex++;
        }

        if (elapsed >= timeline.Duration)
        {
            playing = false;
            controller.ClearReplayInput();
        }
    }

    // Starts playing a recorded timeline from the beginning.
    public void Play(RecordedTimeline recordedTimeline)
    {
        timeline = recordedTimeline;
        frameIndex = 0;
        elapsed = 0f;
        playing = timeline != null && timeline.Frames.Count > 0;
        paused = false;
        controller.ClearReplayInput();
    }

    // Pauses timeline playback without clearing clone state.
    public void Pause()
    {
        paused = true;
    }

    // Resumes timeline playback after a pause.
    public void Resume()
    {
        paused = false;
    }

    // Stops playback and clears the clone's replay inputs.
    public void Stop()
    {
        playing = false;
        paused = false;
        timeline = null;
        frameIndex = 0;
        elapsed = 0f;
        controller.ClearReplayInput();
    }
}
