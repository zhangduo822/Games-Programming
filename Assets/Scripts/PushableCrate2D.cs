using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PushableCrate2D : MonoBehaviour, IResettable
{
    [SerializeField] private float pushForce = 1.35f;
    [SerializeField] private float maxHorizontalSpeed = 1.8f;
    [SerializeField] private float idleSlowdown = 10f;
    [SerializeField] private LayerMask pusherMask = ~0;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Vector3 startPosition;
    private Vector2 startVelocity;

    public bool IsHeld { get; private set; }
    public Vector2 Velocity => rb != null ? rb.velocity : Vector2.zero;

    // Initializes crate physics and stores its starting position.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        startPosition = transform.position;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.mass = Mathf.Max(rb.mass, 4f);
        rb.drag = Mathf.Max(rb.drag, 4f);
        rb.angularDrag = Mathf.Max(rb.angularDrag, 8f);
    }

    // Applies crate speed limits and slows it when no one is pushing.
    private void FixedUpdate()
    {
        IsHeld = IsBeingPressed();
        ClampHorizontalSpeed();

        if (!IsHeld && rb != null)
        {
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, 0f, idleSlowdown * Time.fixedDeltaTime),
                rb.velocity.y
            );
        }
    }

    // Checks whether the player or a clone is currently touching the crate.
    private bool IsBeingPressed()
    {
        Bounds bounds = boxCollider.bounds;
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

    // Applies a controlled horizontal push in the requested direction.
    public void ApplyPushForce(Vector2 direction)
    {
        if (rb != null && direction.magnitude > 0.1f)
        {
            Vector2 pushVelocity = direction.normalized * pushForce;
            rb.velocity = new Vector2(pushVelocity.x, rb.velocity.y);
            ClampHorizontalSpeed();
        }
    }

    // Forces the crate to a specific recorded position and velocity.
    public void ForceState(Vector3 position, Vector2 velocity)
    {
        transform.position = position;
        rb.velocity = velocity;
    }

    // Returns the crate to its starting position with no velocity.
    public void ResetState()
    {
        transform.position = startPosition;
        rb.velocity = Vector2.zero;
        startVelocity = Vector2.zero;
    }

    // Updates the position used when resetting this crate.
    public void SetStartPosition(Vector3 position)
    {
        startPosition = position;
    }

    // Keeps the crate from sliding faster than the intended speed.
    private void ClampHorizontalSpeed()
    {
        if (rb == null)
        {
            return;
        }

        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed), rb.velocity.y);
    }
}
