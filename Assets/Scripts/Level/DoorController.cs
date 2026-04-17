using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorController : MonoBehaviour, IResettable
{
    [SerializeField] private GameObject doorVisual;
    [SerializeField] private bool startsOpen;

    private Collider2D doorCollider;
    private bool isOpen;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        SetOpen(startsOpen);
    }

    public void OpenDoor()
    {
        SetOpen(true);
    }

    public void CloseDoor()
    {
        SetOpen(false);
    }

    private void SetOpen(bool open)
    {
        isOpen = open;
        if (doorCollider != null)
        {
            doorCollider.enabled = !open;
        }

        if (doorVisual != null)
        {
            doorVisual.SetActive(!open);
        }
    }

    public void ResetState()
    {
        SetOpen(startsOpen);
    }
}
