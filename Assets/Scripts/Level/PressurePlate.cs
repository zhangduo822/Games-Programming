using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PressurePlate : MonoBehaviour, IResettable
{
    [SerializeField] private UnityEvent onPressed;
    [SerializeField] private UnityEvent onReleased;
    [SerializeField] private bool startsPressed;

    private int overlapCount;
    private bool isPressed;

    public bool IsPressed => isPressed;

    private void Start()
    {
        isPressed = startsPressed;
        if (isPressed)
        {
            onPressed?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Clone"))
        {
            return;
        }

        overlapCount++;
        if (!isPressed)
        {
            isPressed = true;
            onPressed?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Clone"))
        {
            return;
        }

        overlapCount = Mathf.Max(0, overlapCount - 1);
        if (overlapCount == 0 && isPressed)
        {
            isPressed = false;
            onReleased?.Invoke();
        }
    }

    public void ResetState()
    {
        overlapCount = 0;
        bool wasPressed = isPressed;
        isPressed = startsPressed;

        if (isPressed && !wasPressed)
        {
            onPressed?.Invoke();
        }
        else if (!isPressed && wasPressed)
        {
            onReleased?.Invoke();
        }
    }
}
