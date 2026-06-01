using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class TemporalParadoxSceneBuilder
{
    private const string BuildFlagKey = "TemporalParadox2DSceneBuilt";
    private const string LevelFlagKey = "TemporalParadox2DLevelBuilt";
    private const string CloneTagName = "Clone";

    static TemporalParadoxSceneBuilder()
    {
        EditorApplication.delayCall += AutoBuildOnce;
    }

    [MenuItem("Tools/Temporal Paradox/Rebuild 2D Demo Scene")]
    public static void RebuildScene()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Cannot Rebuild During Play Mode",
                "Please stop the game first (click the Play button to exit), then try again.",
                "OK"
            );
            return;
        }
        EditorPrefs.DeleteKey(BuildFlagKey);
        EditorPrefs.DeleteKey(LevelFlagKey);
        BuildScene();
    }

    [MenuItem("Tools/Temporal Paradox/Build Level 2 (Advanced)")]
    public static void BuildLevel2()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Cannot Build During Play Mode",
                "Please stop the game first (click the Play button to exit), then try again.",
                "OK"
            );
            return;
        }
        EditorPrefs.DeleteKey(BuildFlagKey);
        EditorPrefs.SetInt(LevelFlagKey, 2);
        BuildScene();
    }

    [MenuItem("Tools/Temporal Paradox/Setup Clone Tag")]
    public static void SetupCloneTag()
    {
        EnsureCloneTagExists();
        Debug.Log("Clone tag is ready.");
    }

    [MenuItem("Tools/Temporal Paradox/Fix Missing Sprites")]
    public static void FixSprites()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Cannot Fix During Play", "Exit Play mode first.", "OK");
            return;
        }
        FixAllSpritesInScene();
    }

    private static void AutoBuildOnce()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (EditorPrefs.GetBool(BuildFlagKey, false))
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != "SampleScene")
        {
            return;
        }

        EditorPrefs.SetBool(BuildFlagKey, true);
        EditorPrefs.SetInt(LevelFlagKey, 1);
        BuildScene();
    }

    private static void BuildScene()
    {
        if (Application.isPlaying)
        {
            return;
        }

        EnsureCloneTagExists();

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            return;
        }

        ClearScene();

        int level = EditorPrefs.GetInt(LevelFlagKey, 1);

        if (level == 1)
        {
            BuildLevel1();
        }
        else
        {
            BuildLevel2Advanced();
        }

        Selection.activeGameObject = GameObject.Find("GameSystems");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        FixAllSpritesInScene();

        Debug.Log($"Temporal Paradox Level {level} built successfully.");
    }

    private static void EnsureCloneTagExists()
    {
        try
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
            );
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            bool tagExists = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == CloneTagName)
                {
                    tagExists = true;
                    break;
                }
            }
            if (!tagExists)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = CloneTagName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"Created Tag: '{CloneTagName}'");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not auto-create Clone tag: {e.Message}");
        }
    }

    private static void BuildLevel1()
    {
        CreateCamera();

        GameObject gameSystems = new GameObject("GameSystems");
        gameSystems.AddComponent<TemporalParadoxGame>();
        ReplayRecorder2D recorder = gameSystems.AddComponent<ReplayRecorder2D>();
        gameSystems.AddComponent<CloneManager>();
        gameSystems.AddComponent<TimelineVisualizer>();
        gameSystems.AddComponent<VisualFeedback>();

        CreateGround(new Vector3(0f, -2f, 0f), new Vector3(18f, 0.6f, 1f));

        PlayerController2D player = CreatePlayer(new Vector3(-7f, -1.05f, 0f));
        recorder.Configure(player);

        PressurePlate2D leftPlate = CreatePlate("PlateLeft", new Vector3(-3f, -1.62f, 0f));
        PressurePlate2D rightPlate = CreatePlate("PlateRight", new Vector3(3f, -1.62f, 0f));

        Door2D door = CreateDoor(new Vector3(5f, -0.85f, 0f));
        DualPlateDoor2D gate = gameSystems.AddComponent<DualPlateDoor2D>();
        gate.Configure(leftPlate, rightPlate, door);

        Goal2D goal = CreateGoal(new Vector3(7f, -0.95f, 0f));

        CreatePlatform(new Vector3(-1f, -0.3f, 0f), new Vector3(2.5f, 0.25f, 1f), new Color(0.7f, 0.7f, 0.7f, 1f));

        ReplayClone2D clonePrefab = CreateClonePrefab();
        var game = gameSystems.GetComponent<TemporalParadoxGame>();
        game.Configure(player, recorder, clonePrefab, door, leftPlate, rightPlate);
        goal.Configure(game, door);
    }

    private static void BuildLevel2Advanced()
    {
        CreateCamera();

        GameObject gameSystems = new GameObject("GameSystems");
        gameSystems.AddComponent<TemporalParadoxGame>();
        ReplayRecorder2D recorder = gameSystems.AddComponent<ReplayRecorder2D>();
        gameSystems.AddComponent<CloneManager>();
        gameSystems.AddComponent<TimelineVisualizer>();
        gameSystems.AddComponent<VisualFeedback>();

        CreateGround(new Vector3(0f, -2f, 0f), new Vector3(20f, 0.6f, 1f));

        PlayerController2D player = CreatePlayer(new Vector3(-8f, -1.05f, 0f));
        recorder.Configure(player);

        PressurePlate2D leftPlate = CreatePlate("PlateLeft", new Vector3(-4f, -1.62f, 0f));
        PressurePlate2D middlePlate = CreatePlate("PlateMiddle", new Vector3(0f, -1.62f, 0f));
        PressurePlate2D rightPlate = CreatePlate("PlateRight", new Vector3(4f, -1.62f, 0f));

        Door2D door1 = CreateDoor(new Vector3(2f, -0.85f, 0f));
        CreateDoor(new Vector3(6f, -0.85f, 0f));

        DualPlateDoor2D gate1 = gameSystems.AddComponent<DualPlateDoor2D>();
        gate1.Configure(leftPlate, middlePlate, door1);

        CreatePlatform(new Vector3(0f, 1.2f, 0f), new Vector3(3f, 0.25f, 1f), new Color(0.7f, 0.7f, 0.7f, 1f));

        CreateCrate(new Vector3(-2f, -1.45f, 0f));
        CreateCrate(new Vector3(2f, -1.45f, 0f));

        GameObject switchPlatformObj = CreatePlatform(new Vector3(6f, 0.5f, 0f), new Vector3(2f, 0.3f, 1f), new Color(0.3f, 0.8f, 0.3f, 1f));
        SwitchablePlatform2D switchPlatform = switchPlatformObj.AddComponent<SwitchablePlatform2D>();
        switchPlatform.LinkToPlate(middlePlate);

        Goal2D goal = CreateGoal(new Vector3(8.5f, -0.95f, 0f));

        ReplayClone2D clonePrefab = CreateClonePrefab();
        var game = gameSystems.GetComponent<TemporalParadoxGame>();
        game.Configure(player, recorder, clonePrefab, door1, leftPlate, rightPlate);
        goal.Configure(game, door1);
    }

    private static void ClearScene()
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = roots.Length - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(roots[i]);
        }
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = GameObject.Find("Main Camera");
        if (cameraObject != null)
        {
            Object.DestroyImmediate(cameraObject);
        }

        cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.42f, 0.58f, 0.78f, 1f);
    }

    private static void CreateGround(Vector3 position, Vector3 scale)
    {
        GameObject ground = CreateBox("Ground", position, scale, new Color(0.9f, 0.98f, 1f, 1f), 0, false);
        ground.layer = LayerMask.NameToLayer("Default");
    }

    private static PlayerController2D CreatePlayer(Vector3 position)
    {
        GameObject player = CreateBox("Player", position, new Vector3(0.65f, 1.05f, 1f), new Color(0.28f, 0.48f, 0.95f, 1f), 10, false);
        player.tag = "Player";
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        return player.AddComponent<PlayerController2D>();
    }

    private static PressurePlate2D CreatePlate(string name, Vector3 position)
    {
        GameObject plate = CreateBox(name, position, new Vector3(1.1f, 0.16f, 1f), new Color(1f, 0.72f, 0.05f, 1f), 4, true);
        return plate.AddComponent<PressurePlate2D>();
    }

    private static Door2D CreateDoor(Vector3 position)
    {
        GameObject door = CreateBox("Door", position, new Vector3(0.45f, 1.8f, 1f), new Color(0.95f, 0.25f, 0.25f, 1f), 6, false);
        return door.AddComponent<Door2D>();
    }

    private static Goal2D CreateGoal(Vector3 position)
    {
        GameObject goal = CreateBox("Goal", position, new Vector3(0.7f, 1.25f, 1f), new Color(0.2f, 0.9f, 0.35f, 1f), 3, true);
        return goal.AddComponent<Goal2D>();
    }

    private static GameObject CreatePlatform(Vector3 position, Vector3 scale, Color color)
    {
        return CreateBox("Platform", position, scale, color, 5, false);
    }

    private static GameObject CreateCrate(Vector3 position)
    {
        GameObject crate = CreateBox("Crate", position, new Vector3(0.8f, 0.8f, 1f), new Color(0.6f, 0.4f, 0.2f, 1f), 7, false);
        Rigidbody2D rb = crate.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.mass = 2f;
        crate.AddComponent<PushableCrate2D>();
        return crate;
    }

    private static ReplayClone2D CreateClonePrefab()
    {
        GameObject clone = CreateBox("TimeClonePrefab", new Vector3(-20f, -20f, 0f), new Vector3(0.65f, 1.05f, 1f), new Color(0.25f, 0.9f, 1f, 0.65f), 9, false);
        clone.tag = CloneTagName;
        Rigidbody2D rb = clone.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        PlayerController2D controller = clone.AddComponent<PlayerController2D>();
        controller.SetReplayMode(true);
        clone.AddComponent<ReplayClone2D>();

        string prefabPath = "Assets/Resources/TimeClonePrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(clone, prefabPath);

        if (prefab != null)
        {
            prefab.tag = CloneTagName;
            EditorUtility.SetDirty(prefab);
        }

        Object.DestroyImmediate(clone);
        return prefab != null ? prefab.GetComponent<ReplayClone2D>() : null;
    }

    private static GameObject CreateBox(string name, Vector3 position, Vector3 scale, Color color, int sortingOrder, bool trigger)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = scale;

        SpriteRenderer renderer = SpriteBoxFactory.SetupSpriteBox(obj, color, sortingOrder);

        if (renderer.sprite == null)
        {
            Debug.LogWarning($"CreateBox: Sprite was null for '{name}', forcing assignment.");
            Sprite white = SpriteBoxFactory.WhiteSprite;
            renderer.sprite = white;
        }

        BoxCollider2D collider = SpriteBoxFactory.SetupBoxCollider2D(obj, trigger, Vector2.one);
        return obj;
    }

    private static void FixAllSpritesInScene()
    {
        Sprite whiteSprite = SpriteBoxFactory.WhiteSprite;
        if (whiteSprite == null)
        {
            Debug.LogError("FixAllSpritesInScene: WhiteSprite is null, cannot fix.");
            return;
        }

        SpriteRenderer[] allRenderers = Object.FindObjectsOfType<SpriteRenderer>();
        int fixedCount = 0;
        foreach (var r in allRenderers)
        {
            if (r.sprite == null)
            {
                r.sprite = whiteSprite;
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"Fixed {fixedCount} SpriteRenderer(s) with null sprites.");
        }
    }
}
