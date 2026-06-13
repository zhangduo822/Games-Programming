using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class LayeredCloudBackground2D : MonoBehaviour
{
    private readonly struct CloudLayer
    {
        // Stores the resource path and layout settings for one cloud layer.
        public CloudLayer(string spritePath, bool coverCamera, float scale, Vector2 viewportOffset, Color tint)
        {
            SpritePath = spritePath;
            CoverCamera = coverCamera;
            Scale = scale;
            ViewportOffset = viewportOffset;
            Tint = tint;
        }

        public string SpritePath { get; }
        public bool CoverCamera { get; }
        public float Scale { get; }
        public Vector2 ViewportOffset { get; }
        public Color Tint { get; }
    }

    private static readonly CloudLayer[] CloudLayers =
    {
        new("Art/background1", true, 1.18f, new Vector2(0f, 0.16f), new Color(0.86f, 0.92f, 1f, 0.58f)),
        new("Art/background2", true, 1.1f, new Vector2(0f, 0.04f), new Color(1f, 1f, 1f, 0.72f)),
        new("Art/background3", true, 1.04f, new Vector2(0f, -0.06f), new Color(1f, 1f, 1f, 0.88f)),
        new("Art/cloud1", false, 0.42f, new Vector2(-0.38f, 0.36f), new Color(1f, 1f, 1f, 0.62f)),
        new("Art/cloud2", false, 0.5f, new Vector2(0.32f, 0.29f), new Color(1f, 1f, 1f, 0.58f)),
        new("Art/cloud3", false, 0.35f, new Vector2(-0.08f, 0.18f), new Color(1f, 1f, 1f, 0.66f)),
        new("Art/cloud4", false, 0.52f, new Vector2(-0.48f, 0.05f), new Color(1f, 1f, 1f, 0.72f)),
        new("Art/cloud5", false, 0.43f, new Vector2(0.48f, 0.02f), new Color(1f, 1f, 1f, 0.76f)),
        new("Art/cloud6", false, 0.36f, new Vector2(0.05f, -0.14f), new Color(1f, 1f, 1f, 0.7f)),
        new("Art/cloud7", false, 0.47f, new Vector2(-0.28f, -0.28f), new Color(1f, 1f, 1f, 0.64f)),
        new("Art/cloud8", false, 0.4f, new Vector2(0.37f, -0.34f), new Color(1f, 1f, 1f, 0.68f))
    };

    private Camera targetCamera;

    // Rebuilds the cloud background when the component becomes active.
    private void OnEnable()
    {
        EnsureBackground();
    }

    // Refreshes the cloud background when inspector values change.
    private void OnValidate()
    {
        EnsureBackground();
    }

    // Keeps the cloud layers sized to the active camera view.
    private void LateUpdate()
    {
        EnsureBackground();
    }

    // Creates and positions all cloud sprite layers for the camera.
    private void EnsureBackground()
    {
        targetCamera = targetCamera != null ? targetCamera : GetComponent<Camera>();
        if (targetCamera == null || !targetCamera.orthographic)
        {
            return;
        }

        float visibleHeight = targetCamera.orthographicSize * 2f;
        float visibleWidth = visibleHeight * Mathf.Max(0.01f, targetCamera.aspect);

        for (int i = 0; i < CloudLayers.Length; i++)
        {
            CloudLayer cloudLayer = CloudLayers[i];
            Sprite sprite = Resources.Load<Sprite>(cloudLayer.SpritePath);
            if (sprite == null)
            {
                continue;
            }

            Transform layer = EnsureLayer(i);
            SpriteRenderer renderer = layer.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = cloudLayer.Tint;
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.sortingLayerID = 0;
            renderer.sortingOrder = -30 + i;
            renderer.maskInteraction = SpriteMaskInteraction.None;

            Vector2 spriteSize = sprite.bounds.size;
            float finalScale;
            if (cloudLayer.CoverCamera)
            {
                float fitScale = Mathf.Max(visibleWidth / spriteSize.x, visibleHeight / spriteSize.y);
                finalScale = fitScale * cloudLayer.Scale;
            }
            else
            {
                float targetWidth = visibleWidth * cloudLayer.Scale;
                finalScale = targetWidth / spriteSize.x;
            }

            layer.localPosition = new Vector3(
                visibleWidth * cloudLayer.ViewportOffset.x,
                visibleHeight * cloudLayer.ViewportOffset.y,
                10f + i * 0.01f
            );
            layer.localRotation = Quaternion.identity;
            layer.localScale = new Vector3(finalScale, finalScale, 1f);
        }
    }

    // Finds or creates the child transform for a background layer.
    private Transform EnsureLayer(int index)
    {
        string layerName = $"CloudBackgroundLayer{index + 1}";
        Transform layer = transform.Find(layerName);
        if (layer != null)
        {
            return layer;
        }

        GameObject layerObject = new GameObject(layerName);
        layerObject.transform.SetParent(transform, false);
        SpriteRenderer renderer = layerObject.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = -30 + index;
        return layerObject.transform;
    }
}
