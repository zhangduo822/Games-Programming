using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseSettingsMenu2D : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string PauseIconResourcePath = "Art/Pause";

    private static PauseSettingsMenu2D activeMenu;
    private static Texture2D pauseIcon;

    private bool isOpen;
    private float previousTimeScale = 1f;

    public static bool IsPaused => activeMenu != null && activeMenu.isOpen;

    private void Awake()
    {
        activeMenu = this;
        pauseIcon ??= Resources.Load<Texture2D>(PauseIconResourcePath);
    }

    private void OnDestroy()
    {
        if (activeMenu == this)
        {
            activeMenu = null;
        }

        if (isOpen)
        {
            Time.timeScale = previousTimeScale;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    private void OnGUI()
    {
        DrawSettingsHint();

        if (isOpen)
        {
            DrawSettingsPanel();
        }
    }

    private void DrawSettingsHint()
    {
        Rect iconRect = new Rect(12, 12, 42, 42);

        if (pauseIcon != null)
        {
            GUI.DrawTexture(iconRect, pauseIcon, ScaleMode.ScaleToFit, true);
            if (GUI.Button(iconRect, GUIContent.none, GUIStyle.none))
            {
                ToggleSettings();
            }
        }
        else if (GUI.Button(iconRect, "Pause"))
        {
            ToggleSettings();
        }

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft
        };
        GUI.Label(new Rect(62, 18, 260, 30), "Press Esc to open Settings", labelStyle);
    }

    private void DrawSettingsPanel()
    {
        float width = 360f;
        float height = 230f;
        Rect panelRect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

        GUI.Box(panelRect, string.Empty);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(panelRect.x + 20f, panelRect.y + 18f, panelRect.width - 40f, 42f), "Settings", titleStyle);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };

        if (GUI.Button(new Rect(panelRect.x + 55f, panelRect.y + 82f, panelRect.width - 110f, 44f), "Continue Game", buttonStyle))
        {
            CloseSettings();
        }

        if (GUI.Button(new Rect(panelRect.x + 55f, panelRect.y + 146f, panelRect.width - 110f, 44f), "Return to Menu", buttonStyle))
        {
            ReturnToMenu();
        }
    }

    private void ToggleSettings()
    {
        if (isOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }

    private void OpenSettings()
    {
        previousTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
        Time.timeScale = 0f;
        isOpen = true;
    }

    private void CloseSettings()
    {
        Time.timeScale = previousTimeScale;
        isOpen = false;
    }

    private void ReturnToMenu()
    {
        Time.timeScale = previousTimeScale;
        isOpen = false;
        SceneManager.LoadScene(MainMenuSceneName);
    }
}
