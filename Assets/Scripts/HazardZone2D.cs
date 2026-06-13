using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class HazardZone2D : MonoBehaviour, IResettable
{
    [SerializeField] private Color activeColor = new Color(0.95f, 0.2f, 0.12f, 1f);
    [SerializeField] private Color warningColor = new Color(1f, 0.65f, 0.15f, 1f);
    [SerializeField] private bool affectsClones = true;

    private const string AnimatedVisualName = "AnimatedVisual";

    private BoxCollider2D hazardCollider;
    private SpriteRenderer spriteRenderer;
    private Transform animatedVisual;
    private Transform lavaBase;
    private Transform lavaCore;
    private Transform glowBand;
    private readonly Transform[] spikes = new Transform[6];
    private float pulseTime;
    private bool hasSceneAnimatedVisual;

    // Initializes the hazard collider and chooses scene-provided or generated visuals.
    private void Awake()
    {
        hazardCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animatedVisual = transform.Find(AnimatedVisualName);
        hazardCollider.isTrigger = true;
        hasSceneAnimatedVisual = animatedVisual != null;
        if (!hasSceneAnimatedVisual && gameObject.name == "SpikeShelf")
        {
            EnsureAnimatedFireTrapVisual();
            hasSceneAnimatedVisual = true;
        }
        EnsureVisual();
        ApplyVisual(0f);
    }

    // Pulses generated hazard visuals over time.
    private void Update()
    {
        pulseTime += Time.deltaTime * 4f;
        ApplyVisual((Mathf.Sin(pulseTime) + 1f) * 0.5f);
    }

    // Handles damage while an object remains inside the hazard.
    private void OnTriggerStay2D(Collider2D other)
    {
        HandleHazardContact(other);
    }

    // Handles damage as soon as an object enters the hazard.
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHazardContact(other);
    }

    // Resets the hazard visual pulse to its starting state.
    public void ResetState()
    {
        pulseTime = 0f;
        ApplyVisual(0f);
    }

    // Resets players or disables clones that touch the active hazard area.
    private void HandleHazardContact(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (!ShouldAffect(other))
            {
                return;
            }

            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.ResetState();
            }
            return;
        }

        if (affectsClones && other.CompareTag("Clone"))
        {
            if (!ShouldAffect(other))
            {
                return;
            }

            ReplayClone2D clone = other.GetComponent<ReplayClone2D>();
            if (clone != null)
            {
                clone.Stop();
                other.gameObject.SetActive(false);
            }
        }
    }

    // Checks whether the target bounds actually overlap the visible hazard area.
    private bool ShouldAffect(Collider2D target)
    {
        if (hazardCollider == null || target == null)
        {
            return false;
        }

        Bounds hazardBounds = hazardCollider.bounds;
        if (hasSceneAnimatedVisual && animatedVisual != null)
        {
            Vector3 visualOffset = animatedVisual.position - transform.position;
            hazardBounds.center += new Vector3(visualOffset.x, visualOffset.y, 0f);
        }

        return hazardBounds.Intersects(target.bounds);
    }

    // Creates generated lava visuals when no animated scene visual is present.
    private void EnsureVisual()
    {
        if (hasSceneAnimatedVisual)
        {
            HideGeneratedVisuals();
            return;
        }

        RestoreRootSpriteRenderer();

        Vector2 size = hazardCollider != null ? hazardCollider.size : new Vector2(2f, 0.3f);

        lavaBase = EnsurePart("LavaBase", new Vector3(0f, -0.01f, 0f), new Vector2(size.x, Mathf.Max(0.16f, size.y)), activeColor, 2);
        lavaCore = EnsurePart("LavaCore", new Vector3(0f, 0.01f, 0f), new Vector2(size.x * 0.88f, Mathf.Max(0.08f, size.y * 0.45f)), warningColor, 3);
        glowBand = EnsurePart("GlowBand", new Vector3(0f, size.y * 0.18f, 0f), new Vector2(size.x * 0.94f, 0.05f), new Color(1f, 0.95f, 0.55f, 0.85f), 5);

        float step = size.x / spikes.Length;
        for (int i = 0; i < spikes.Length; i++)
        {
            float x = -size.x * 0.5f + step * (i + 0.5f);
            float spikeHeight = Mathf.Max(0.14f, size.y * (i % 2 == 0 ? 0.95f : 0.72f));
            spikes[i] = EnsurePart($"Spike_{i}", new Vector3(x, size.y * 0.45f, 0f), new Vector2(step * 0.52f, spikeHeight), new Color(0.24f, 0.08f, 0.08f, 1f), 4);
        }
    }

    // Applies color pulsing to generated hazard visual parts.
    private void ApplyVisual(float pulse)
    {
        EnsureVisual();

        if (hasSceneAnimatedVisual)
        {
            return;
        }

        SetPartColor(lavaBase, Color.Lerp(activeColor, warningColor, pulse * 0.35f));
        SetPartColor(lavaCore, Color.Lerp(new Color(1f, 0.45f, 0.12f, 1f), new Color(1f, 0.82f, 0.22f, 1f), pulse * 0.7f));
        SetPartColor(glowBand, Color.Lerp(new Color(1f, 0.95f, 0.55f, 0.55f), new Color(1f, 1f, 0.75f, 0.95f), pulse));

        for (int i = 0; i < spikes.Length; i++)
        {
            if (spikes[i] == null)
            {
                continue;
            }

            float offsetPulse = (pulse + i * 0.13f) % 1f;
            SetPartColor(spikes[i], Color.Lerp(new Color(0.22f, 0.07f, 0.07f, 1f), new Color(0.5f, 0.16f, 0.12f, 1f), offsetPulse * 0.5f));
        }
    }

    // Hides generated parts when an animated scene visual is used.
    private void HideGeneratedVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        SetActiveIfExists(lavaBase, false);
        SetActiveIfExists(lavaCore, false);
        SetActiveIfExists(glowBand, false);

        for (int i = 0; i < spikes.Length; i++)
        {
            SetActiveIfExists(spikes[i], false);
        }
    }

    // Restores the root renderer for generated hazard visuals.
    private void RestoreRootSpriteRenderer()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.enabled = true;
        spriteRenderer.sprite = SpriteBoxFactory.WhiteSprite;
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteRenderer.color = activeColor;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        spriteRenderer.size = hazardCollider != null ? hazardCollider.size : new Vector2(2f, 0.3f);
    }

    // Finds or creates one generated visual part for the hazard.
    private Transform EnsurePart(string partName, Vector3 localPosition, Vector2 size, Color color, int sortingOrder)
    {
        Transform part = transform.Find(partName);
        if (part == null)
        {
            GameObject child = new GameObject(partName);
            child.transform.SetParent(transform, false);
            part = child.transform;
        }

        if (!part.gameObject.activeSelf)
        {
            part.gameObject.SetActive(true);
        }

        part.localPosition = localPosition;
        part.localRotation = Quaternion.identity;
        part.localScale = Vector3.one;

        SpriteRenderer renderer = part.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = part.gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = SpriteBoxFactory.WhiteSprite;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        renderer.maskInteraction = SpriteMaskInteraction.None;

        return part;
    }

    // Creates the animated fire trap child visual for shelf-style hazards.
    private void EnsureAnimatedFireTrapVisual()
    {
        GameObject child = new GameObject(AnimatedVisualName);
        child.transform.SetParent(transform, false);
        child.transform.localPosition = new Vector3(0f, 0.42f, 0f);
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = new Vector3(1.25f, 1.75f, 1f);

        SpriteRenderer visualRenderer = child.AddComponent<SpriteRenderer>();
        visualRenderer.sortingOrder = 6;
        visualRenderer.maskInteraction = SpriteMaskInteraction.None;
        visualRenderer.color = Color.white;

        SpriteFrameAnimator2D animator = child.AddComponent<SpriteFrameAnimator2D>();
        animator.Configure("Art/Fire Trap", 7, 10f, Color.white);

        animatedVisual = child.transform;
    }

    // Applies a color to a generated visual part if it exists.
    private static void SetPartColor(Transform part, Color color)
    {
        if (part == null)
        {
            return;
        }

        SpriteRenderer renderer = part.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
        }
    }

    // Toggles a generated visual part only when it exists.
    private static void SetActiveIfExists(Transform part, bool active)
    {
        if (part != null)
        {
            part.gameObject.SetActive(active);
        }
    }
}
