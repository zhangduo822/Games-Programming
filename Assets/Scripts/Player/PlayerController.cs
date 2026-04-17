using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IResettable
{
    public enum ControlMode
    {
        Live,
        Replay
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundMask;

    [Header("Control")]
    [SerializeField] private ControlMode controlMode = ControlMode.Live;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private Rigidbody2D rb;
    private Vector2 replayMove;
    private bool replayJumpRequested;
    private bool replayInteractRequested;
    private Vector3 initialPosition;

    public bool InteractPressedThisFrame { get; private set; }
    public Vector2 CurrentMoveInput { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool IsGrounded { get; private set; }
    public ControlMode Mode => controlMode;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
    }

    private void Update()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        if (controlMode == ControlMode.Live)
        {
            CurrentMoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
            JumpPressedThisFrame = Input.GetKeyDown(jumpKey);
            InteractPressedThisFrame = Input.GetKeyDown(interactKey);
        }
        else
        {
            CurrentMoveInput = replayMove;
            JumpPressedThisFrame = replayJumpRequested;
            InteractPressedThisFrame = replayInteractRequested;
            replayJumpRequested = false;
            replayInteractRequested = false;
        }

        if (JumpPressedThisFrame && IsGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(CurrentMoveInput.x * moveSpeed, rb.velocity.y);
    }

    public void ApplyReplayFrame(FrameInput frame)
    {
        replayMove = frame.Move;
        if (frame.JumpPressed)
        {
            replayJumpRequested = true;
        }

        if (frame.InteractPressed)
        {
            replayInteractRequested = true;
        }
    }

    public void SetControlMode(ControlMode newMode)
    {
        controlMode = newMode;
    }

    public void ResetState()
    {
        transform.position = initialPosition;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        replayMove = Vector2.zero;
        replayJumpRequested = false;
        replayInteractRequested = false;
        CurrentMoveInput = Vector2.zero;
        JumpPressedThisFrame = false;
        InteractPressedThisFrame = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
