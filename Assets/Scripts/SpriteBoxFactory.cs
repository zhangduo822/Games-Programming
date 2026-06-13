using UnityEngine;

public static class SpriteBoxFactory
{
    private const string WhiteSpriteResourcePath = "Sprites/WhiteSquare";

    private static Sprite whiteSprite;
    private static Texture2D whiteTexture;
    private static Material defaultMaterial;

    // Creates a one-pixel white texture used as a fallback sprite source.
    private static Texture2D CreateWhiteTexture()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.hideFlags = HideFlags.None;
        return tex;
    }

    // Ensures a usable white sprite exists for generated boxes.
    private static void EnsureWhiteSpriteExists()
    {
        if (whiteSprite != null && whiteSprite.texture != null) return;

        whiteSprite = Resources.Load<Sprite>(WhiteSpriteResourcePath);
        if (whiteSprite != null && whiteSprite.texture != null)
        {
            return;
        }

        if (whiteTexture == null || whiteTexture.width <= 0)
        {
            whiteTexture = CreateWhiteTexture();
        }

        if (whiteSprite == null || whiteSprite.texture == null)
        {
            whiteSprite = Sprite.Create(whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }

    public static Sprite WhiteSprite
    {
        get
        {
            EnsureWhiteSpriteExists();
            return whiteSprite;
        }
    }

    public static Material DefaultMaterial
    {
        get
        {
            if (defaultMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    Debug.LogWarning("SpriteBoxFactory: Sprites/Default shader not found, using fallback.");
                    shader = Shader.Find("Unlit/Color");
                }
                defaultMaterial = new Material(shader);
                defaultMaterial.hideFlags = HideFlags.None;
            }
            return defaultMaterial;
        }
    }

    // Adds or configures a SpriteRenderer to display a simple colored box.
    public static SpriteRenderer SetupSpriteBox(GameObject obj, Color color, int sortingOrder)
    {
        Remove3DComponents(obj);

        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = obj.AddComponent<SpriteRenderer>();
        }

        EnsureWhiteSpriteExists();
        renderer.sprite = whiteSprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        renderer.material = DefaultMaterial;

        return renderer;
    }

    // Adds or configures a 2D box collider with the requested trigger state.
    public static BoxCollider2D SetupBoxCollider2D(GameObject obj, bool isTrigger, Vector2 size)
    {
        BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = obj.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = isTrigger;
        collider.size = size;
        collider.offset = Vector2.zero;
        return collider;
    }

    // Removes incompatible 3D components from generated 2D objects.
    private static void Remove3DComponents(GameObject obj)
    {
        Remove(obj.GetComponent<Collider>());
        Remove(obj.GetComponent<MeshRenderer>());
        Remove(obj.GetComponent<MeshFilter>());
    }

    // Destroys a component safely in play mode or edit mode.
    private static void Remove(Component component)
    {
        if (component == null) return;

        if (Application.isPlaying)
            Object.Destroy(component);
        else
            Object.DestroyImmediate(component);
    }
}
