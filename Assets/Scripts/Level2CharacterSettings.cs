using UnityEngine;

public class Level2CharacterSettings : MonoBehaviour
{
    [Header("Player Transform")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 playerLocalPosition = new Vector3(-9f, -1.05f, 0f);
    [SerializeField] private Vector3 playerLocalScale = new Vector3(0.65f, 1.05f, 1f);
    [SerializeField] private bool lockTransformToSettings;

    public Transform PlayerTransform => playerTransform;
    public Vector3 PlayerLocalPosition => playerLocalPosition;
    public Vector3 PlayerLocalScale => playerLocalScale;

    // Initializes settings from the current player when the component is reset.
    private void Reset()
    {
        TryAutoAssignPlayer();
        SyncFromPlayerIfUnlocked();
        Apply();
    }

    // Keeps editor-time settings synchronized with the assigned player.
    private void OnValidate()
    {
        TryAutoAssignPlayer();
        SyncFromPlayerIfUnlocked();
        Apply();
    }

    // Assigns the player transform and chooses default placement settings.
    public void Configure(Transform targetPlayerTransform, Vector3 defaultPosition, Vector3 defaultScale)
    {
        playerTransform = targetPlayerTransform;

        if (playerTransform != null && !lockTransformToSettings)
        {
            playerLocalPosition = playerTransform.localPosition;
            playerLocalScale = playerTransform.localScale;
        }
        else
        {
            if (playerLocalPosition == Vector3.zero)
            {
                playerLocalPosition = defaultPosition;
            }

            if (playerLocalScale == Vector3.zero)
            {
                playerLocalScale = defaultScale;
            }
        }

        Apply();
    }

    // Applies the stored transform values when locking is enabled.
    public void Apply()
    {
        if (playerTransform == null || !lockTransformToSettings)
        {
            return;
        }

        playerTransform.localPosition = playerLocalPosition;
        playerTransform.localScale = playerLocalScale;
    }

    // Finds the scene player automatically when no transform is assigned.
    private void TryAutoAssignPlayer()
    {
        if (playerTransform != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // Copies the current player transform into settings while unlocked.
    private void SyncFromPlayerIfUnlocked()
    {
        if (playerTransform == null || lockTransformToSettings)
        {
            return;
        }

        Vector3 currentScale = playerTransform.localScale;
        if (currentScale.x < 0.1f || currentScale.y < 0.1f)
        {
            return;
        }

        playerLocalPosition = playerTransform.localPosition;
        playerLocalScale = currentScale;
    }
}
