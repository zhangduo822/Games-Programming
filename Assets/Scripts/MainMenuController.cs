using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    private const string StorySceneName = "StoryIntro";
    private const string RunSpritePrefix = "Art/Walk";

    [SerializeField] private Color panelColor = new Color(0.08f, 0.14f, 0.22f, 0.46f);
    [SerializeField] private Color buttonColor = new Color(0.12f, 0.32f, 0.56f, 0.9f);
    [SerializeField] private Color buttonHoverColor = new Color(0.18f, 0.48f, 0.78f, 0.94f);

    private Font menuFont;

    // Builds the menu scene UI after ensuring camera and input systems exist.
    private void Awake()
    {
        EnsureCamera();
        EnsureEventSystem();
        BuildMenu();
    }

    // Loads the story intro scene before gameplay starts.
    public void PlayGame()
    {
        SceneManager.LoadScene(StorySceneName);
    }

    // Quits the built game or stops play mode inside the Unity Editor.
    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Creates or configures the menu camera and layered cloud background.
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

        camera.orthographic = true;
        camera.orthographicSize = 4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.42f, 0.58f, 0.78f, 1f);

        if (camera.GetComponent<LayeredCloudBackground2D>() == null)
        {
            camera.gameObject.AddComponent<LayeredCloudBackground2D>();
        }
    }

    // Creates an EventSystem so UI buttons can receive input.
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

    // Creates the hero animation, title panel, and menu buttons.
    private void BuildMenu()
    {
        menuFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Canvas canvas = CreateCanvas();
        RectTransform root = canvas.GetComponent<RectTransform>();

        Image overlay = CreateImage("SoftOverlay", root, new Color(0.1f, 0.17f, 0.26f, 0.16f));
        StretchToFill(overlay.rectTransform);

        Image hero = CreateImage("HeroImage", root, Color.white);
        hero.preserveAspect = true;
        UIImageFrameAnimator animator = hero.gameObject.AddComponent<UIImageFrameAnimator>();
        animator.Configure(RunSpritePrefix, 10, 10f, true);
        hero.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        hero.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        hero.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        hero.rectTransform.anchoredPosition = new Vector2(-210f, 60f);
        hero.rectTransform.sizeDelta = new Vector2(190f, 230f);

        Image panel = CreateImage("MenuPanel", root, panelColor);
        panel.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        panel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panel.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        panel.rectTransform.anchoredPosition = new Vector2(170f, 0f);
        panel.rectTransform.sizeDelta = new Vector2(360f, 330f);

        Text title = CreateText("Title", panel.rectTransform, "Time Replay", 38, FontStyle.Bold, Color.white);
        title.alignment = TextAnchor.MiddleCenter;
        title.rectTransform.anchorMin = new Vector2(0.08f, 0.72f);
        title.rectTransform.anchorMax = new Vector2(0.92f, 0.92f);
        title.rectTransform.offsetMin = Vector2.zero;
        title.rectTransform.offsetMax = Vector2.zero;

        Button playButton = CreateButton("PlayButton", panel.rectTransform, "Play Game");
        RectTransform playRect = (RectTransform)playButton.transform;
        playRect.anchorMin = new Vector2(0.16f, 0.42f);
        playRect.anchorMax = new Vector2(0.84f, 0.58f);
        playRect.offsetMin = Vector2.zero;
        playRect.offsetMax = Vector2.zero;
        playButton.onClick.AddListener(PlayGame);

        Button quitButton = CreateButton("QuitButton", panel.rectTransform, "Quit Game");
        RectTransform quitRect = (RectTransform)quitButton.transform;
        quitRect.anchorMin = new Vector2(0.16f, 0.2f);
        quitRect.anchorMax = new Vector2(0.84f, 0.36f);
        quitRect.offsetMin = Vector2.zero;
        quitRect.offsetMax = Vector2.zero;
        quitButton.onClick.AddListener(QuitGame);
    }

    // Creates the screen-space canvas used by the main menu UI.
    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
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
        text.font = menuFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    // Creates a menu button with a centered text label.
    private Button CreateButton(string objectName, Transform parent, string label)
    {
        Image image = CreateImage(objectName, parent, buttonColor);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = new Color(0.08f, 0.22f, 0.42f, 1f);
        colors.selectedColor = buttonHoverColor;
        colors.disabledColor = new Color(0.32f, 0.36f, 0.42f, 0.55f);
        button.colors = colors;

        Text text = CreateText("Label", image.transform, label, 26, FontStyle.Bold, Color.white);
        text.alignment = TextAnchor.MiddleCenter;
        StretchToFill(text.rectTransform);
        return button;
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
