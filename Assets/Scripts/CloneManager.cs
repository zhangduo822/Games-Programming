using System.Collections.Generic;
using UnityEngine;

public class CloneManager : MonoBehaviour
{
    private readonly List<ReplayClone2D> activeClones = new List<ReplayClone2D>();
    [SerializeField] private ReplayClone2D clonePrefab;
    [SerializeField] private Transform spawnParent;

    private void Awake()
    {
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
            Debug.LogWarning("CloneManager: No clone prefab assigned.");
            return null;
        }

        ReplayClone2D clone = Instantiate(clonePrefab, position, Quaternion.identity, spawnParent);

        var renderer = clone.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color c = renderer.color;
            c.a = 0.65f;
            renderer.color = c;
        }

        clone.Play(timeline);
        activeClones.Add(clone);
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
