using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIImageFrameAnimator : MonoBehaviour
{
    [SerializeField] private string resourcePrefix = "Art/Walk";
    [SerializeField] private int frameCount = 10;
    [SerializeField] private float framesPerSecond = 10f;
    [SerializeField] private bool faceRight = true;

    private Image image;
    private Sprite[] frames;
    private float timer;
    private int currentFrame;

    // Configures the sprite frame sequence and facing direction.
    public void Configure(string prefix, int count, float fps, bool shouldFaceRight)
    {
        resourcePrefix = prefix;
        frameCount = count;
        framesPerSecond = fps;
        faceRight = shouldFaceRight;
        LoadFrames();
        ApplyFrame(0);
        ApplyFacing();
    }

    // Loads frames and applies the first frame when the image wakes up.
    private void Awake()
    {
        image = GetComponent<Image>();
        LoadFrames();
        ApplyFrame(0);
        ApplyFacing();
    }

    // Advances the UI image animation using unscaled time.
    private void Update()
    {
        if (frames == null || frames.Length == 0 || framesPerSecond <= 0f)
        {
            return;
        }

        timer += Time.unscaledDeltaTime;
        float frameDuration = 1f / framesPerSecond;
        while (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrame = (currentFrame + 1) % frames.Length;
            ApplyFrame(currentFrame);
        }
    }

    // Loads all configured animation frames from Resources.
    private void LoadFrames()
    {
        image = image != null ? image : GetComponent<Image>();
        frames = new Sprite[Mathf.Max(0, frameCount)];

        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] = Resources.Load<Sprite>($"{resourcePrefix}{i:00}");
        }
    }

    // Applies a specific loaded sprite frame to the image.
    private void ApplyFrame(int index)
    {
        if (image == null || frames == null || frames.Length == 0)
        {
            return;
        }

        Sprite frame = frames[Mathf.Clamp(index, 0, frames.Length - 1)];
        if (frame != null)
        {
            image.sprite = frame;
            image.preserveAspect = true;
        }
    }

    // Flips the RectTransform scale so the image faces the requested direction.
    private void ApplyFacing()
    {
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform == null)
        {
            return;
        }

        Vector3 scale = rectTransform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        rectTransform.localScale = scale;
    }
}
