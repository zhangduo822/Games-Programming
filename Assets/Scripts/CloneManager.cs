using System.Collections.Generic;
using UnityEngine;

public class CloneManager : MonoBehaviour
{
    private const string CloneSpriteResourcePath = "Art/Hero2";

    private readonly List<ReplayClone2D> activeClones = new List<ReplayClone2D>();
    [SerializeField] private ReplayClone2D clonePrefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private Transform playerVisualSource;
    [SerializeField] private bool cloneSourceSpriteFacesLeft = true;

    private static Sprite cachedCloneSprite;

    // Initializes clone parenting and finds the player as a visual reference.
    private void Awake()
    {
        Debug.Log("[CloneManager] Awake called");
        if (spawnParent == null)
        {
            spawnParent = transform;
        }

        if (playerVisualSource == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerVisualSource = player.transform;
            }
        }
    }

    // Sets the prefab used when spawning replay clones.
    public void SetClonePrefab(ReplayClone2D prefab)
    {
        clonePrefab = prefab;
    }

    // Spawns a replay clone and starts it playing the provided timeline.
    public ReplayClone2D SpawnClone(Vector3 position, RecordedTimeline timeline)
    {
        if (clonePrefab == null)
        {
            Debug.LogWarning("CloneManager: No clone prefab assigned!");
            return null;
        }

        Debug.Log($"CloneManager: Requested spawn at {position} with timeline having {timeline?.Frames?.Count ?? 0} frames");

        Vector3 spawnPosition = position;
        if (playerVisualSource == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerVisualSource = player.transform;
            }
        }

        if (playerVisualSource != null)
        {
            spawnPosition = playerVisualSource.position + new Vector3(1.1f, 0f, 0f);
        }

        ReplayClone2D clone = Instantiate(clonePrefab, spawnPosition, Quaternion.identity, spawnParent);
        clone.name = $"RuntimeClone_{activeClones.Count}";
        ConfigureCloneInstance(clone);
        clone.Play(timeline);
        activeClones.Add(clone);
        Debug.Log($"CloneManager: Spawned {clone.name} at world position {clone.transform.position}, local position {clone.transform.localPosition}, parent {(clone.transform.parent != null ? clone.transform.parent.name : "<none>")}");
        return clone;
    }

    // Applies tag, collider, scale, and visual settings to a spawned clone.
    private void ConfigureCloneInstance(ReplayClone2D clone)
    {
        if (clone == null)
        {
            return;
        }

        clone.gameObject.tag = "Clone";

        if (playerVisualSource == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerVisualSource = player.transform;
            }
        }

        if (playerVisualSource != null)
        {
            clone.transform.localScale = playerVisualSource.localScale;

            BoxCollider2D playerCollider = playerVisualSource.GetComponent<BoxCollider2D>();
            BoxCollider2D cloneCollider = clone.GetComponent<BoxCollider2D>();
            if (playerCollider != null && cloneCollider != null)
            {
                cloneCollider.offset = playerCollider.offset;
                cloneCollider.size = playerCollider.size;
            }
        }

        SimpleHumanoidVisual visual = clone.GetComponent<SimpleHumanoidVisual>();
        if (visual != null)
        {
            visual.enabled = false;
        }

        SpriteRenderer rootRenderer = clone.GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
        }

        SpriteRenderer visualRenderer = EnsureCloneVisual(clone.transform);
        if (visualRenderer != null)
        {
            ConfigureCloneVisualFromPlayer(visualRenderer);
            Debug.Log($"CloneManager: Visual child configured for {clone.name}. Sprite={(visualRenderer.sprite != null ? visualRenderer.sprite.name : "<none>")}, enabled={visualRenderer.enabled}, color={visualRenderer.color}, sortingOrder={visualRenderer.sortingOrder}");
        }
        else
        {
            Debug.LogError($"CloneManager: Failed to create visual child for {clone.name}!");
        }
    }

    // Configures the clone child renderer to use the clone sprite artwork.
    private void ConfigureCloneVisualFromPlayer(SpriteRenderer visualRenderer)
    {
        Sprite cloneSprite = GetCloneSprite();

        visualRenderer.enabled = true;
        visualRenderer.drawMode = SpriteDrawMode.Simple;
        visualRenderer.maskInteraction = SpriteMaskInteraction.None;
        visualRenderer.sortingLayerID = 0;
        visualRenderer.sortingOrder = 40;
        visualRenderer.material = SpriteBoxFactory.DefaultMaterial;

        if (cloneSprite != null)
        {
            visualRenderer.sprite = cloneSprite;
            visualRenderer.flipX = cloneSourceSpriteFacesLeft;
            visualRenderer.flipY = false;
            visualRenderer.color = Color.white;
            AlignCloneVisualToBody(visualRenderer.transform, cloneSprite);
        }
        else
        {
            visualRenderer.sprite = SpriteBoxFactory.WhiteSprite;
            visualRenderer.flipX = false;
            visualRenderer.flipY = false;
            visualRenderer.color = new Color(0.35f, 0.9f, 1f, 0.92f);
        }
    }

    // Loads and caches the clone sprite from Resources.
    private static Sprite GetCloneSprite()
    {
        cachedCloneSprite ??= Resources.Load<Sprite>(CloneSpriteResourcePath);
        return cachedCloneSprite;
    }

    // Finds or creates the child SpriteRenderer used for clone artwork.
    private SpriteRenderer EnsureCloneVisual(Transform cloneTransform)
    {
        Transform visualChild = cloneTransform.Find("CloneVisual");
        if (visualChild == null)
        {
            GameObject visualObject = new GameObject("CloneVisual");
            visualChild = visualObject.transform;
            visualChild.SetParent(cloneTransform, false);
        }

        visualChild.localPosition = new Vector3(0f, 0f, -0.1f);
        visualChild.localRotation = Quaternion.identity;
        visualChild.localScale = Vector3.one;
        visualChild.gameObject.layer = cloneTransform.gameObject.layer;
        visualChild.gameObject.SetActive(true);

        SpriteRenderer renderer = visualChild.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = visualChild.gameObject.AddComponent<SpriteRenderer>();
        }

        return renderer;
    }

    // Scales and positions clone artwork to match the clone collider.
    private void AlignCloneVisualToBody(Transform visual, Sprite sprite)
    {
        if (visual == null || sprite == null)
        {
            return;
        }

        BoxCollider2D cloneCollider = visual.GetComponentInParent<BoxCollider2D>();
        Vector2 targetSize = cloneCollider != null ? cloneCollider.size : new Vector2(0.65f, 1.05f);
        Vector2 targetOffset = cloneCollider != null ? cloneCollider.offset : Vector2.zero;
        Bounds spriteBounds = sprite.bounds;

        if (spriteBounds.size.y <= Mathf.Epsilon)
        {
            return;
        }

        float uniformScale = targetSize.y / spriteBounds.size.y;
        float colliderBottom = targetOffset.y - targetSize.y * 0.5f;
        float localX = targetOffset.x - spriteBounds.center.x * uniformScale;
        float localY = colliderBottom - spriteBounds.min.y * uniformScale;

        visual.localPosition = new Vector3(localX, localY, -0.1f);
        visual.localScale = new Vector3(uniformScale, uniformScale, 1f);
    }

    // Removes a single clone from tracking and destroys its GameObject.
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

    // Removes and destroys every active clone.
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

    // Returns the currently tracked active clones.
    public IReadOnlyList<ReplayClone2D> GetActiveClones() => activeClones;

    // Pauses all active clone replays.
    public void PauseAllClones()
    {
        foreach (var clone in activeClones)
        {
            if (clone != null) clone.Pause();
        }
    }

    // Resumes all paused clone replays.
    public void ResumeAllClones()
    {
        foreach (var clone in activeClones)
        {
            if (clone != null) clone.Resume();
        }
    }
}
