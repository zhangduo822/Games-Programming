using UnityEngine;

public class VisualFeedback : MonoBehaviour
{
    [Header("Recording Effects")]
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private SpriteRenderer cloneRenderer;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float maxPulseIntensity = 1.5f;

    [Header("Colors")]
    [SerializeField] private Color recordingColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color cloneActiveColor = new Color(0.3f, 1f, 1f, 1f);
    [SerializeField] private Color clonePendingColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    private ReplayRecorder2D recorder;
    private CloneManager cloneManager;
    private Color originalPlayerColor;
    private float pulseTime;
    private bool initialized;

    // Finds the player renderer when the feedback component starts.
    private void Awake()
    {
        TryFindPlayerRenderer();
    }

    // Attempts to locate and cache the player's sprite renderer.
    private void TryFindPlayerRenderer()
    {
        if (playerRenderer != null)
        {
            originalPlayerColor = playerRenderer.color;
            initialized = true;
            return;
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRenderer = player.GetComponent<SpriteRenderer>();
            if (playerRenderer != null)
            {
                originalPlayerColor = playerRenderer.color;
            }
        }
        initialized = playerRenderer != null;
    }

    // Connects feedback effects to the recorder and clone manager.
    public void Initialize(ReplayRecorder2D replayRecorder, CloneManager clones)
    {
        recorder = replayRecorder;
        cloneManager = clones;
        TryFindPlayerRenderer();
    }

    // Updates player and clone visual feedback each frame.
    private void Update()
    {
        if (!initialized)
        {
            TryFindPlayerRenderer();
            return;
        }

        UpdateRecordingEffects();
        UpdateCloneEffects();
    }

    // Pulses the player sprite while recording is active.
    private void UpdateRecordingEffects()
    {
        if (playerRenderer == null || playerRenderer.gameObject == null) return;

        if (recorder != null && recorder.IsRecording)
        {
            pulseTime += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTime) * (maxPulseIntensity - 1f);
            playerRenderer.color = recordingColor * pulse;

            var outline = playerRenderer.gameObject.GetComponent<SpriteOutline>();
            if (outline == null)
            {
                outline = playerRenderer.gameObject.AddComponent<SpriteOutline>();
                outline.OutlineColor = recordingColor;
                outline.OutlineWidth = 3;
            }
        }
        else
        {
            playerRenderer.color = originalPlayerColor;

            var outline = playerRenderer.gameObject.GetComponent<SpriteOutline>();
            if (outline != null)
            {
                Destroy(outline);
            }
        }
    }

    // Pulses active clone sprites during replay playback.
    private void UpdateCloneEffects()
    {
        if (cloneManager == null) return;

        var clones = cloneManager.GetActiveClones();
        foreach (var clone in clones)
        {
            if (clone == null) continue;

            var renderer = clone.GetComponent<SpriteRenderer>();
            if (renderer == null) continue;

            if (clone.IsActive)
            {
                pulseTime += Time.deltaTime * pulseSpeed * 0.5f;
                float pulse = 0.7f + Mathf.Sin(pulseTime) * 0.3f;
                renderer.color = cloneActiveColor * pulse;
            }
        }
    }

    // Applies the pending color to the configured clone renderer.
    public void SetPendingCloneColor()
    {
        if (cloneRenderer != null)
        {
            cloneRenderer.color = clonePendingColor;
        }
    }
}

public class SpriteOutline : MonoBehaviour
{
    public Color OutlineColor = Color.white;
    [Range(0f, 0.3f)]
    public float OutlineWidth = 0.05f;

    private SpriteRenderer spriteRenderer;

    // Caches the sprite renderer used by the outline effect.
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
