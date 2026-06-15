using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryIntroController : MonoBehaviour
{
    private const string DefaultTargetSceneName = "SampleScene";

    private static readonly string[] StoryLines =
    {
        "An explorer wakes inside a world outside his own time.",
        "The way home is gone, and the sky itself feels unfamiliar.",
        "In this strange dimension, he discovers two impossible powers.",
        "He can rewind time, and his past actions can return as living echoes.",
        "To escape, he must work with his own copies and unlock the path home."
    };

    [SerializeField] private float charactersPerSecond = 26f;
    [SerializeField] private float linePause = 0.55f;
    [SerializeField] private float finishedDelay = 1.25f;
    [SerializeField] private string targetSceneName = DefaultTargetSceneName;
    [SerializeField] private string[] storyLines = StoryLines;

    private Text storyText;
    private Text promptText;
    private Font storyFont;
    private int lineIndex;
    private int characterIndex;
    private float timer;
    private float pauseTimer;
    private bool lineComplete;
    private bool storyComplete;

    // Returns the configured story lines or the default intro text.
    private string[] ActiveStoryLines => storyLines != null && storyLines.Length > 0 ? storyLines : StoryLines;

    // Builds the story screen after ensuring camera and UI input exist.
    private void Awake()
    {
        EnsureCamera();
        EnsureEventSystem();
        BuildUI();
    }

    // Advances story text playback and handles skip input.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            EnterTargetScene();
            return;
        }

        PlayStory();
    }

    // Loads the configured scene after the story ends or is skipped.
    public void EnterTargetScene()
    {
        string sceneName = string.IsNullOrWhiteSpace(targetSceneName) ? DefaultTargetSceneName : targetSceneName;
        SceneManager.LoadScene(sceneName);
    }

    // Types the story lines over time and exits when finished.
    private void PlayStory()
    {
        string[] activeLines = ActiveStoryLines;

        if (storyComplete)
        {
            pauseTimer += Time.unscaledDeltaTime;
            if (pauseTimer >= finishedDelay)
            {
                EnterTargetScene();
            }
            return;
        }

        if (lineIndex >= activeLines.Length)
        {
            storyComplete = true;
            pauseTimer = 0f;
            return;
        }

        if (lineComplete)
        {
            pauseTimer += Time.unscaledDeltaTime;
            if (pauseTimer >= linePause)
            {
                lineIndex++;
                characterIndex = 0;
                timer = 0f;
                pauseTimer = 0f;
                lineComplete = false;
            }
            return;
        }

        timer += Time.unscaledDeltaTime * charactersPerSecond;
        int nextCharacterIndex = Mathf.Min(activeLines[lineIndex].Length, Mathf.FloorToInt(timer));
        if (nextCharacterIndex != characterIndex)
        {
            characterIndex = nextCharacterIndex;
            RefreshStoryText();
        }

        if (characterIndex >= activeLines[lineIndex].Length)
        {
            lineComplete = true;
        }
    }

    // Rebuilds the visible story text from completed and current lines.
    private void RefreshStoryText()
    {
        string[] activeLines = ActiveStoryLines;
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < lineIndex; i++)
        {
            builder.AppendLine(activeLines[i]);
            builder.AppendLine();
        }

        if (lineIndex < activeLines.Length)
        {
            builder.Append(activeLines[lineIndex].Substring(0, characterIndex));
        }

        storyText.text = builder.ToString();
    }

    // Creates or configures a black-background camera for the intro.
    private void EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
    }

    // Creates an EventSystem so the skip button can be clicked.
    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    // Creates the black background, story text, prompt, and skip button.
    private void BuildUI()
    {
        storyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Canvas canvas = CreateCanvas();
        RectTransform root = canvas.GetComponent<RectTransform>();

        Image background = CreateImage("BlackBackground", root, Color.black);
        StretchToFill(background.rectTransform);

        storyText = CreateText("StoryText", root, string.Empty, 34, FontStyle.Normal, Color.white);
        storyText.alignment = TextAnchor.MiddleLeft;
        storyText.lineSpacing = 1.25f;
        storyText.rectTransform.anchorMin = new Vector2(0.16f, 0.28f);
        storyText.rectTransform.anchorMax = new Vector2(0.84f, 0.72f);
        storyText.rectTransform.offsetMin = Vector2.zero;
        storyText.rectTransform.offsetMax = Vector2.zero;

        promptText = CreateText("SkipPrompt", root, "Click X or press X to skip", 20, FontStyle.Normal, new Color(1f, 1f, 1f, 0.72f));
        promptText.alignment = TextAnchor.LowerRight;
        promptText.rectTransform.anchorMin = new Vector2(0.56f, 0.06f);
        promptText.rectTransform.anchorMax = new Vector2(0.94f, 0.16f);
        promptText.rectTransform.offsetMin = Vector2.zero;
        promptText.rectTransform.offsetMax = Vector2.zero;

        Button skipButton = CreateSkipButton(root);
        skipButton.onClick.AddListener(EnterTargetScene);
    }

    // Creates the screen-space canvas used by the story UI.
    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("StoryCanvas", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    // Creates the clickable X button that skips directly to gameplay.
    private Button CreateSkipButton(Transform parent)
    {
        Image image = CreateImage("SkipButton", parent, new Color(1f, 1f, 1f, 0.08f));
        image.rectTransform.anchorMin = new Vector2(0.93f, 0.88f);
        image.rectTransform.anchorMax = new Vector2(0.985f, 0.975f);
        image.rectTransform.offsetMin = Vector2.zero;
        image.rectTransform.offsetMax = Vector2.zero;

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.08f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.22f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.34f);
        button.colors = colors;

        Text label = CreateText("Label", image.transform, "X", 30, FontStyle.Bold, Color.white);
        label.alignment = TextAnchor.MiddleCenter;
        StretchToFill(label.rectTransform);
        return button;
    }

    // Creates a UI image under the requested parent.
    private Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    // Creates a styled UI text element under the requested parent.
    private Text CreateText(string objectName, Transform parent, string content, int fontSize, FontStyle fontStyle, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = storyFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    // Stretches a RectTransform to fill its parent.
    private static void StretchToFill(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
