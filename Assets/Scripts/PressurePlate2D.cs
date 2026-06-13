using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PressurePlate2D : MonoBehaviour, IResettable
{
    [SerializeField] private Vector2 detectionPadding = new Vector2(0.1f, 0.45f);
    [SerializeField] private float pressedDrop = 0.04f;

    private BoxCollider2D plateCollider;
    private SpriteRenderer spriteRenderer;
    private Vector3 restPosition;

    public bool IsPressed { get; private set; }

    // Initializes the plate collider and stores its unpressed position.
    private void Awake()
    {
        plateCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        plateCollider.isTrigger = true;
        restPosition = transform.position;
    }

    // Refreshes the pressed state during the physics step.
    private void FixedUpdate()
    {
        SetPressed(HasWeight());
    }

    // Restores the plate to its unpressed state.
    public void ResetState()
    {
        SetPressed(false);
    }

    // Detects whether a player, clone, or crate is pressing the plate.
    private bool HasWeight()
    {
        Bounds bounds = plateCollider.bounds;
        Vector2 size = new Vector2(bounds.size.x + detectionPadding.x, bounds.size.y + detectionPadding.y);
        Vector2 center = new Vector2(bounds.center.x, bounds.center.y + detectionPadding.y * 0.35f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit == plateCollider)
            {
                continue;
            }

            if (hit.CompareTag("Player") || hit.CompareTag("Clone") || hit.GetComponent<PushableCrate2D>() != null)
            {
                return true;
            }
        }

        return false;
    }

    // Updates the plate position and color for the requested pressed state.
    private void SetPressed(bool pressed)
    {
        IsPressed = pressed;
        transform.position = restPosition + (pressed ? Vector3.down * pressedDrop : Vector3.zero);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = pressed ? new Color(1f, 0.95f, 0.25f, 1f) : new Color(1f, 0.72f, 0.05f, 1f);
        }
    }
}
