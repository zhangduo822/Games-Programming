using UnityEditor;
using UnityEngine;

public static class TagManagerSetup
{
    private const string CloneTagName = "Clone";

    [MenuItem("Tools/Temporal Paradox/Setup Tags & Layers")]
    public static void SetupTagsAndLayers()
    {
        CreateCloneTag();
        CreateCloneLayer();
        Debug.Log("Temporal Paradox: Tags and Layers configured successfully!");
    }

    private static void CreateCloneTag()
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
        else
        {
            Debug.Log($"Tag '{CloneTagName}' already exists.");
        }
    }

    private static void CreateCloneLayer()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );

        SerializedProperty layersProp = tagManager.FindProperty("layers");

        int emptySlot = -1;
        for (int i = 8; i < layersProp.arraySize; i++)
        {
            string layerName = layersProp.GetArrayElementAtIndex(i).stringValue;
            if (string.IsNullOrEmpty(layerName))
            {
                if (emptySlot < 0) emptySlot = i;
            }
            else if (layerName == "Clone")
            {
                Debug.Log("Layer 'Clone' already exists.");
                return;
            }
        }

        if (emptySlot >= 0)
        {
            layersProp.GetArrayElementAtIndex(emptySlot).stringValue = "Clone";
            tagManager.ApplyModifiedProperties();
            Debug.Log($"Created Layer: 'Clone' at slot {emptySlot}");
        }
        else
        {
            Debug.LogWarning("No empty layer slots available!");
        }
    }

    public static bool IsCloneTagDefined()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );

        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == CloneTagName)
            {
                return true;
            }
        }
        return false;
    }
}
