using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(BiomeConfig))]
public class BiomeConfigEditor : Editor
{
    GameObject lastGenerated;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        BiomeConfig config = (BiomeConfig)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚙️ Biome Configuration", EditorStyles.boldLabel);

        // Основное
        EditorGUILayout.PropertyField(serializedObject.FindProperty("biomeName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mapColor"));

        EditorGUILayout.Space();

        // Размер карты
        EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("height"));

        EditorGUILayout.Space();

        // Рельеф
        EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundMaterial"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainScale"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("heightMultiplier"));

        // ✅ Параметры для FractalMountains
        if (config.terrainType == TerrainType.FractalMountains)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fractal Mountains Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fractalOctaves"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fractalPersistence"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fractalLacunarity"));
        }

        EditorGUILayout.Space();

        // Окружение
        EditorGUILayout.PropertyField(serializedObject.FindProperty("environmentPrefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("environmentDensity"));

        // Ресурсы
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resourcePrefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resourceDensity"));

        // Квесты
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questPrefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questSpawnChance"));

        // Эффекты
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weatherPrefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientSounds"), true);

        EditorGUILayout.Space();

        // Небо
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skyboxMaterial"));

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚒️ Генерация", EditorStyles.boldLabel);

        if (GUILayout.Button("▶ Generate Biome in Scene"))
        {
            if (lastGenerated != null)
            {
                DestroyImmediate(lastGenerated); // очистим старое перед новой генерацией
            }
            lastGenerated = GenerateBiome(config);
        }

        if (lastGenerated != null)
        {
            if (GUILayout.Button("❌ Delete Last Generated"))
            {
                DestroyImmediate(lastGenerated);
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
