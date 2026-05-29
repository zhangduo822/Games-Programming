using System.Collections.Generic;
using UnityEngine;

public class TemporalParadoxGame : MonoBehaviour
{
    [SerializeField] private PlayerController2D player;
    [SerializeField] private ReplayRecorder2D recorder;
    [SerializeField] private ReplayClone2D clonePrefab;
    [SerializeField] private CloneManager cloneManager;
    [SerializeField] private Door2D door;
    [SerializeField] private PressurePlate2D leftPlate;
    [SerializeField] private PressurePlate2D rightPlate;

    private readonly List<RecordedTimeline> pendingTimelines = new List<RecordedTimeline>();
    private string message = "Press R to record. Hold the left plate, then press R again.";
    private float messageTimer = 4f;
    private const int MAX_CLONES = 3;

    public bool IsCleared { get; private set; }

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

    private void Awake()
    {
        if (cloneManager == null)
        {
            cloneManager = GetComponent<CloneManager>();
            if (cloneManager == null)
            {
                cloneManager = gameObject.AddComponent<CloneManager>();
            }
        }
        if (clonePrefab != null)
        {
            cloneManager.SetClonePrefab(clonePrefab);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRecording();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuickReset();
        }

        if (Input.GetKeyDown(KeyCode.E) && cloneManager.CloneCount > 0)
        {
            SpawnAllPendingClones();
        }

        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
        }
    }

    private void ToggleRecording()
    {
        if (recorder == null)
        {
            return;
        }

        if (recorder.IsRecording)
        {
            RecordedTimeline timeline = recorder.StopRecording();
            if (timeline != null && timeline.Frames.Count > 0)
            {
                pendingTimelines.Add(timeline);
                UpdateRecordingMessage();
            }
        }
        else
        {
            if (cloneManager.CloneCount + pendingTimelines.Count >= MAX_CLONES)
            {
                ShowMessage($"Max {MAX_CLONES} clones allowed. Press E to spawn all or Q to reset.");
                return;
            }

            recorder.StartRecording();
            ShowMessage("Recording... move to a pressure plate, then press R.");
        }
    }

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

    private void SpawnAllPendingClones()
    {
        if (pendingTimelines.Count == 0)
        {
            ShowMessage("No recordings to spawn. Press R to record first.");
            return;
        }

        cloneManager.RemoveAllClones();
        player.ResetState();

        Vector3 spawnPosition = player.transform.position;
        foreach (var timeline in pendingTimelines)
        {
            cloneManager.SpawnClone(spawnPosition, timeline);
        }

        pendingTimelines.Clear();
        ShowMessage($"Spawned {cloneManager.CloneCount} clone(s)! Cooperate to solve the puzzle.");
    }

    public void QuickReset()
    {
        cloneManager.RemoveAllClones();
        pendingTimelines.Clear();

        if (recorder != null && recorder.IsRecording)
        {
            recorder.StopRecording();
        }

        if (player != null)
        {
            player.ResetState();
        }

        if (leftPlate != null) leftPlate.ResetState();
        if (rightPlate != null) rightPlate.ResetState();
        if (door != null) door.ResetState();

        IsCleared = false;
        ShowMessage("Reset. Press R to start recording.");
    }

    public void MarkCleared()
    {
        IsCleared = true;
        ShowMessage("Level cleared! Press Q to retry.");
    }

    private void ShowMessage(string text)
    {
        message = text;
        messageTimer = 3f;
    }

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

        GUI.Box(new Rect(12, 12, 540, 210), text, style);
    }
}
