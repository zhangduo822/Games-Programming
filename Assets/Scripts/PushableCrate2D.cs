using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PushableCrate2D : MonoBehaviour, IResettable
{
    [SerializeField] private float pushForce = 3f;
    [SerializeField] private LayerMask pusherMask = ~0;

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Vector2 startVelocity;

    public bool IsHeld { get; private set; }
    public Vector2 Velocity => rb != null ? rb.velocity : Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        IsHeld = IsBeingPressed();
    }

    private bool IsBeingPressed()
    {
        Bounds bounds = GetComponent<BoxCollider2D>().bounds;
        Vector2 size = new Vector2(bounds.size.x + 0.3f, bounds.size.y + 0.3f);
        Vector2 center = (Vector2)bounds.center + Vector2.up * 0.1f;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, pusherMask);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Clone"))
            {
                return true;
            }
        }
        return false;
    }

    public void ApplyPushForce(Vector2 direction)
    {
        if (rb != null && direction.magnitude > 0.1f)
        {
            rb.velocity = direction.normalized * pushForce;
        }
    }

    public void ForceState(Vector3 position, Vector2 velocity)
    {
        transform.position = position;
        rb.velocity = velocity;
    }

    public void ResetState()
    {
        transform.position = startPosition;
        rb.velocity = Vector2.zero;
        startVelocity = Vector2.zero;
    }

    public void SetStartPosition(Vector3 position)
    {
        startPosition = position;
    }
}
