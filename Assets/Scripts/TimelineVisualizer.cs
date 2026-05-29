using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineVisualizer : MonoBehaviour
{
    [SerializeField] private Image recordingFill;
    [SerializeField] private Image[] cloneTimelines;
    [SerializeField] private Color recordingColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color cloneColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color pendingColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private float maxRecordingTime = 5f;

    private ReplayRecorder2D recorder;
    private CloneManager cloneManager;
    private float[] cloneProgress;
    private RectTransform rectTransform;
    private bool initialized;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (cloneTimelines != null)
        {
            cloneProgress = new float[cloneTimelines.Length];
        }
    }

    public void Initialize(ReplayRecorder2D replayRecorder, CloneManager clones)
    {
        recorder = replayRecorder;
        cloneManager = clones;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        UpdateRecordingBar();
        UpdateCloneTimelines();
    }

    private void UpdateRecordingBar()
    {
        if (recordingFill == null) return;

        if (recorder != null && recorder.IsRecording)
        {
            float progress = 1f - (recorder.RemainingTime / maxRecordingTime);
            recordingFill.fillAmount = progress;
            recordingFill.color = recordingColor;
        }
        else
        {
            recordingFill.fillAmount = 0f;
        }
    }

    private void UpdateCloneTimelines()
    {
        if (cloneManager == null || cloneTimelines == null) return;

        var clones = cloneManager.GetActiveClones();
        for (int i = 0; i < cloneTimelines.Length; i++)
        {
            Image timelineBar = cloneTimelines[i];
            if (timelineBar == null) continue;

            if (i < clones.Count)
            {
                var clone = clones[i];
                if (clone != null && clone.IsActive)
                {
                    timelineBar.fillAmount = clone.Progress;
                    timelineBar.color = cloneColor;
                }
                else
                {
                    timelineBar.fillAmount = 0f;
                    timelineBar.color = pendingColor;
                }
            }
            else
            {
                timelineBar.fillAmount = 0f;
                timelineBar.color = pendingColor;
            }
        }
    }
}
