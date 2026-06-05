using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class SimpleHumanoidVisual : MonoBehaviour
{
    [SerializeField] private int pixelsPerUnit = 16;
    [SerializeField] private Color tunicColor = new(0.25f, 0.47f, 0.93f, 1f);
    [SerializeField] private Color pantsColor = new(0.16f, 0.22f, 0.45f, 1f);
    [SerializeField] private Color skinColor = new(0.97f, 0.84f, 0.72f, 1f);
    [SerializeField] private Color hairColor = new(0.14f, 0.1f, 0.08f, 1f);
    [SerializeField] private Color outlineColor = new(0.07f, 0.09f, 0.14f, 1f);
    [SerializeField] private bool ghostTint;

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

        if (ghostTint)
        {
            finalTunic = new Color(0.45f, 0.95f, 1f, 0.72f);
            finalPants = new Color(0.22f, 0.62f, 0.8f, 0.72f);
            finalSkin = new Color(0.86f, 0.98f, 1f, 0.62f);
            finalHair = new Color(0.9f, 1f, 1f, 0.52f);
            finalOutline = new Color(0.05f, 0.25f, 0.35f, 0.45f);
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
