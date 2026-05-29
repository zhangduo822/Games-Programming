using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Door2D : MonoBehaviour, IResettable
{
    private BoxCollider2D doorCollider;
    private SpriteRenderer spriteRenderer;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider.isTrigger = false;
        Close();
    }

    public void Open()
    {
        IsOpen = true;
        doorCollider.enabled = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.95f, 0.25f, 0.25f, 0.25f);
        }
    }

    public void Close()
    {
        IsOpen = false;
        doorCollider.enabled = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.95f, 0.25f, 0.25f, 1f);
        }
    }

    public void ResetState()
    {
        Close();
    }
}
