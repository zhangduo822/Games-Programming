using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class SwitchablePlatform2D : MonoBehaviour, IResettable
{
    [SerializeField] private bool startsActive = true;
    [SerializeField] private float moveDistance = 0.5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector3 activePosition;
    [SerializeField] private Vector3 inactivePosition;
    [SerializeField] private PressurePlate2D linkedPlate;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D collider;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isActive;
    private bool isMoving;

    public bool IsActive => isActive;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();
        startPosition = transform.position;
        activePosition = startPosition + Vector3.up * moveDistance;
        inactivePosition = startPosition;
        isActive = startsActive;
        targetPosition = isActive ? activePosition : inactivePosition;

        if (collider != null)
        {
            collider.isTrigger = false;
        }

        UpdateVisuals();
    }

    private void Start()
    {
        if (linkedPlate != null)
        {
            isActive = linkedPlate.IsPressed;
            targetPosition = isActive ? activePosition : inactivePosition;
        }
    }

    [SerializeField] private PressurePlate2D linkedPlateRef;

    public PressurePlate2D LinkedPlate => linkedPlate ?? linkedPlateRef;

    public void LinkToPlate(PressurePlate2D plate)
    {
        linkedPlate = plate;
        linkedPlateRef = plate;
    }

    private void Update()
    {
        PressurePlate2D plate = linkedPlate ?? linkedPlateRef;
        if (plate != null && plate.gameObject != null)
        {
            bool shouldBeActive = plate.IsPressed;
            if (shouldBeActive != isActive)
            {
                isActive = shouldBeActive;
                targetPosition = isActive ? activePosition : inactivePosition;
                UpdateVisuals();
                if (collider != null) collider.enabled = isActive;
            }
        }

        if (isMoving || Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            isMoving = true;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        targetPosition = isActive ? activePosition : inactivePosition;
        UpdateVisuals();
        if (collider != null) collider.enabled = isActive;
    }

    public void Toggle()
    {
        SetActive(!isActive);
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isActive ? new Color(0.3f, 0.8f, 0.3f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    public void ResetState()
    {
        isActive = startsActive;
        targetPosition = isActive ? activePosition : inactivePosition;
        transform.position = startPosition;
        isMoving = false;
        UpdateVisuals();
    }
}
