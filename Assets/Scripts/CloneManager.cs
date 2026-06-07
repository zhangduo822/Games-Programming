using System.Collections.Generic;
using UnityEngine;

public class CloneManager : MonoBehaviour
{
    private readonly List<ReplayClone2D> activeClones = new List<ReplayClone2D>();
    [SerializeField] private ReplayClone2D clonePrefab;
    [SerializeField] private Transform spawnParent;

    private void Awake()
    {
        Debug.Log("[CloneManager] Awake called");
        if (spawnParent == null)
        {
            spawnParent = transform;
        }
    }

    public void SetClonePrefab(ReplayClone2D prefab)
    {
        clonePrefab = prefab;
    }

    public ReplayClone2D SpawnClone(Vector3 position, RecordedTimeline timeline)
    {
        if (clonePrefab == null)
        {
            Debug.LogWarning("CloneManager: No clone prefab assigned!");
            return null;
        }

        Debug.Log($"CloneManager: Spawning clone at {position} with timeline having {timeline?.Frames?.Count ?? 0} frames");

        ReplayClone2D clone = Instantiate(clonePrefab, position, Quaternion.identity, spawnParent);
        SimpleHumanoidVisual visual = clone.GetComponent<SimpleHumanoidVisual>();
        if (visual != null)
        {
            visual.SetVisualRole(SimpleHumanoidVisual.VisualRole.Clone);
        }

        clone.Play(timeline);
        activeClones.Add(clone);
        Debug.Log($"CloneManager: Clone spawned successfully. Active clones: {activeClones.Count}");
        return clone;
    }

    public void RemoveClone(ReplayClone2D clone)
    {
        if (clone == null) return;

        activeClones.Remove(clone);
        if (Application.isPlaying)
        {
            Destroy(clone.gameObject);
        }
        else
        {
            DestroyImmediate(clone.gameObject);
        }
    }

    public void RemoveAllClones()
    {
        for (int i = activeClones.Count - 1; i >= 0; i--)
        {
            ReplayClone2D clone = activeClones[i];
            activeClones.RemoveAt(i);
            if (clone != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(clone.gameObject);
                }
                else
                {
                    DestroyImmediate(clone.gameObject);
                }
            }
        }
    }

    public int CloneCount => activeClones.Count;

    public IReadOnlyList<ReplayClone2D> GetActiveClones() => activeClones;

    public void PauseAllClones()
    {
        foreach (var clone in activeClones)
        {
            if (clone != null) clone.Pause();
        }
    }

    public void ResumeAllClones()
    {
        foreach (var clone in activeClones)
        {
            if (clone != null) clone.Resume();
        }
    }
}
