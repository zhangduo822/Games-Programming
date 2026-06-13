using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Door2D : MonoBehaviour, IResettable
{
    private const string DoorSpriteResourcePath = "Art/door";

    private static Sprite cachedDoorSprite;

    private BoxCollider2D doorCollider;
    private SpriteRenderer spriteRenderer;

    public bool IsOpen { get; private set; }

    // Initializes the door collider and visual state when the object is created.
    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider.isTrigger = false;
        EnsureVisual();
        Close();
    }

    // Opens the door by disabling its collider and fading its sprite.
    public void Open()
    {
        IsOpen = true;
        doorCollider.enabled = false;
        EnsureVisual();
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.38f);
    }

    // Closes the door by restoring its collider and full sprite opacity.
    public void Close()
    {
        IsOpen = false;
        doorCollider.enabled = true;
        EnsureVisual();
        spriteRenderer.color = Color.white;
    }

    // Resets the door to its default closed state.
    public void ResetState()
    {
        Close();
    }

    // Applies the configured door sprite and renderer settings.
    private void EnsureVisual()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Sprite doorSprite = GetDoorSprite();
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = doorSprite != null ? doorSprite : SpriteBoxFactory.WhiteSprite;
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        if (doorCollider != null)
        {
            spriteRenderer.size = doorCollider.size;
        }
        spriteRenderer.sortingOrder = 8;
        spriteRenderer.material = SpriteBoxFactory.DefaultMaterial;
    }

    // Loads and caches the door sprite from Resources.
    private static Sprite GetDoorSprite()
    {
        if (cachedDoorSprite != null)
        {
            return cachedDoorSprite;
        }

        cachedDoorSprite = Resources.Load<Sprite>(DoorSpriteResourcePath);
        return cachedDoorSprite;
    }
}
