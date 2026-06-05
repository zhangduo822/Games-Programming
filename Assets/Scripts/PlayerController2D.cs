using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour, IResettable
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 5.4f;
    [SerializeField] private float groundedNormalThreshold = 0.35f;
    [SerializeField] private bool controlledByReplay;

    private Rigidbody2D rb;
    private BoxCollider2D bodyCollider;
    private Vector3 startPosition;
    private float replayMoveX;
    private bool replayJumpPressed;

    public float MoveX { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public Vector2 Velocity => rb != null ? rb.velocity : Vector2.zero;
    public bool IsReplay => controlledByReplay;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        startPosition = transform.position;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (controlledByReplay)
        {
            MoveX = replayMoveX;
            JumpPressedThisFrame = replayJumpPressed;
            replayJumpPressed = false;
        }
        else
        {
            MoveX = Input.GetAxisRaw("Horizontal");
            JumpPressedThisFrame = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        }

        if (JumpPressedThisFrame && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(MoveX * moveSpeed, rb.velocity.y);
    }

    public void SetReplayMode(bool replay)
    {
        controlledByReplay = replay;
    }

    public void ApplyReplayFrame(RecordedFrame frame)
    {
        replayMoveX = frame.MoveX;
        if (frame.JumpPressed)
        {
            replayJumpPressed = true;
        }
    }

    public void ForceState(Vector3 position, Vector2 velocity)
    {
        transform.position = position;
        rb.velocity = velocity;
    }

    public void ClearReplayInput()
    {
        replayMoveX = 0f;
        replayJumpPressed = false;
        MoveX = 0f;
        JumpPressedThisFrame = false;
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    public void ResetState()
    {
        transform.position = startPosition;
        rb.velocity = Vector2.zero;
        replayMoveX = 0f;
        replayJumpPressed = false;
        MoveX = 0f;
        JumpPressedThisFrame = false;
    }

    private bool IsGrounded()
    {
        ContactPoint2D[] contacts = new ContactPoint2D[8];
        int contactCount = bodyCollider.GetContacts(contacts);

        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = contacts[i];
            if (contact.collider == null || contact.collider.isTrigger)
            {
                continue;
            }

            if (contact.normal.y >= groundedNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }
}
