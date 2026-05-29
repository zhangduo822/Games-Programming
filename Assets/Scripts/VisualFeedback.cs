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

    private void Awake()
    {
        TryFindPlayerRenderer();
    }

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

    public void Initialize(ReplayRecorder2D replayRecorder, CloneManager clones)
    {
        recorder = replayRecorder;
        cloneManager = clones;
        TryFindPlayerRenderer();
    }

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
    private GameObject outlineObject;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        if (outlineObject != null)
        {
            Destroy(outlineObject);
        }
    }
}
