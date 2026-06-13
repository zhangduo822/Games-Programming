using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator2D : MonoBehaviour, IResettable
{
    [SerializeField] private string resourcePrefix = "Art/Fire Trap";
    [SerializeField] private int frameCount = 7;
    [SerializeField] private float framesPerSecond = 10f;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private Color tint = Color.white;
    [SerializeField] private int zeroPadDigits;
    [SerializeField] private int idleFrameIndex;
    [SerializeField] private bool matchReferenceSpriteSize = true;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float elapsed;
    private bool isPlaying;
    private Vector3 baseLocalScale;
    private Vector2? referenceSpriteSize;
    private Sprite fallbackSprite;

    public bool HasAnyLoadedFrames { get; private set; }

    // Loads animation frames and prepares the starting frame.
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseLocalScale = transform.localScale;
        CaptureReferenceSpriteSize();
        LoadFrames();
        ApplyFrame(idleFrameIndex);
        isPlaying = playOnAwake && HasAnyLoadedFrames;
    }

    // Advances the sprite animation while playback is active.
    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length == 0)
        {
            return;
        }

        elapsed += Time.deltaTime;
        int frameIndex = Mathf.FloorToInt(elapsed * Mathf.Max(0.1f, framesPerSecond)) % frames.Length;
        ApplyFrame(frameIndex);
    }

    // Configures the resource prefix, frame count, speed, and tint.
    public void Configure(string prefix, int totalFrames, float fps, Color color)
    {
        resourcePrefix = prefix;
        frameCount = Mathf.Max(1, totalFrames);
        framesPerSecond = Mathf.Max(0.1f, fps);
        tint = color;
        frames = null;
        elapsed = 0f;
        CaptureReferenceSpriteSize();
        LoadFrames();
        ApplyFrame(idleFrameIndex);
        isPlaying = HasAnyLoadedFrames;
    }

    // Sets how many digits should be used when loading numbered frames.
    public void SetZeroPadDigits(int digits)
    {
        zeroPadDigits = Mathf.Max(0, digits);
        frames = null;
        LoadFrames();
        ApplyFrame(idleFrameIndex);
    }

    // Sets which frame should be shown while the animation is idle.
    public void SetIdleFrame(int frameIndex)
    {
        idleFrameIndex = Mathf.Clamp(frameIndex, 0, Mathf.Max(0, frameCount - 1));
        if (!isPlaying)
        {
            ApplyFrame(idleFrameIndex);
        }
    }

    // Uses a reference sprite to preserve visual size across animation frames.
    public void SetReferenceSprite(Sprite referenceSprite)
    {
        fallbackSprite = referenceSprite;
        referenceSpriteSize = referenceSprite != null ? (Vector2?)referenceSprite.bounds.size : null;
        baseLocalScale = transform.localScale;
        if (!isPlaying)
        {
            ApplyFrame(idleFrameIndex);
        }
    }

    // Starts sprite animation playback when frames are available.
    public void Play()
    {
        if (!HasAnyLoadedFrames)
        {
            ShowFallbackSprite();
            isPlaying = false;
            return;
        }

        isPlaying = true;
    }

    // Stops sprite animation playback on the current frame.
    public void Stop()
    {
        isPlaying = false;
    }

    // Stops playback and displays the configured idle frame.
    public void ShowIdleFrame()
    {
        elapsed = 0f;
        isPlaying = false;
        ApplyFrame(idleFrameIndex);
    }

    // Restores the animator to its initial playback and frame state.
    public void ResetState()
    {
        elapsed = 0f;
        ApplyFrame(idleFrameIndex);
        isPlaying = playOnAwake && HasAnyLoadedFrames;
    }

    // Loads the configured sprite frames from Resources.
    private void LoadFrames()
    {
        if (frames != null && frames.Length > 0)
        {
            return;
        }

        frames = new Sprite[Mathf.Max(1, frameCount)];
        HasAnyLoadedFrames = false;
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] = LoadFrame(i);
            if (frames[i] != null)
            {
                HasAnyLoadedFrames = true;
            }
        }
    }

    // Loads a single frame using supported resource naming patterns.
    private Sprite LoadFrame(int frameIndex)
    {
        Sprite sprite = null;

        if (zeroPadDigits > 0)
        {
            sprite = Resources.Load<Sprite>($"{resourcePrefix}{frameIndex.ToString($"D{zeroPadDigits}")}");
        }

        sprite ??= Resources.Load<Sprite>($"{resourcePrefix} {frameIndex}");
        sprite ??= Resources.Load<Sprite>($"{resourcePrefix}{frameIndex}");
        return sprite;
    }

    // Applies a loaded frame or fallback sprite to the renderer.
    private void ApplyFrame(int frameIndex)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.enabled = true;
        spriteRenderer.color = tint;
        spriteRenderer.drawMode = SpriteDrawMode.Simple;

        if (frames == null || frames.Length == 0)
        {
            ShowFallbackSprite();
            return;
        }

        int clampedFrameIndex = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
        if (frames[clampedFrameIndex] == null)
        {
            ShowFallbackSprite();
            return;
        }

        spriteRenderer.sprite = frames[clampedFrameIndex];
        ApplyReferenceScale(frames[clampedFrameIndex]);
    }

    // Stores the current sprite size as the scaling reference.
    private void CaptureReferenceSpriteSize()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            fallbackSprite = spriteRenderer.sprite;
            referenceSpriteSize = spriteRenderer.sprite.bounds.size;
        }
    }

    // Adjusts scale so animation frames match the reference sprite size.
    private void ApplyReferenceScale(Sprite frameSprite)
    {
        if (!matchReferenceSpriteSize || !referenceSpriteSize.HasValue || frameSprite == null)
        {
            return;
        }

        Vector2 frameSize = frameSprite.bounds.size;
        if (frameSize.x <= Mathf.Epsilon || frameSize.y <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 adjustedScale = baseLocalScale;
        adjustedScale.x *= referenceSpriteSize.Value.x / frameSize.x;
        adjustedScale.y *= referenceSpriteSize.Value.y / frameSize.y;
        transform.localScale = adjustedScale;
    }

    // Displays the fallback sprite when animation frames are unavailable.
    private void ShowFallbackSprite()
    {
        spriteRenderer.enabled = fallbackSprite != null;
        spriteRenderer.sprite = fallbackSprite;
        RestoreBaseScale();
    }

    // Restores the transform scale captured before animation adjustments.
    private void RestoreBaseScale()
    {
        transform.localScale = baseLocalScale;
    }
}
