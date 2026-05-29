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

    public void Configure(PlayerController2D player)
    {
        target = player;
    }

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
