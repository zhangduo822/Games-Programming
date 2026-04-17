using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ReplaySessionController sessionController;
    [SerializeField] private string demoLevelSceneName = "DemoLevel_01";

    public bool IsLevelCleared { get; private set; }

    public void MarkLevelCleared()
    {
        IsLevelCleared = true;
        Debug.Log("Level cleared.");
    }

    public void QuickReset()
    {
        if (sessionController != null)
        {
            sessionController.QuickReset();
        }
    }

    public void ReloadDemoScene()
    {
        if (!string.IsNullOrWhiteSpace(demoLevelSceneName))
        {
            SceneManager.LoadScene(demoLevelSceneName);
        }
    }
}
