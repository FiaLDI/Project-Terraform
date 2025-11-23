using UnityEngine;
using UnityEditor;
using Quests;

[CustomEditor(typeof(BiomeConfig))]
public class BiomeConfigEditor : Editor
{
    private const int PreviewSize = 128;
    private Texture2D _preview;

    private SerializedProperty questsProp;

    private void OnEnable()
    {
        questsProp = serializedObject.FindProperty("possibleQuests");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var config = (BiomeConfig)target;

        // ─────────── Базовая информация ───────────
        EditorGUILayout.LabelField("Biome Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("biomeName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mapColor"));

        DrawBiomePreview(config);

        // ─────────── Параметры генерации ───────────
        DrawHeader("Terrain");
        DrawProps("terrainType", "groundMaterial", "terrainScale", "heightMultiplier");

        if ((TerrainType)serializedObject.FindProperty("terrainType").enumValueIndex ==
            TerrainType.FractalMountains)
        {
            DrawProps("fractalOctaves", "fractalPersistence", "fractalLacunarity");
        }

        DrawHeader("LowPoly");
        DrawProps("useLowPoly");

        DrawHeader("Environment");
        DrawProps("environmentPrefabs", "environmentDensity");

        DrawHeader("Resources");
        DrawProps("possibleResources", "resourceDensity", "resourceSpawnYOffset");

        // ─────────── КВЕСТЫ ───────────
        DrawHeader("🎯 Quests");
        DrawQuestEditor();

        // ─────────── Эффекты ───────────
        DrawHeader("Effects");
        DrawProps("weatherPrefabs", "ambientSounds", "skyboxMaterial");

        DrawHeader("Fog");
        DrawProps("enableFog", "fogMode", "fogColor", "fogDensity", "fogLinearStart", "fogLinearEnd");

  

         DrawHeader("Water");
        DrawProps(
            "useWater",
            "seaLevel",
            "waterMaterial",
            "generateLakes",
            "lakeLevel",
            "lakeNoiseScale",
            "generateRivers",
            "riverNoiseScale",
            "riverWidth",
            "riverDepth"
        );

        
        serializedObject.ApplyModifiedProperties();

        // ─────────── ТЕСТОВЫЙ СПАВН КВЕСТОВ ───────────
        DrawHeader("Debug Tools");

        if (GUILayout.Button("🎯 Test Spawn Quests In Scene"))
        {
            TestSpawnQuests(config);
        }

        DrawHeader("Biome Generation (ChunkManager)");

        if (GUILayout.Button("▶ Generate Biome Preview (ChunkManager)"))
        {
            GenerateBiomePreview(config);
        }
    }

    private void DrawHeader(string title)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }

    private void DrawProps(params string[] properties)
    {
        foreach (string p in properties)
        {
            var prop = serializedObject.FindProperty(p);
            if (prop != null)
                EditorGUILayout.PropertyField(prop, true);
        }
    }

    // ─────────── Мини-карта ───────────
    private void DrawBiomePreview(BiomeConfig config)
    {
        if (_preview == null)
            _preview = new Texture2D(PreviewSize, PreviewSize);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mini-map", EditorStyles.boldLabel);

        GeneratePreviewTexture(config, _preview);
        Rect r = GUILayoutUtility.GetRect(PreviewSize, PreviewSize);
        EditorGUI.DrawPreviewTexture(r, _preview);
    }

    private void GeneratePreviewTexture(BiomeConfig config, Texture2D tex)
    {
        int w = tex.width;
        int h = tex.height;

        float maxH = config.heightMultiplier + 0.001f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float bx = (float)x / (w - 1) * config.width;
                float bz = (float)y / (h - 1) * config.height;

                float height = BiomeHeightUtility.GetHeight(config, bx, bz);
                float t = height / maxH;

                Color c = Color.Lerp(config.mapColor * 0.5f, Color.white, t);
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
    }

    // ─────────── Редактор квестов ───────────
    private void DrawQuestEditor()
    {
        EditorGUILayout.PropertyField(questsProp, true);
    }

    // ─────────── Test Spawn ───────────
    private void TestSpawnQuests(BiomeConfig config)
    {
        if (config.possibleQuests == null || config.possibleQuests.Length == 0)
        {
            Debug.LogWarning("No quests defined.");
            return;
        }

        foreach (var entry in config.possibleQuests)
        {
            if (entry.questAsset == null || entry.questPointPrefab == null)
                continue;

            int count = Random.Range(entry.minTargets, entry.maxTargets + 1);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(0f, config.width),
                    1000f,
                    Random.Range(0f, config.height)
                );

                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 5000f))
                {
                    pos = hit.point + Vector3.up * 0.5f;
                }

                GameObject point = PrefabUtility.InstantiatePrefab(entry.questPointPrefab) as GameObject;
                point.transform.position = pos;

                var qp = point.GetComponent<QuestPoint>();
                if (qp != null)
                {
                    qp.linkedQuest = entry.questAsset;
                }
            }
        }

        Debug.Log("🎯 Test quests spawned into scene.");
    }

    private void GenerateBiomePreview(BiomeConfig config)
{
    // ищем WorldConfig, чтобы взять chunkSize и blending
    string[] guids = AssetDatabase.FindAssets("t:WorldConfig");
    if (guids.Length == 0)
    {
        Debug.LogError("❌ WorldConfig not found in project!");
        return;
    }

    WorldConfig world = AssetDatabase.LoadAssetAtPath<WorldConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));

    GameObject old = GameObject.Find("BiomePreview_" + config.biomeName);
    if (old != null)
        GameObject.DestroyImmediate(old);

    GameObject previewRoot = new GameObject("BiomePreview_" + config.biomeName);

    ChunkManager manager = new ChunkManager(world);

    // Генерация довольно маленькой зоны вокруг 0,0
    Vector2Int center = new Vector2Int(0, 0);
    int radius = 3; // 7×7 чанков

    GameObject area = manager.GenerateStaticArea(center, radius);
    area.transform.SetParent(previewRoot.transform);

    Debug.Log($"✅ Biome preview generated for '{config.biomeName}' using ChunkManager.");
}
}
