using UnityEngine;

public class ReplayCloneController : MonoBehaviour, IResettable
{
    [SerializeField] private PlayerController playerController;

    private RecordedTimeline timeline;
    private int frameIndex;
    private float playbackElapsed;
    private Vector3 initialPosition;
    private bool isPlaying;

    private void Awake()
    {
        initialPosition = transform.position;
        if (playerController != null)
        {
            playerController.SetControlMode(PlayerController.ControlMode.Replay);
        }
    }

    private void Update()
    {
        if (!isPlaying || timeline == null || playerController == null)
        {
            return;
        }

        playbackElapsed += Time.deltaTime;

        while (frameIndex < timeline.Frames.Count && timeline.Frames[frameIndex].Timestamp <= playbackElapsed)
        {
            playerController.ApplyReplayFrame(timeline.Frames[frameIndex]);
            frameIndex++;
        }

        if (playbackElapsed >= timeline.Duration)
        {
            isPlaying = false;
        }
    }

    public void Play(RecordedTimeline recordedTimeline)
    {
        timeline = recordedTimeline;
        frameIndex = 0;
        playbackElapsed = 0f;
        isPlaying = timeline != null && timeline.Frames.Count > 0;
    }

    public void ResetState()
    {
        transform.position = initialPosition;
        frameIndex = 0;
        playbackElapsed = 0f;
        isPlaying = false;

        if (playerController != null)
        {
            playerController.ResetState();
        }
    }
}
