using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TemporalParadoxGame : MonoBehaviour
{
    [SerializeField] private PlayerController2D player;
    [SerializeField] private ReplayRecorder2D recorder;
    [SerializeField] private ReplayClone2D clonePrefab;
    [SerializeField] private CloneManager cloneManager;
    [SerializeField] private Door2D door;
    [SerializeField] private PressurePlate2D leftPlate;
    [SerializeField] private PressurePlate2D rightPlate;
    [SerializeField] private string nextSceneName;

    private readonly List<RecordedTimeline> pendingTimelines = new List<RecordedTimeline>();
    private string message = "Press R to record. Hold the left plate, then press R again.";
    private float messageTimer = 4f;
    private const int MAX_CLONES = 3;

    public bool IsCleared { get; private set; }
    public string NextSceneName => nextSceneName;

    // Wires the core gameplay references used by this level controller.
    public void Configure(PlayerController2D livePlayer, ReplayRecorder2D replayRecorder, ReplayClone2D prefab, Door2D targetDoor, PressurePlate2D left, PressurePlate2D right)
    {
        player = livePlayer;
        recorder = replayRecorder;
        clonePrefab = prefab;
        door = targetDoor;
        leftPlate = left;
        rightPlate = right;

        if (cloneManager != null)
        {
            cloneManager.SetClonePrefab(clonePrefab);
        }
    }

    // Sets the scene that should load when the level is cleared.
    public void SetNextSceneName(string sceneName)
    {
        nextSceneName = sceneName;
    }

    // Finds required components and fills default scene-transition settings.
    private void Awake()
    {
        if (GetComponent<PauseSettingsMenu2D>() == null)
        {
            gameObject.AddComponent<PauseSettingsMenu2D>();
        }

        if (cloneManager == null)
        {
            cloneManager = GetComponent<CloneManager>();
            if (cloneManager == null)
            {
                cloneManager = gameObject.AddComponent<CloneManager>();
            }
        }

        if (recorder == null)
        {
            recorder = GetComponent<ReplayRecorder2D>();
        }

        if (clonePrefab == null)
        {
            clonePrefab = Resources.Load<ReplayClone2D>("TimeClonePrefab");
        }

        if (clonePrefab != null && cloneManager != null)
        {
            cloneManager.SetClonePrefab(clonePrefab);
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            nextSceneName = GetDefaultNextSceneName();
        }
    }

    // Logs startup wiring information for debugging.
    private void Start()
    {
        Debug.Log($"[START] TemporalParadoxGame - Recorder: {(recorder != null ? "OK" : "NULL")}, CloneManager: {(cloneManager != null ? "OK" : "NULL")}, ClonePrefab: {(clonePrefab != null ? "OK" : "NULL")}");
    }

    // Handles recording, reset, clone spawning, and message timers.
    private void Update()
    {
        if (PauseSettingsMenu2D.IsPaused)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[DEBUG] R key pressed!");
            ToggleRecording();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuickReset();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[DEBUG] E key pressed! Pending: {pendingTimelines.Count}, Active: {cloneManager.CloneCount}");
            SpawnAllPendingClones();
        }

        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
        }
    }

    // Starts or stops recording the player's current timeline.
    private void ToggleRecording()
    {
        if (recorder == null)
        {
            Debug.LogWarning("ToggleRecording: Recorder is null!");
            return;
        }

        if (recorder.IsRecording)
        {
            Debug.Log("ToggleRecording: Stopping recording...");
            RecordedTimeline timeline = recorder.StopRecording();
            Debug.Log($"ToggleRecording: Timeline has {timeline?.Frames?.Count ?? 0} frames");
            if (timeline != null && timeline.Frames.Count > 0)
            {
                pendingTimelines.Add(timeline);
                UpdateRecordingMessage();
                Debug.Log($"ToggleRecording: Added timeline to pending. Total pending: {pendingTimelines.Count}");
            }
        }
        else
        {
            if (cloneManager.CloneCount + pendingTimelines.Count >= MAX_CLONES)
            {
                ShowMessage($"Max {MAX_CLONES} clones allowed. Press E to spawn all or Q to reset.");
                return;
            }

            Debug.Log("ToggleRecording: Starting recording...");
            recorder.StartRecording();
            ShowMessage("Recording... move to a pressure plate, then press R.");
        }
    }

    // Updates the HUD message after saving a recording.
    private void UpdateRecordingMessage()
    {
        int total = cloneManager.CloneCount + pendingTimelines.Count;
        if (total == 0)
        {
            ShowMessage("Recording saved. Press R to record more or E to spawn all clones.");
        }
        else
        {
            ShowMessage($"{total} clone(s) ready. Press R for more or E to spawn all.");
        }
    }

    // Spawns all saved recordings as replay clones.
    private void SpawnAllPendingClones()
    {
        Debug.Log($"[SpawnAllPendingClones] Called - pending: {pendingTimelines.Count}, player: {(player != null ? "OK" : "NULL")}");

        if (pendingTimelines.Count == 0)
        {
            ShowMessage("No recordings to spawn. Press R to record first.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("[SpawnAllPendingClones] Player is null!");
            return;
        }

        int remainingSlots = MAX_CLONES - cloneManager.CloneCount;
        if (pendingTimelines.Count > remainingSlots)
        {
            ShowMessage($"Only {remainingSlots} clone slot(s) left. Press Q to reset clones.");
            return;
        }

        if (player != null)
        {
            player.ClearReplayInput();
        }

        int spawnedCount = 0;
        foreach (var timeline in pendingTimelines)
        {
            Vector3 spawnPosition = player.transform.position;
            if (timeline != null && timeline.Frames != null && timeline.Frames.Count > 0)
            {
                spawnPosition = timeline.Frames[0].Position;
            }
            Debug.Log($"[SpawnAllPendingClones] Spawning clone at {spawnPosition}");
            ReplayClone2D clone = cloneManager.SpawnClone(spawnPosition, timeline);
            if (clone == null)
            {
                Debug.LogError("[SpawnAllPendingClones] Clone spawn returned null!");
            }
            else
            {
                spawnedCount++;
            }
        }

        pendingTimelines.Clear();
        ShowMessage($"Spawned {spawnedCount} new clone(s). Active clones: {cloneManager.CloneCount}/{MAX_CLONES}.");
    }

    // Clears clones and resets all resettable gameplay objects.
    public void QuickReset()
    {
        cloneManager.RemoveAllClones();
        pendingTimelines.Clear();

        if (recorder != null && recorder.IsRecording)
        {
            recorder.StopRecording();
        }

        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IResettable resettable)
            {
                resettable.ResetState();
            }
        }

        IsCleared = false;
        ShowMessage("Reset. Press R to start recording.");
    }

    // Marks the level complete and loads the configured next scene.
    public void MarkCleared()
    {
        IsCleared = true;

        if (!string.IsNullOrWhiteSpace(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            ShowMessage($"Level cleared! Loading {nextSceneName}...");
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        ShowMessage("Level cleared! Press Q to retry.");
    }

    // Chooses the next scene based on the current scene name.
    private string GetDefaultNextSceneName()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "SampleScene")
        {
            return "Level2";
        }

        if (currentSceneName == "Level2")
        {
            return "Level3";
        }

        if (currentSceneName == "Level3")
        {
            return "EndingStory";
        }

        return string.Empty;
    }

    // Shows a temporary HUD message to the player.
    private void ShowMessage(string text)
    {
        message = text;
        messageTimer = 3f;
    }

    // Draws the simple debug HUD and controls reminder.
    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 15,
            alignment = TextAnchor.UpperLeft,
            wordWrap = true
        };

        string recording = recorder != null && recorder.IsRecording ? $"Recording: {recorder.RemainingTime:0.0}s left" : "Not recording";
        string cloneStatus = $"Clones: {cloneManager.CloneCount}/{MAX_CLONES}";
        if (pendingTimelines.Count > 0)
        {
            cloneStatus += $" (+{pendingTimelines.Count} pending)";
        }

        string text =
            "Temporal Paradox - 2D Demo\n\n" +
            "A/D or Arrows: move | Space/W: jump\n" +
            "R: record/stop | E: spawn clones | Q: reset\n\n" +
            $"{cloneStatus} | {recording}\n" +
            "Goal: record clones on plates, then spawn them to open the door.\n";

        if (messageTimer > 0f)
        {
            text += "\n" + message;
        }

        if (IsCleared)
        {
            text += "\n\nLEVEL CLEARED";
        }

        GUI.Box(new Rect(12, 58, 540, 210), text, style);
    }
}
