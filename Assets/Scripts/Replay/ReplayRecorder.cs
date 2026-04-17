using UnityEngine;

public class ReplayRecorder : MonoBehaviour
{
    [SerializeField] private PlayerController targetPlayer;
    [SerializeField] private float maxRecordDuration = 5f;
    [SerializeField] private float sampleInterval = 0.02f;

    private RecordedTimeline activeTimeline;
    private float recordElapsed;
    private float sampleElapsed;

    public bool IsRecording { get; private set; }
    public float MaxRecordDuration => maxRecordDuration;

    private void Update()
    {
        if (!IsRecording || targetPlayer == null)
        {
            return;
        }

        recordElapsed += Time.deltaTime;
        sampleElapsed += Time.deltaTime;

        if (sampleElapsed >= sampleInterval)
        {
            sampleElapsed = 0f;
            CaptureFrame();
        }

        if (recordElapsed >= maxRecordDuration)
        {
            StopRecording();
        }
    }

    public void StartRecording()
    {
        activeTimeline = new RecordedTimeline();
        recordElapsed = 0f;
        sampleElapsed = 0f;
        IsRecording = true;
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
            activeTimeline.Duration = recordElapsed;
        }

        return activeTimeline;
    }

    private void CaptureFrame()
    {
        activeTimeline.Frames.Add(new FrameInput
        {
            Timestamp = recordElapsed,
            Move = targetPlayer.CurrentMoveInput,
            JumpPressed = targetPlayer.JumpPressedThisFrame,
            InteractPressed = targetPlayer.InteractPressedThisFrame
        });
    }
}
