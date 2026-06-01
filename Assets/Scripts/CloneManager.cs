using System.Collections.Generic;
using UnityEngine;

public class CloneManager : MonoBehaviour
{
    private readonly List<ReplayClone2D> activeClones = new List<ReplayClone2D>();
    [SerializeField] private ReplayClone2D clonePrefab;
    [SerializeField] private Transform spawnParent;
    private static Sprite whiteSprite;
    private static Material defaultMaterial;

    private void Awake()
    {
        Debug.Log("[CloneManager] Awake called");
        if (spawnParent == null)
        {
            spawnParent = transform;
        }
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return whiteSprite;
    }

    private static Material GetDefaultMaterial()
    {
        if (defaultMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            defaultMaterial = new Material(shader);
        }
        return defaultMaterial;
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

        var renderer = clone.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = new Color(1f, 0.41f, 0.71f, 0.7f);
            renderer.material = GetDefaultMaterial();

            if (renderer.sprite == null)
            {
                Debug.Log("CloneManager: Sprite was null, assigning WhiteSprite");
                renderer.sprite = GetWhiteSprite();
            }
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
