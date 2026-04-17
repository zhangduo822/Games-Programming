using System.Collections.Generic;
using UnityEngine;

public class ReplaySessionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController livePlayer;
    [SerializeField] private ReplayRecorder recorder;
    [SerializeField] private ReplayCloneController clonePrefab;
    [SerializeField] private Transform cloneSpawnPoint;
    [SerializeField] private Transform cloneParent;

    [Header("Input")]
    [SerializeField] private KeyCode recordToggleKey = KeyCode.R;
    [SerializeField] private KeyCode quickResetKey = KeyCode.T;

    [Header("Resettable Objects")]
    [SerializeField] private List<MonoBehaviour> resettableBehaviours = new List<MonoBehaviour>();

    private readonly List<IResettable> resettables = new List<IResettable>();
    private ReplayCloneController activeClone;

    private void Awake()
    {
        CacheResettables();
    }

    private void Update()
    {
        if (Input.GetKeyDown(recordToggleKey))
        {
            if (recorder.IsRecording)
            {
                StopRecordingAndReplay();
            }
            else
            {
                StartRecording();
            }
        }

        if (Input.GetKeyDown(quickResetKey))
        {
            QuickReset();
        }
    }

    public void StartRecording()
    {
        if (recorder == null)
        {
            return;
        }

        recorder.StartRecording();
    }

    public void StopRecordingAndReplay()
    {
        if (recorder == null || clonePrefab == null)
        {
            return;
        }

        RecordedTimeline timeline = recorder.StopRecording();
        if (timeline == null || timeline.Frames.Count == 0)
        {
            return;
        }

        if (activeClone != null)
        {
            Destroy(activeClone.gameObject);
        }

        Vector3 spawnPosition = cloneSpawnPoint != null ? cloneSpawnPoint.position : livePlayer.transform.position;
        activeClone = Instantiate(clonePrefab, spawnPosition, Quaternion.identity, cloneParent);
        activeClone.gameObject.tag = "Clone";
        activeClone.Play(timeline);
    }

    public void QuickReset()
    {
        if (activeClone != null)
        {
            Destroy(activeClone.gameObject);
            activeClone = null;
        }

        if (recorder != null && recorder.IsRecording)
        {
            recorder.StopRecording();
        }

        for (int i = 0; i < resettables.Count; i++)
        {
            resettables[i].ResetState();
        }
    }

    private void CacheResettables()
    {
        resettables.Clear();

        if (livePlayer != null)
        {
            resettables.Add(livePlayer);
        }

        for (int i = 0; i < resettableBehaviours.Count; i++)
        {
            if (resettableBehaviours[i] is IResettable resettable)
            {
                resettables.Add(resettable);
            }
        }
    }
}
