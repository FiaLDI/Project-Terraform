using UnityEngine;
using UnityEditor;
using System.IO;
using Quests;

[CustomEditor(typeof(BiomeConfig))]
public class BiomeConfigEditor : Editor
{
    GameObject lastGenerated;
    BiomeGenerator generator;

    private SerializedProperty questsProp;

    private void OnEnable()
    {
        questsProp = serializedObject.FindProperty("possibleQuests");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        BiomeConfig config = (BiomeConfig)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚙️ Biome Configuration", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("biomeName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mapColor"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("isGenerate"));

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

        // --- КВЕСТЫ ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🎯 Квесты", EditorStyles.boldLabel);
        DrawQuestEntries(questsProp);

        EditorGUILayout.Space();

        // Эффекты
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weatherPrefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientSounds"), true);

        EditorGUILayout.Space();

        // Небо
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skyboxMaterial"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🌫 Fog Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("enableFog"));
        if (config.enableFog)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogColor"));

            if (config.fogMode == FogMode.Linear)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fogLinearStart"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fogLinearEnd"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fogDensity"));
            }
        }

        serializedObject.ApplyModifiedProperties();

        // --- Генерация ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚒️ Генерация", EditorStyles.boldLabel);

        if (generator != null)
        {
            generator.autoSpawnQuests = EditorGUILayout.Toggle("Auto Spawn Quests", generator.autoSpawnQuests);
        }
        else
        {
            EditorGUILayout.HelpBox("Сначала сгенерируйте биом, чтобы управлять настройками генератора.", MessageType.Info);
        }

        if (GUILayout.Button("▶ Generate Biome in Scene"))
        {
            if (lastGenerated != null)
            {
                DestroyImmediate(lastGenerated);
                lastGenerated = null;
            }
            lastGenerated = GenerateBiome(config);
        }

        if (lastGenerated != null)
        {
            if (GUILayout.Button("🎯 Generate Quests Only"))
            {
                if (generator != null)
                {
                    generator.SpawnQuests();
                }
            }

            if (GUILayout.Button("❌ Delete Last Generated"))
            {
                DestroyImmediate(lastGenerated);
                lastGenerated = null;
                generator = null;
            }

            if (GUILayout.Button("🌀 Sandstorm Test (5s)"))
            {
                if (generator != null)
                {
                    generator.StartSandstorm(5f);
                    EditorApplication.delayCall += () =>
                    {
                        if (generator != null)
                            generator.EndSandstorm(5f);
                    };
                }
            }
        }

        // --- Управление ассетом ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("📄 Управление ассетом", EditorStyles.boldLabel);

        if (GUILayout.Button("📄 Save Config As New"))
        {
            SaveConfigAsNew(config);
        }
    }

    private void DrawQuestEntries(SerializedProperty list)
    {
        if (list == null) return;

        EditorGUILayout.BeginVertical("box");

        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("helpbox");

            var questAssetProp = element.FindPropertyRelative("questAsset");
            var questAsset = questAssetProp.objectReferenceValue as QuestAsset;
            string questName = questAsset != null ? questAsset.questName : "None";

            EditorGUILayout.LabelField($"Quest Entry {i + 1}: {questName}", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(questAssetProp, new GUIContent("Quest Asset"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("questPointPrefab"), new GUIContent("Point Prefab"));

            EditorGUILayout.Slider(element.FindPropertyRelative("spawnChance"), 0f, 1f, new GUIContent("Spawn Chance"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("minTargets"), new GUIContent("Min Targets"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("maxTargets"), new GUIContent("Max Targets"));

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("▲", GUILayout.Width(30)) && i > 0)
                list.MoveArrayElement(i, i - 1);
            if (GUILayout.Button("▼", GUILayout.Width(30)) && i < list.arraySize - 1)
                list.MoveArrayElement(i, i + 1);
            if (GUILayout.Button("✖", GUILayout.Width(30)))
                list.DeleteArrayElementAtIndex(i);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ Add Quest Entry"))
        {
            list.InsertArrayElementAtIndex(list.arraySize);
        }

        EditorGUILayout.EndVertical();
    }

    private GameObject GenerateBiome(BiomeConfig config)
    {
        string rootName = config.biomeName + (config.isGenerate ? "_Location" : "_Generator");
        GameObject biomeRoot = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(biomeRoot, "Generate Biome");

        generator = biomeRoot.AddComponent<BiomeGenerator>();
        generator.biome = config;

        if (config.isGenerate)
        {
            // ⚡ Генерация локации
            generator.Generate();

            // После генерации удаляем компонент BiomeGenerator
            DestroyImmediate(generator);
            generator = null;

            Debug.Log($"✅ Biome '{config.biomeName}' сгенерирован как Location.");
        }
        else
        {
            // ⚡ Локация не создаётся, остаётся только объект с BiomeGenerator
            Debug.Log($"⚙️ Biome '{config.biomeName}' создан как Generator (isGenerate = false).");
        }

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
