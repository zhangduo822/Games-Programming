using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class SimpleHumanoidVisual : MonoBehaviour
{
    public enum VisualRole
    {
        Player,
        Clone
    }

    [SerializeField] private int pixelsPerUnit = 16;
    [SerializeField] private Color tunicColor = new(0.25f, 0.47f, 0.93f, 1f);
    [SerializeField] private Color pantsColor = new(0.16f, 0.22f, 0.45f, 1f);
    [SerializeField] private Color skinColor = new(0.97f, 0.84f, 0.72f, 1f);
    [SerializeField] private Color hairColor = new(0.14f, 0.1f, 0.08f, 1f);
    [SerializeField] private Color outlineColor = new(0.07f, 0.09f, 0.14f, 1f);
    [SerializeField] private VisualRole visualRole;

    private static Sprite whiteSprite;

    private void Awake()
    {
        EnsureVisual();
    }

    private void Reset()
    {
        EnsureVisual();
    }

    private void OnValidate()
    {
        EnsureVisual();
    }

    public void SetVisualRole(VisualRole role)
    {
        visualRole = role;
        EnsureVisual();
    }

    private void EnsureVisual()
    {
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
        }

        Color finalTunic = tunicColor;
        Color finalPants = pantsColor;
        Color finalSkin = skinColor;
        Color finalHair = hairColor;
        Color finalOutline = outlineColor;
        bool isClone = visualRole == VisualRole.Clone;

        if (isClone)
        {
            finalTunic = new Color(0.45f, 0.95f, 1f, 0.78f);
            finalPants = new Color(0.22f, 0.62f, 0.8f, 0.78f);
            finalSkin = new Color(0.86f, 0.98f, 1f, 0.7f);
            finalHair = new Color(0.9f, 1f, 1f, 0.6f);
            finalOutline = new Color(0.05f, 0.25f, 0.35f, 0.55f);
        }

        HideLegacyChildren();

        EnsurePart("Body", 0, 1, 8, 6, finalTunic, 22);
        EnsurePart("Belt", 0, -1, 8, 1, finalOutline, 23);
        EnsurePart("Head", 0, 8, 7, 6, finalSkin, 24);
        EnsurePart("Hair", 0, 10, 8, 2, finalHair, 25);
        EnsurePart("EyeL", -2, 8, 1, 1, finalOutline, 26);
        EnsurePart("EyeR", 2, 8, 1, 1, finalOutline, 26);
        EnsurePart("ArmL", -5, 1, 2, 6, finalSkin, 21);
        EnsurePart("ArmR", 5, 1, 2, 6, finalSkin, 21);
        EnsurePart("HandL", -5, -2, 2, 2, finalSkin, 22);
        EnsurePart("HandR", 5, -2, 2, 2, finalSkin, 22);
        EnsurePart("LegL", -2, -7, 3, 7, finalPants, 20);
        EnsurePart("LegR", 2, -7, 3, 7, finalPants, 20);
        EnsurePart("FootL", -2, -11, 3, 2, finalOutline, 19);
        EnsurePart("FootR", 2, -11, 3, 2, finalOutline, 19);

        EnsureAccessory(isClone);
    }

    private void HideLegacyChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
            if (childRenderer != null)
            {
                childRenderer.enabled = true;
            }
        }
    }

    private void EnsureAccessory(bool isClone)
    {
        Transform playerBadge = transform.Find("PlayerBadge");
        if (playerBadge != null)
        {
            playerBadge.gameObject.SetActive(!isClone);
        }

        Transform cloneAura = transform.Find("CloneAura");
        if (cloneAura != null)
        {
            cloneAura.gameObject.SetActive(isClone);
        }

        Transform cloneBadge = transform.Find("CloneBadge");
        if (cloneBadge != null)
        {
            cloneBadge.gameObject.SetActive(isClone);
        }

        if (isClone)
        {
            EnsurePart("CloneAura", 0, 1, 10, 15, new Color(0.3f, 0.92f, 1f, 0.18f), 18);
            EnsurePart("CloneBadge", 0, 13, 5, 2, new Color(0.85f, 1f, 1f, 0.95f), 27);
        }
        else
        {
            EnsurePart("PlayerBadge", 0, 13, 4, 2, new Color(1f, 0.84f, 0.2f, 0.95f), 27);
        }
    }

    private void EnsurePart(string partName, int pixelX, int pixelY, int pixelWidth, int pixelHeight, Color color, int sortingOrder)
    {
        Transform part = transform.Find(partName);
        if (part == null)
        {
            GameObject child = new GameObject(partName);
            child.transform.SetParent(transform, false);
            part = child.transform;
        }

        float unit = 1f / Mathf.Max(1, pixelsPerUnit);
        part.localPosition = new Vector3(pixelX * unit, pixelY * unit, 0f);
        part.localRotation = Quaternion.identity;
        part.localScale = new Vector3(pixelWidth * unit, pixelHeight * unit, 1f);

        SpriteRenderer renderer = part.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = part.gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.enabled = true;
        renderer.sprite = GetWhiteSprite();
        renderer.color = color;
        renderer.sortingLayerID = 0;
        renderer.sortingOrder = sortingOrder;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.maskInteraction = SpriteMaskInteraction.None;
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite == null)
        {
            whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            whiteSprite.name = "GeneratedWhiteSprite";
        }

        return whiteSprite;
    }
}
