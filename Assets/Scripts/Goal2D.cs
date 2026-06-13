using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Goal2D : MonoBehaviour
{
    private const string GoalSpriteResourcePath = "Art/MagicDoor";

    [SerializeField] private Door2D requiredDoor;
    [SerializeField] private TemporalParadoxGame game;

    private static Sprite cachedGoalSprite;
    private BoxCollider2D goalCollider;
    private SpriteRenderer spriteRenderer;
    private bool hasTriggered;

    // Stores the game controller and door associated with this goal.
    public void Configure(TemporalParadoxGame gameController, Door2D door)
    {
        game = gameController;
        requiredDoor = door;
    }

    // Prepares the goal trigger and portal visual.
    private void Awake()
    {
        goalCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        goalCollider.isTrigger = true;
        EnsureVisual();
    }

    // Checks for the player when they first enter the goal trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTriggerGoal(other);
    }

    // Checks for the player while they remain inside the goal trigger.
    private void OnTriggerStay2D(Collider2D other)
    {
        TryTriggerGoal(other);
    }

    // Completes the level or loads the next scene when the player reaches the portal.
    private void TryTriggerGoal(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;

        if (game != null)
        {
            game.MarkCleared();
            return;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;
        string fallbackScene = activeSceneName == "SampleScene" ? "Level2" : activeSceneName == "Level2" ? "Level3" : activeSceneName == "Level3" ? "EndingStory" : string.Empty;
        if (!string.IsNullOrWhiteSpace(fallbackScene))
        {
            SceneManager.LoadScene(fallbackScene);
        }
    }

    // Applies the magic door sprite and renderer settings.
    private void EnsureVisual()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Sprite goalSprite = GetGoalSprite();
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = goalSprite != null ? goalSprite : SpriteBoxFactory.WhiteSprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        if (goalCollider != null)
        {
            spriteRenderer.size = goalCollider.size;
        }
        spriteRenderer.sortingOrder = 7;
        spriteRenderer.material = SpriteBoxFactory.DefaultMaterial;
    }

    // Loads and caches the goal portal sprite from Resources.
    private static Sprite GetGoalSprite()
    {
        if (cachedGoalSprite != null)
        {
            return cachedGoalSprite;
        }

        cachedGoalSprite = Resources.Load<Sprite>(GoalSpriteResourcePath);
        return cachedGoalSprite;
    }
}
