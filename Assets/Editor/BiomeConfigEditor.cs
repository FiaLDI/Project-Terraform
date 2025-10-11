using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(BiomeConfig))]
public class BiomeConfigEditor : Editor
{
    GameObject lastGenerated;

    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // синхронизируем сериализацию
        BiomeConfig config = (BiomeConfig)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚙️ Biome Configuration", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("biomeName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mapColor"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("height"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundMaterial"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainScale"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("heightMultiplier"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("environmentPrefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("environmentDensity"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("resourcePrefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resourceDensity"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("questPrefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questSpawnChance"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("weatherPrefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientSounds"), true);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("skyboxMaterial"));

        serializedObject.ApplyModifiedProperties(); // сохраняем изменения в .asset

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚒️ Генерация", EditorStyles.boldLabel);

        if (GUILayout.Button("▶ Generate Biome in Scene"))
        {
            lastGenerated = GenerateBiome(config);
        }

        if (lastGenerated != null)
        {
            if (GUILayout.Button("❌ Delete Last Generated"))
            {
                Undo.DestroyObjectImmediate(lastGenerated);
                lastGenerated = null;
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("📄 Управление ассетом", EditorStyles.boldLabel);

        if (GUILayout.Button("📄 Save Config As New"))
        {
            SaveConfigAsNew(config);
        }
    }

    private GameObject GenerateBiome(BiomeConfig config)
    {
        GameObject biomeRoot = new GameObject(config.biomeName + "_Generated");
        Undo.RegisterCreatedObjectUndo(biomeRoot, "Generate Biome");

        BiomeGenerator generator = biomeRoot.AddComponent<BiomeGenerator>();
        generator.biome = config;
        generator.Generate();

        return biomeRoot;
    }

    private void SaveConfigAsNew(BiomeConfig originalConfig)
    {
        string originalPath = AssetDatabase.GetAssetPath(originalConfig);
        string directory = Path.GetDirectoryName(originalPath);
        string fileName = Path.GetFileNameWithoutExtension(originalPath);

        string newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{fileName}_Copy.asset");

        BiomeConfig newConfig = Instantiate(originalConfig);
        AssetDatabase.CreateAsset(newConfig, newPath);
        AssetDatabase.SaveAssets();

        EditorGUIUtility.PingObject(newConfig);
        Debug.Log($"✅ BiomeConfig скопирован: {newPath}");
    }
}
