using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour, IResettable
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 5.4f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float groundedNormalThreshold = 0.35f;
    [SerializeField] private bool controlledByReplay;
    [SerializeField] private LayerMask groundMask = Physics2D.AllLayers;

    private Rigidbody2D rb;
    private BoxCollider2D bodyCollider;
    private SpriteRenderer spriteRenderer;
    private SimpleHumanoidVisual humanoidVisual;
    private Vector2 startPosition;
    private float replayMoveX;
    private bool replayJumpPressed;
    private RigidbodyType2D originalBodyType;
    private float originalGravityScale;
    private int jumpsUsed;
    private bool wasGrounded;

    public float MoveX { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public Vector2 Velocity => rb != null ? rb.velocity : Vector2.zero;
    public bool IsReplay => controlledByReplay;

    // Caches components and applies the initial physics mode.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        humanoidVisual = GetComponent<SimpleHumanoidVisual>();
        if (humanoidVisual != null)
        {
            humanoidVisual.SetFacingDirection(1f);
        }
        startPosition = rb.position;
        rb.freezeRotation = true;
        originalBodyType = rb.bodyType;
        originalGravityScale = rb.gravityScale;
        ApplyReplayPhysicsMode(controlledByReplay);
        SnapToGroundIfNeeded();
        startPosition = rb.position;
    }

    // Reads live or replay input and handles jump state each frame.
    private void Update()
    {
        if (controlledByReplay)
        {
            MoveX = replayMoveX;
            JumpPressedThisFrame = replayJumpPressed;
            replayJumpPressed = false;
            return;
        }

        MoveX = Input.GetAxisRaw("Horizontal");
        JumpPressedThisFrame = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        bool grounded = IsGrounded();

        if (grounded && rb.velocity.y <= 0.01f)
        {
            jumpsUsed = 0;
        }
        else if (!grounded && wasGrounded && jumpsUsed == 0)
        {
            jumpsUsed = 1;
        }

        if (JumpPressedThisFrame && jumpsUsed < maxJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpsUsed++;
            grounded = false;
        }

        if (humanoidVisual != null)
        {
            humanoidVisual.SetFacingDirection(MoveX);
            humanoidVisual.SetWalking(Mathf.Abs(MoveX) > 0.05f && grounded);
        }
        else if (spriteRenderer != null && Mathf.Abs(MoveX) > 0.05f)
        {
            spriteRenderer.flipX = MoveX < 0f;
        }

        wasGrounded = grounded;
    }

    // Applies live horizontal movement during the physics step.
    private void FixedUpdate()
    {
        if (controlledByReplay)
        {
            return;
        }

        rb.velocity = new Vector2(MoveX * moveSpeed, rb.velocity.y);
    }

    // Switches this controller between live input and replay playback.
    public void SetReplayMode(bool replay)
    {
        controlledByReplay = replay;
        if (rb != null)
        {
            ApplyReplayPhysicsMode(replay);
        }
    }

    // Applies recorded input values from a replay frame.
    public void ApplyReplayFrame(RecordedFrame frame)
    {
        replayMoveX = frame.MoveX;
        if (frame.JumpPressed)
        {
            replayJumpPressed = true;
        }
    }

    // Forces the character to a recorded position and velocity.
    public void ForceState(Vector3 position, Vector2 velocity)
    {
        rb.position = position;
        rb.velocity = controlledByReplay ? Vector2.zero : velocity;
    }

    // Clears any held replay input values and horizontal velocity.
    public void ClearReplayInput()
    {
        replayMoveX = 0f;
        replayJumpPressed = false;
        MoveX = 0f;
        JumpPressedThisFrame = false;
        rb.velocity = controlledByReplay ? Vector2.zero : new Vector2(0f, rb.velocity.y);
    }

    // Restores the character to its starting position and movement state.
    public void ResetState()
    {
        rb.position = startPosition;
        rb.velocity = Vector2.zero;
        replayMoveX = 0f;
        replayJumpPressed = false;
        MoveX = 0f;
        JumpPressedThisFrame = false;
        jumpsUsed = 0;
        wasGrounded = false;
        SnapToGroundIfNeeded();
    }

    // Adjusts Rigidbody behavior so replay clones follow recorded frames cleanly.
    private void ApplyReplayPhysicsMode(bool replay)
    {
        rb.bodyType = replay ? RigidbodyType2D.Kinematic : originalBodyType;
        rb.gravityScale = replay ? 0f : originalGravityScale;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    // Checks whether the body has non-trigger ground directly beneath it.
    private bool IsGrounded()
    {
        if (bodyCollider == null)
        {
            return false;
        }

        Bounds bounds = bodyCollider.bounds;
        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, 0.08f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, groundMask);
        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == bodyCollider || hit.isTrigger)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    // Snaps the live player down to nearby ground after scene setup or reset.
    private void SnapToGroundIfNeeded()
    {
        if (bodyCollider == null || rb == null || controlledByReplay)
        {
            return;
        }

        Bounds bounds = bodyCollider.bounds;
        Vector2 castOrigin = new Vector2(bounds.center.x, bounds.center.y + 0.05f);
        Vector2 castSize = new Vector2(bounds.size.x * 0.9f, bounds.size.y);
        RaycastHit2D hit = Physics2D.BoxCast(castOrigin, castSize, 0f, Vector2.down, 3f, groundMask);

        if (hit.collider == null || hit.collider.isTrigger)
        {
            return;
        }

        if (hit.normal.y < groundedNormalThreshold)
        {
            return;
        }

        float bottomOffsetFromCenter = bounds.center.y - bounds.min.y;
        float targetCenterY = hit.point.y + bottomOffsetFromCenter + 0.01f;
        rb.position = new Vector2(rb.position.x, rb.position.y + (targetCenterY - bounds.center.y));
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }
}
