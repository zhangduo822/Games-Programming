using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class SimpleHumanoidVisual : MonoBehaviour
{
    private const string PlayerSpriteResourcePath = "Art/Hero1";
    private const string CloneSpriteResourcePath = "Art/Hero2";
    private const string WalkSpriteResourcePrefix = "Art/Walk";
    private const string CharacterVisualName = "CharacterSpriteVisual";

    public enum VisualRole
    {
        Player,
        Clone
    }

    [SerializeField] private int pixelsPerUnit = 16;
    [SerializeField] private Color tunicColor = new(0.25f, 0.47f, 0.93f, 1f);
    [SerializeField] private Color pantsColor = new(0.16f, 0.22f, 0.45f, 1f);
    [SerializeField] private Color skinColor = new(0.97f, 0.84f, 0.72f, 1f);
    [SerializeField] private Color hairColor = new(0.14f, 0.1f, 0.08f, 1f);
    [SerializeField] private Color outlineColor = new(0.07f, 0.09f, 0.14f, 1f);
    [SerializeField] private VisualRole visualRole;
    [SerializeField] private bool useAutoPlayerSprite = true;
    [SerializeField] private bool sourceSpriteFacesLeft = true;
    [SerializeField] private int walkFrameCount = 10;
    [SerializeField] private float walkFramesPerSecond = 10f;

    private static Sprite whiteSprite;
    private static Sprite cachedPlayerSprite;
    private static Sprite cachedCloneSprite;
    private static Sprite[] cachedWalkSprites;
    private bool facingRight = true;
    private bool isWalking;
    private float walkElapsed;
    private int currentWalkFrame = -1;

    // Builds the character visual when the component starts.
    private void Awake()
    {
        EnsureVisual();
    }

    // Advances walking animation while the game is running.
    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        UpdateWalkAnimation();
    }

    // Rebuilds the visual when the component is reset in the editor.
    private void Reset()
    {
        EnsureVisual();
    }

    // Refreshes the visual after inspector value changes.
    private void OnValidate()
    {
        EnsureVisual();
    }

    // Sets whether this visual represents the player or a clone.
    public void SetVisualRole(VisualRole role)
    {
        visualRole = role;
        if (visualRole == VisualRole.Clone)
        {
            useAutoPlayerSprite = false;
        }
        EnsureVisual();
    }

    // Updates the facing direction from horizontal movement input.
    public void SetFacingDirection(float horizontalDirection)
    {
        if (Mathf.Abs(horizontalDirection) <= 0.05f)
        {
            return;
        }

        facingRight = horizontalDirection > 0f;
        ApplyFacingDirection();
    }

    // Enables or disables walking animation playback.
    public void SetWalking(bool walking)
    {
        if (visualRole != VisualRole.Player)
        {
            return;
        }

        if (isWalking == walking)
        {
            return;
        }

        isWalking = walking;
        walkElapsed = 0f;
        currentWalkFrame = -1;

        if (!isWalking)
        {
            ShowPlayerIdleSprite();
        }
    }

    // Chooses between sprite artwork and generated body parts for this character.
    private void EnsureVisual()
    {
        bool isClone = visualRole == VisualRole.Clone;
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        bool shouldDeferToRuntimeAnimator = Application.isPlaying && !isClone && GetComponent<SpriteFrameAnimator2D>() != null;

        if (shouldDeferToRuntimeAnimator)
        {
            if (rootRenderer != null)
            {
                rootRenderer.enabled = rootRenderer.sprite != null;
                rootRenderer.color = Color.white;
                rootRenderer.drawMode = SpriteDrawMode.Simple;
                rootRenderer.maskInteraction = SpriteMaskInteraction.None;
                rootRenderer.sortingLayerID = 0;
                rootRenderer.sortingOrder = 10;
            }

            HideGeneratedParts();
            return;
        }

        if (rootRenderer != null && isClone)
        {
            rootRenderer.enabled = false;
            EnsureCharacterSpriteVisual(GetCloneSprite(), Color.white, 25);
            HideGeneratedParts(CharacterVisualName);
            return;
        }

        Sprite characterSprite = useAutoPlayerSprite ? GetCharacterSprite() : null;
        if (rootRenderer != null && characterSprite != null)
        {
            rootRenderer.enabled = false;
            EnsureCharacterSpriteVisual(characterSprite, Color.white, 10);
            HideGeneratedParts(CharacterVisualName);
            return;
        }

        if (rootRenderer != null && !useAutoPlayerSprite)
        {
            rootRenderer.enabled = rootRenderer.sprite != null;
            rootRenderer.drawMode = SpriteDrawMode.Simple;
            rootRenderer.maskInteraction = SpriteMaskInteraction.None;
            rootRenderer.sortingLayerID = 0;
            rootRenderer.sortingOrder = 10;
            HideGeneratedParts();
            return;
        }

        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
        }

        HideCharacterSpriteVisual();

        Color finalTunic = tunicColor;
        Color finalPants = pantsColor;
        Color finalSkin = skinColor;
        Color finalHair = hairColor;
        Color finalOutline = outlineColor;

        ShowGeneratedParts();

        EnsurePart("Body", 0, 1, 8, 6, finalTunic, 22);
        EnsurePart("Belt", 0, -1, 8, 1, finalOutline, 23);
        EnsurePart("Head", 0, 8, 7, 6, finalSkin, 24);
        EnsurePart("Hair", 0, 10, 8, 2, finalHair, 25);
        EnsurePart("EyeL", -2, 8, 1, 1, finalOutline, 26);
        EnsurePart("EyeR", 2, 8, 1, 1, finalOutline, 26);
        EnsurePart("ArmL", -5, 1, 2, 6, finalSkin, 21);
        EnsurePart("ArmR", 5, 1, 2, 6, finalSkin, 21);
        EnsurePart("HandL", -5, -2, 2, 2, finalSkin, 22);
        EnsurePart("HandR", 5, -2, 2, 2, finalSkin, 22);
        EnsurePart("LegL", -2, -7, 3, 7, finalPants, 20);
        EnsurePart("LegR", 2, -7, 3, 7, finalPants, 20);
        EnsurePart("FootL", -2, -11, 3, 2, finalOutline, 19);
        EnsurePart("FootR", 2, -11, 3, 2, finalOutline, 19);

        EnsureAccessory(isClone);
    }

    // Loads and caches the player idle sprite.
    private static Sprite GetCharacterSprite()
    {
        cachedPlayerSprite ??= Resources.Load<Sprite>(PlayerSpriteResourcePath);
        return cachedPlayerSprite;
    }

    // Loads and caches the clone idle sprite.
    private static Sprite GetCloneSprite()
    {
        cachedCloneSprite ??= Resources.Load<Sprite>(CloneSpriteResourcePath);
        return cachedCloneSprite;
    }

    // Loads and caches the configured walk animation sprites.
    private static Sprite[] GetWalkSprites(int frameCount)
    {
        int count = Mathf.Max(1, frameCount);
        if (cachedWalkSprites != null && cachedWalkSprites.Length == count)
        {
            return cachedWalkSprites;
        }

        cachedWalkSprites = new Sprite[count];
        for (int i = 0; i < cachedWalkSprites.Length; i++)
        {
            cachedWalkSprites[i] = Resources.Load<Sprite>($"{WalkSpriteResourcePrefix}{i:00}");
        }

        return cachedWalkSprites;
    }

    // Creates or updates the child renderer used for character artwork.
    private void EnsureCharacterSpriteVisual(Sprite characterSprite, Color color, int sortingOrder)
    {
        if (characterSprite == null)
        {
            return;
        }

        Transform visual = transform.Find(CharacterVisualName);
        if (visual == null)
        {
            GameObject child = new GameObject(CharacterVisualName);
            child.transform.SetParent(transform, false);
            visual = child.transform;
        }

        if (!visual.gameObject.activeSelf)
        {
            visual.gameObject.SetActive(true);
        }

        visual.localRotation = Quaternion.identity;

        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = visual.gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.enabled = true;
        renderer.sprite = characterSprite;
        renderer.color = color;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.maskInteraction = SpriteMaskInteraction.None;
        renderer.sortingLayerID = 0;
        renderer.sortingOrder = sortingOrder;

        AlignCharacterSpriteToBody(visual, characterSprite);
        ApplyFacingDirection(renderer);
    }

    // Swaps walk frames while the player is moving.
    private void UpdateWalkAnimation()
    {
        if (!isWalking || visualRole != VisualRole.Player)
        {
            return;
        }

        Sprite[] walkSprites = GetWalkSprites(walkFrameCount);
        if (walkSprites == null || walkSprites.Length == 0 || walkSprites[0] == null)
        {
            ShowPlayerIdleSprite();
            return;
        }

        walkElapsed += Time.deltaTime;
        int frameIndex = Mathf.FloorToInt(walkElapsed * Mathf.Max(0.1f, walkFramesPerSecond)) % walkSprites.Length;
        if (frameIndex == currentWalkFrame || walkSprites[frameIndex] == null)
        {
            return;
        }

        currentWalkFrame = frameIndex;
        SpriteRenderer renderer = GetCharacterSpriteRenderer();
        if (renderer == null)
        {
            EnsureVisual();
            renderer = GetCharacterSpriteRenderer();
        }

        if (renderer == null)
        {
            return;
        }

        renderer.sprite = walkSprites[frameIndex];
        AlignCharacterSpriteToBody(renderer.transform, walkSprites[frameIndex]);
        ApplyFacingDirection(renderer);
    }

    // Restores the player idle sprite when walking stops.
    private void ShowPlayerIdleSprite()
    {
        Sprite idleSprite = GetCharacterSprite();
        if (idleSprite == null)
        {
            return;
        }

        SpriteRenderer renderer = GetCharacterSpriteRenderer();
        if (renderer == null)
        {
            EnsureCharacterSpriteVisual(idleSprite, Color.white, 10);
            return;
        }

        renderer.sprite = idleSprite;
        renderer.color = Color.white;
        renderer.sortingOrder = 10;
        AlignCharacterSpriteToBody(renderer.transform, idleSprite);
        ApplyFacingDirection(renderer);
    }

    // Returns the SpriteRenderer used by the child character artwork.
    private SpriteRenderer GetCharacterSpriteRenderer()
    {
        Transform visual = transform.Find(CharacterVisualName);
        return visual != null ? visual.GetComponent<SpriteRenderer>() : null;
    }

    // Scales and positions character artwork to match the body collider.
    private void AlignCharacterSpriteToBody(Transform visual, Sprite characterSprite)
    {
        if (visual == null || characterSprite == null)
        {
            return;
        }

        BoxCollider2D bodyCollider = GetComponent<BoxCollider2D>();
        Vector2 targetSize = bodyCollider != null ? bodyCollider.size : new Vector2(0.65f, 1.05f);
        Vector2 targetOffset = bodyCollider != null ? bodyCollider.offset : Vector2.zero;
        Bounds spriteBounds = characterSprite.bounds;

        if (spriteBounds.size.y <= Mathf.Epsilon)
        {
            return;
        }

        float uniformScale = targetSize.y / spriteBounds.size.y;
        float colliderBottom = targetOffset.y - targetSize.y * 0.5f;
        float localX = targetOffset.x - spriteBounds.center.x * uniformScale;
        float localY = colliderBottom - spriteBounds.min.y * uniformScale;

        visual.localPosition = new Vector3(localX, localY, 0f);
        visual.localScale = new Vector3(uniformScale, uniformScale, 1f);
    }

    // Hides the child artwork renderer when generated parts are used.
    private void HideCharacterSpriteVisual()
    {
        Transform visual = transform.Find(CharacterVisualName);
        if (visual != null)
        {
            visual.gameObject.SetActive(false);
        }
    }

    // Applies the current facing direction to the active artwork renderer.
    private void ApplyFacingDirection()
    {
        Transform visual = transform.Find(CharacterVisualName);
        if (visual == null)
        {
            return;
        }

        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            ApplyFacingDirection(renderer);
        }
    }

    // Flips a SpriteRenderer according to the stored facing direction.
    private void ApplyFacingDirection(SpriteRenderer renderer)
    {
        renderer.flipX = sourceSpriteFacesLeft == facingRight;
    }

    // Enables all generated body-part renderers.
    private void ShowGeneratedParts()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
            if (childRenderer != null)
            {
                childRenderer.enabled = true;
            }
        }
    }

    // Disables generated body parts except an optional named child.
    private void HideGeneratedParts(string exceptName = null)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!string.IsNullOrEmpty(exceptName) && child.name == exceptName)
            {
                continue;
            }

            child.gameObject.SetActive(false);
        }
    }

    // Creates small role-specific accessory visuals for player or clone.
    private void EnsureAccessory(bool isClone)
    {
        Transform playerBadge = transform.Find("PlayerBadge");
        if (playerBadge != null)
        {
            playerBadge.gameObject.SetActive(!isClone);
        }

        Transform cloneAura = transform.Find("CloneAura");
        if (cloneAura != null)
        {
            cloneAura.gameObject.SetActive(isClone);
        }

        Transform cloneBadge = transform.Find("CloneBadge");
        if (cloneBadge != null)
        {
            cloneBadge.gameObject.SetActive(isClone);
        }

        if (isClone)
        {
            EnsurePart("CloneAura", 0, 1, 10, 15, new Color(0.3f, 0.92f, 1f, 0.18f), 18);
            EnsurePart("CloneBadge", 0, 13, 5, 2, new Color(0.85f, 1f, 1f, 0.95f), 27);
        }
        else
        {
            EnsurePart("PlayerBadge", 0, 13, 4, 2, new Color(1f, 0.84f, 0.2f, 0.95f), 27);
        }
    }

    // Creates or updates one generated pixel-art body part.
    private void EnsurePart(string partName, int pixelX, int pixelY, int pixelWidth, int pixelHeight, Color color, int sortingOrder)
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

        float unit = 1f / Mathf.Max(1, pixelsPerUnit);
        part.localPosition = new Vector3(pixelX * unit, pixelY * unit, 0f);
        part.localRotation = Quaternion.identity;
        part.localScale = new Vector3(pixelWidth * unit, pixelHeight * unit, 1f);

        SpriteRenderer renderer = part.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = part.gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.enabled = true;
        renderer.sprite = GetWhiteSprite();
        renderer.color = color;
        renderer.sortingLayerID = 0;
        renderer.sortingOrder = sortingOrder;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.maskInteraction = SpriteMaskInteraction.None;
    }

    // Returns the shared white sprite used by generated body parts.
    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite == null)
        {
            whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            whiteSprite.name = "GeneratedWhiteSprite";
        }

        return whiteSprite;
    }
}
