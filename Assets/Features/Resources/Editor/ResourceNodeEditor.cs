using UnityEngine;
using UnityEditor;

public class ResourceNodeEditor : MonoBehaviour
{
    [MenuItem("Assets/Create/Resources/Resource", priority = 0)]
    public static void CreateResource()
    {
        CreateScriptableObject<ResourceSO>("Resource");
    }

    [MenuItem("Assets/Create/Resources/Resource Node", priority = 1)]
    public static void CreateResourceNode()
    {
        CreateScriptableObject<ResourceNodeSO>("ResourceNode");
    }

    [MenuItem("Assets/Create/Resources/Resource Drop", priority = 2)]
    public static void CreateResourceDrop()
    {
        CreateScriptableObject<ResourceDropSO>("ResourceDrop");
    }

    [MenuItem("Assets/Create/Resources/Biome Spawn Table", priority = 3)]
    public static void CreateBiomeSpawnTable()
    {
        CreateScriptableObject<BiomeSpawnTableSO>("BiomeSpawnTable");
    }

    private static void CreateScriptableObject<T>(string defaultName) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
            path = "Assets";
        else if (!System.IO.Directory.Exists(path))
            path = System.IO.Path.GetDirectoryName(path);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{defaultName}.asset");
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
