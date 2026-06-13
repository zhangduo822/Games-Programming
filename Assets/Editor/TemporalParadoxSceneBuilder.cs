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
    private const string GroundSpriteResourcePath = "Art/GrassGround";
    private const string PlatformSpriteResourcePath = "Art/StoneRoad";

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

    [MenuItem("Tools/Temporal Paradox/Build Level 3")]
    public static void BuildLevel3()
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

        const string level3Path = "Assets/Scenes/Level3.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, level3Path);
        EditorPrefs.DeleteKey(BuildFlagKey);
        EditorPrefs.SetInt(LevelFlagKey, 3);
        BuildScene();
        EnsureSceneInBuildSettings("Assets/Scenes/SampleScene.unity");
        EnsureSceneInBuildSettings("Assets/Scenes/Level2.unity");
        EnsureSceneInBuildSettings(level3Path);
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
        else if (level == 2)
        {
            BuildLevel2Advanced();
        }
        else
        {
            BuildLevel3Advanced();
        }

        Selection.activeGameObject = GameObject.Find("GameSystems");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        FixAllSpritesInScene();

        Debug.Log($"Temporal Paradox Level {level} built successfully.");
    }

    private static void EnsureSceneInBuildSettings(string scenePath)
    {
        EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
        for (int i = 0; i < currentScenes.Length; i++)
        {
            if (currentScenes[i].path == scenePath)
            {
                currentScenes[i].enabled = true;
                EditorBuildSettings.scenes = currentScenes;
                return;
            }
        }

        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(currentScenes)
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();
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
        EnsureLevel2CharacterSettings(gameSystems, player.transform, new Vector3(-7f, -1.05f, 0f), new Vector3(0.65f, 1.05f, 1f));
        recorder.Configure(player);

        PressurePlate2D leftPlate = CreatePlate("PlateLeft", new Vector3(-3f, -1.62f, 0f));
        PressurePlate2D rightPlate = CreatePlate("PlateRight", new Vector3(3f, -1.62f, 0f));

        Door2D door = CreateDoor(new Vector3(3.82f, -0.15f, 0f));
        DualPlateDoor2D gate = gameSystems.AddComponent<DualPlateDoor2D>();
        gate.Configure(leftPlate, rightPlate, door);

        Goal2D goal = CreateGoal(new Vector3(7f, -0.95f, 0f));

        CreatePlatform(new Vector3(-1f, -0.3f, 0f), new Vector3(2.5f, 0.25f, 1f), new Color(0.7f, 0.7f, 0.7f, 1f));

        ReplayClone2D clonePrefab = CreateClonePrefab();
        var game = gameSystems.GetComponent<TemporalParadoxGame>();
        game.Configure(player, recorder, clonePrefab, door, leftPlate, rightPlate);
        game.SetNextSceneName("Level2");
        goal.Configure(game, door);
    }

    private static void BuildLevel2Advanced()
    {
        CreateCamera();

        GameObject gameSystems = new GameObject("GameSystems");
        gameSystems.AddComponent<TemporalParadoxGame>();
        ReplayRecorder2D recorder = gameSystems.AddComponent<ReplayRecorder2D>();
        gameSystems.AddComponent<CloneManager>();

        CreateGround(new Vector3(0f, -2f, 0f), new Vector3(24f, 0.6f, 1f));

        PlayerController2D player = CreatePlayer(new Vector3(-10.3f, -1.165f, 0f));
        EnsureLevel2CharacterSettings(gameSystems, player.transform, new Vector3(-10.3f, -1.165f, 0f), Vector3.one);
        recorder.Configure(player);

        PressurePlate2D leftPlate = CreatePlate("PlateLeft", new Vector3(-7.1f, -1.62f, 0f));
        PressurePlate2D middlePlate = CreatePlate("PlateMiddle", new Vector3(-1.4f, 0.955f, 0f));
        PressurePlate2D rightPlate = CreatePlate("PlateRight", new Vector3(1.7f, -0.245f, 0f));

        Door2D door1 = CreateDoor(new Vector3(8.45f, 4.925f, 0f));
        DualPlateDoor2D gate1 = gameSystems.AddComponent<DualPlateDoor2D>();
        gate1.Configure(leftPlate, middlePlate, door1);

        CreatePlatform(new Vector3(-4.4f, -0.45f, 0f), new Vector3(2.6f, 0.25f, 1f), new Color(0.68f, 0.68f, 0.72f, 1f));
        CreatePlatform(new Vector3(-1.4f, 0.75f, 0f), new Vector3(2.8f, 0.25f, 1f), new Color(0.68f, 0.68f, 0.72f, 1f));
        CreatePlatform(new Vector3(2.2f, -0.45f, 0f), new Vector3(2.2f, 0.25f, 1f), new Color(0.68f, 0.68f, 0.72f, 1f));
        CreatePlatform(new Vector3(9.8f, 3.2f, 0f), new Vector3(4.6f, 0.25f, 1f), new Color(0.68f, 0.68f, 0.72f, 1f));

        CreateCrate(new Vector3(-8.6f, -1.25f, 0f));

        GameObject bridgeC = CreatePlatform(new Vector3(5.4f, 0.6f, 0f), new Vector3(2f, 0.24f, 1f), new Color(0.25f, 0.78f, 0.34f, 1f));
        SwitchablePlatform2D bridgeCSwitch = bridgeC.AddComponent<SwitchablePlatform2D>();
        bridgeCSwitch.LinkToPlate(rightPlate);

        HazardZone2D lavaPitA = CreateHazardZone("LavaPitA", new Vector3(-0.4f, -1.63f, 0f), new Vector3(2.2f, 0.32f, 1f));
        ConfigureFireTrapVisual(lavaPitA.gameObject, new Vector3(0f, 0.42f, 0f), new Vector3(1.55f, 1.75f, 1f), 6);
        HazardZone2D lavaPitB = CreateHazardZone("LavaPitB", new Vector3(5.4f, -1.63f, 0f), new Vector3(3.8f, 0.32f, 1f));
        ConfigureFireTrapVisual(lavaPitB.gameObject, new Vector3(0f, 0.42f, 0f), new Vector3(2.05f, 1.75f, 1f), 6);
        HazardZone2D lavaPitC = CreateHazardZone("LavaPitC", new Vector3(9.4f, -1.63f, 0f), new Vector3(1.6f, 0.32f, 1f));
        ConfigureFireTrapVisual(lavaPitC.gameObject, new Vector3(0f, 0.42f, 0f), new Vector3(1.25f, 1.75f, 1f), 6);
        HazardZone2D shelfFire = CreateHazardZone("SpikeShelf", new Vector3(2.9f, -0.18f, 0f), new Vector3(1.25f, 0.32f, 1f));
        ConfigureFireTrapVisual(shelfFire.gameObject, new Vector3(0f, 0.42f, 0f), new Vector3(1.25f, 1.75f, 1f), 6);

        Goal2D goal = CreateGoal(new Vector3(11.2f, 3.95f, 0f));

        ReplayClone2D clonePrefab = CreateClonePrefab();
        var game = gameSystems.GetComponent<TemporalParadoxGame>();
        game.Configure(player, recorder, clonePrefab, door1, leftPlate, rightPlate);
        game.SetNextSceneName("Level3");
        goal.Configure(game, door1);
    }

    private static void BuildLevel3Advanced()
    {
        CreateCamera();

        GameObject gameSystems = new GameObject("GameSystems");
        gameSystems.AddComponent<TemporalParadoxGame>();
        ReplayRecorder2D recorder = gameSystems.AddComponent<ReplayRecorder2D>();
        gameSystems.AddComponent<CloneManager>();

        CreateGround(new Vector3(0f, -2f, 0f), new Vector3(30f, 0.6f, 1f));

        PlayerController2D player = CreatePlayer(new Vector3(-13.1f, -1.165f, 0f));
        EnsureLevel2CharacterSettings(gameSystems, player.transform, new Vector3(-13.1f, -1.165f, 0f), Vector3.one);
        recorder.Configure(player);

        PressurePlate2D cratePlate = CreatePlate("PlateCrate", new Vector3(-9.6f, -1.62f, 0f));
        PressurePlate2D lowerClonePlate = CreatePlate("PlateLowerClone", new Vector3(-2.8f, -1.62f, 0f));
        PressurePlate2D upperClonePlate = CreatePlate("PlateUpperClone", new Vector3(3.65f, 1.305f, 0f));

        Door2D door = CreateDoor(new Vector3(10.7f, 4.55f, 0f));
        DualPlateDoor2D gate = gameSystems.AddComponent<DualPlateDoor2D>();
        gate.Configure(cratePlate, upperClonePlate, door);

        CreateCrate(new Vector3(-11.4f, -1.25f, 0f));

        CreatePlatform(new Vector3(-6.25f, -0.62f, 0f), new Vector3(2.3f, 0.24f, 1f), new Color(0.68f, 0.68f, 0.72f, 1f));
        CreatePlatform(new Vector3(-2.8f, -0.05f, 0f), new Vector3(2.35f, 0.24f, 1f), new Color(0.68f, 0.68f, 0.72f, 1f));
        CreatePlatform(new Vector3(0.4f, 0.78f, 0f), new Vector3(1.8f, 0.24f, 1f), new Color(0.68f, 0.68f, 0.72f, 1f));
        CreatePlatform(new Vector3(3.65f, 1.1f, 0f), new Vector3(2.4f, 0.24f, 1f), new Color(0.68f, 0.68f, 0.72f, 1f));
        CreatePlatform(new Vector3(12f, 2.75f, 0f), new Vector3(4.8f, 0.25f, 1f), new Color(0.68f, 0.68f, 0.72f, 1f));

        GameObject firstBridge = CreatePlatform(new Vector3(6.1f, 1.05f, 0f), new Vector3(1.65f, 0.24f, 1f), new Color(0.25f, 0.78f, 0.34f, 1f));
        SwitchablePlatform2D firstBridgeSwitch = firstBridge.AddComponent<SwitchablePlatform2D>();
        firstBridgeSwitch.LinkToPlate(lowerClonePlate);

        GameObject finalBridge = CreatePlatform(new Vector3(8.25f, 2.02f, 0f), new Vector3(1.75f, 0.24f, 1f), new Color(0.25f, 0.78f, 0.34f, 1f));
        SwitchablePlatform2D finalBridgeSwitch = finalBridge.AddComponent<SwitchablePlatform2D>();
        finalBridgeSwitch.LinkToPlate(upperClonePlate);

        HazardZone2D lavaPitA = CreateHazardZone("LavaPitA", new Vector3(-5.15f, -1.63f, 0f), new Vector3(1.9f, 0.32f, 1f));
        ConfigureFireTrapVisual(lavaPitA.gameObject, new Vector3(0f, 0.42f, 0f), new Vector3(1.35f, 1.75f, 1f), 6);
        HazardZone2D lavaPitB = CreateHazardZone("LavaPitB", new Vector3(0.25f, -1.63f, 0f), new Vector3(3.2f, 0.32f, 1f));
        ConfigureFireTrapVisual(lavaPitB.gameObject, new Vector3(0f, 0.42f, 0f), new Vector3(1.85f, 1.75f, 1f), 6);
        HazardZone2D lavaPitC = CreateHazardZone("LavaPitC", new Vector3(5.25f, -1.63f, 0f), new Vector3(3.4f, 0.32f, 1f));
        ConfigureFireTrapVisual(lavaPitC.gameObject, new Vector3(0f, 0.42f, 0f), new Vector3(1.95f, 1.75f, 1f), 6);
        HazardZone2D upperFire = CreateHazardZone("UpperFire", new Vector3(1.35f, 1.05f, 0f), new Vector3(1.05f, 0.32f, 1f));
        ConfigureFireTrapVisual(upperFire.gameObject, new Vector3(0f, 0.42f, 0f), new Vector3(1.05f, 1.75f, 1f), 6);
        HazardZone2D finalFire = CreateHazardZone("FinalFire", new Vector3(9.55f, 2.98f, 0f), new Vector3(1.15f, 0.32f, 1f));
        ConfigureFireTrapVisual(finalFire.gameObject, new Vector3(0f, 0.42f, 0f), new Vector3(1.1f, 1.75f, 1f), 6);

        Goal2D goal = CreateGoal(new Vector3(13.4f, 3.5f, 0f));

        ReplayClone2D clonePrefab = CreateClonePrefab();
        var game = gameSystems.GetComponent<TemporalParadoxGame>();
        game.Configure(player, recorder, clonePrefab, door, cratePlate, upperClonePlate);
        goal.Configure(game, door);
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
        System.Type backgroundType = System.Type.GetType("LayeredCloudBackground2D, Assembly-CSharp");
        if (backgroundType != null)
        {
            cameraObject.AddComponent(backgroundType);
        }
        else
        {
            Debug.LogWarning("LayeredCloudBackground2D was not found, so the layered cloud background was not added.");
        }
    }

    private static void CreateGround(Vector3 position, Vector3 scale)
    {
        GameObject ground = CreateBox("Ground", position, scale, Color.white, 0, false);
        ConfigureGroundVisual(ground, scale);
        ground.layer = LayerMask.NameToLayer("Default");
    }

    private static void ConfigureGroundVisual(GameObject ground, Vector3 scale)
    {
        SpriteRenderer renderer = ground.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            return;
        }

        Sprite grassSprite = Resources.Load<Sprite>(GroundSpriteResourcePath);
        if (grassSprite == null)
        {
            return;
        }

        renderer.sprite = grassSprite;
        renderer.color = Color.white;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(scale.x, scale.y);
        renderer.sortingOrder = 0;
    }

    private static PlayerController2D CreatePlayer(Vector3 position)
    {
        GameObject player = CreateBox("Player", position, new Vector3(0.65f, 1.05f, 1f), new Color(0.28f, 0.48f, 0.95f, 1f), 10, false);
        player.tag = "Player";
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var visual = player.AddComponent<SimpleHumanoidVisual>();
        visual.SetVisualRole(SimpleHumanoidVisual.VisualRole.Player);
        return player.AddComponent<PlayerController2D>();
    }

    private static void EnsureLevel2CharacterSettings(GameObject gameSystems, Transform playerTransform, Vector3 defaultPosition, Vector3 defaultScale)
    {
        Level2CharacterSettings settings = gameSystems.GetComponent<Level2CharacterSettings>();
        if (settings == null)
        {
            settings = gameSystems.AddComponent<Level2CharacterSettings>();
        }

        settings.Configure(playerTransform, defaultPosition, defaultScale);
    }

    private static PressurePlate2D CreatePlate(string name, Vector3 position)
    {
        GameObject plate = CreateBox(name, position, new Vector3(1.1f, 0.16f, 1f), new Color(1f, 0.72f, 0.05f, 1f), 4, true);
        return plate.AddComponent<PressurePlate2D>();
    }

    private static Door2D CreateDoor(Vector3 position)
    {
        GameObject door = CreateBox("Door", position, new Vector3(0.65f, 3.2f, 1f), new Color(0.95f, 0.25f, 0.25f, 1f), 6, false);
        return door.AddComponent<Door2D>();
    }

    private static Goal2D CreateGoal(Vector3 position)
    {
        GameObject goal = CreateBox("Goal", position, new Vector3(0.7f, 1.25f, 1f), new Color(0.2f, 0.9f, 0.35f, 1f), 3, true);
        return goal.AddComponent<Goal2D>();
    }

    private static HazardZone2D CreateHazardZone(string name, Vector3 position, Vector3 scale)
    {
        GameObject hazard = CreateBox(name, position, scale, new Color(0.95f, 0.22f, 0.08f, 1f), 2, true);
        return hazard.AddComponent<HazardZone2D>();
    }

    private static void ConfigureFireTrapVisual(GameObject hazard, Vector3 localPosition, Vector3 localScale, int sortingOrder)
    {
        SpriteRenderer rootRenderer = hazard.GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
            rootRenderer.sprite = null;
        }

        Transform visual = hazard.transform.Find("AnimatedVisual");
        if (visual == null)
        {
            GameObject child = new GameObject("AnimatedVisual");
            child.transform.SetParent(hazard.transform, false);
            visual = child.transform;
        }

        visual.localPosition = localPosition;
        visual.localRotation = Quaternion.identity;
        visual.localScale = localScale;

        SpriteRenderer visualRenderer = visual.GetComponent<SpriteRenderer>();
        if (visualRenderer == null)
        {
            visualRenderer = visual.gameObject.AddComponent<SpriteRenderer>();
        }

        visualRenderer.sortingOrder = sortingOrder;
        visualRenderer.maskInteraction = SpriteMaskInteraction.None;
        visualRenderer.color = Color.white;

        SpriteFrameAnimator2D animator = visual.GetComponent<SpriteFrameAnimator2D>();
        if (animator == null)
        {
            animator = visual.gameObject.AddComponent<SpriteFrameAnimator2D>();
        }

        animator.Configure("Art/Fire Trap", 7, 10f, Color.white);
    }

    private static GameObject CreatePlatform(Vector3 position, Vector3 scale, Color color)
    {
        GameObject platform = CreateBox("Platform", position, scale, color, 5, false);
        ConfigurePlatformVisual(platform, scale, color);
        return platform;
    }

    private static void ConfigurePlatformVisual(GameObject platform, Vector3 scale, Color hintColor)
    {
        SpriteRenderer renderer = platform.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            return;
        }

        Sprite stoneSprite = Resources.Load<Sprite>(PlatformSpriteResourcePath);
        if (stoneSprite == null)
        {
            return;
        }

        bool isSwitchableBridge = hintColor.g > 0.75f && hintColor.r < 0.4f;
        renderer.sprite = stoneSprite;
        renderer.color = isSwitchableBridge ? new Color(0.78f, 1f, 0.82f, 1f) : Color.white;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(scale.x, scale.y);
        renderer.sortingOrder = 5;
    }

    private static GameObject CreateCrate(Vector3 position)
    {
        GameObject crate = CreateBox("Crate", position, new Vector3(0.8f, 0.8f, 1f), new Color(0.6f, 0.4f, 0.2f, 1f), 7, false);
        Rigidbody2D rb = crate.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.mass = 4f;
        rb.drag = 4f;
        rb.angularDrag = 8f;
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
        var visual = clone.AddComponent<SimpleHumanoidVisual>();
        visual.SetVisualRole(SimpleHumanoidVisual.VisualRole.Clone);
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
        obj.transform.localScale = Vector3.one;

        SpriteRenderer renderer = SpriteBoxFactory.SetupSpriteBox(obj, color, sortingOrder);
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(scale.x, scale.y);

        if (renderer.sprite == null)
        {
            Debug.LogWarning($"CreateBox: Sprite was null for '{name}', forcing assignment.");
            Sprite white = SpriteBoxFactory.WhiteSprite;
            renderer.sprite = white;
        }

        BoxCollider2D collider = SpriteBoxFactory.SetupBoxCollider2D(obj, trigger, new Vector2(scale.x, scale.y));
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
