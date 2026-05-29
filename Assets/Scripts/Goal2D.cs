using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Goal2D : MonoBehaviour
{
    [SerializeField] private Door2D requiredDoor;
    [SerializeField] private TemporalParadoxGame game;

    public void Configure(TemporalParadoxGame gameController, Door2D door)
    {
        game = gameController;
        requiredDoor = door;
    }

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (requiredDoor != null && !requiredDoor.IsOpen)
        {
            return;
        }

        if (game != null)
        {
            game.MarkCleared();
        }
    }
}
