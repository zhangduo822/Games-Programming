using UnityEngine;

public class ReplayRecorder2D : MonoBehaviour
{
    [SerializeField] private PlayerController2D target;
    [SerializeField] private float maxDuration = 5f;
    [SerializeField] private float sampleInterval = 0.02f;

    private RecordedTimeline activeTimeline;
    private float elapsed;
    private float sampleTimer;

    public bool IsRecording { get; private set; }
    public float RemainingTime => Mathf.Max(0f, maxDuration - elapsed);
    public PlayerController2D Target => target;

    // Assigns the player whose movement should be recorded.
    public void Configure(PlayerController2D player)
    {
        target = player;
    }

    // Captures player frames while an active recording is running.
    private void Update()
    {
        if (!IsRecording || target == null)
        {
            return;
        }

        elapsed += Time.deltaTime;
        sampleTimer += Time.deltaTime;

        if (sampleTimer >= sampleInterval)
        {
            sampleTimer = 0f;
            Capture();
        }

        if (elapsed >= maxDuration)
        {
            StopRecording();
        }
    }

    // Begins a new movement recording for the configured player.
    public void StartRecording()
    {
        if (target == null)
        {
            return;
        }

        activeTimeline = new RecordedTimeline();
        elapsed = 0f;
        sampleTimer = 0f;
        IsRecording = true;
        Capture();
    }

    // Finishes the current recording and returns its timeline.
    public RecordedTimeline StopRecording()
    {
        if (!IsRecording)
        {
            return activeTimeline;
        }

        IsRecording = false;
        if (activeTimeline != null)
        {
            activeTimeline.Duration = elapsed;
        }
        return activeTimeline;
    }

    // Stores one snapshot of the player's current movement state.
    private void Capture()
    {
        activeTimeline.Frames.Add(new RecordedFrame
        {
            Time = elapsed,
            Position = target.transform.position,
            Velocity = target.Velocity,
            MoveX = target.MoveX,
            JumpPressed = target.JumpPressedThisFrame
        });
    }
}
