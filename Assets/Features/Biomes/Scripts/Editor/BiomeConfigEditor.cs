using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;

[CustomEditor(typeof(BiomeConfig))]
public class BiomeConfigEditor : Editor
{
    private const int PreviewSize = 128;

    // Preview
    private Texture2D _preview;
    private bool _previewDirty = true;
    private bool showAdvancedPreview = true;

    // Foldouts
    private bool fInfo = true;
    private bool fTerrain = true;
    private bool fEnvironment = true;
    private bool fResources = true;
    private bool fQuests = true;
    private bool fEnemies = true;
    private bool fSkyFog = true;
    private bool fFogSettings = true;
    private bool fWeather = true;
    private bool fWater = true;
    private bool fLakes = true;
    private bool fRivers = true;
    private bool fSize = true;

    // Serialized properties
    private SerializedProperty pQuests;
    private SerializedProperty pEnvPrefabs;
    private SerializedProperty pResources;
    private SerializedProperty pEnemies;

    // Reorderable lists
    private ReorderableList questList;
    private ReorderableList envPrefabList;
    private ReorderableList resourceList;
    private ReorderableList enemyList;

    private void OnEnable()
    {
        pQuests     = serializedObject.FindProperty("possibleQuests");
        pEnvPrefabs = serializedObject.FindProperty("environmentPrefabs");
        pResources  = serializedObject.FindProperty("possibleResources");
        pEnemies    = serializedObject.FindProperty("enemyTable");

        SetupLists();
    }

    private void SetupLists()
    {
        if (pQuests != null)
        {
            questList = new ReorderableList(serializedObject, pQuests, true, true, true, true);
            questList.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "Possible Quests");

            questList.drawElementCallback = (rect, index, active, focused) =>
            {
                var elem = pQuests.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(rect, elem, GUIContent.none, true);
            };

            questList.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(pQuests.GetArrayElementAtIndex(index), true) + 4;
        }

        if (pEnvPrefabs != null)
        {
            envPrefabList = new ReorderableList(serializedObject, pEnvPrefabs, true, true, true, true);
            envPrefabList.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "Environment Prefabs");

            envPrefabList.drawElementCallback = (rect, index, active, focused) =>
            {
                var elem = pEnvPrefabs.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(rect, elem, GUIContent.none, true);
            };

            envPrefabList.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(pEnvPrefabs.GetArrayElementAtIndex(index), true) + 4;
        }

        if (pResources != null)
        {
            resourceList = new ReorderableList(serializedObject, pResources, true, true, true, true);
            resourceList.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "Possible Resources");

            resourceList.drawElementCallback = (rect, index, active, focused) =>
            {
                var elem = pResources.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(rect, elem, GUIContent.none, true);
            };

            resourceList.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(pResources.GetArrayElementAtIndex(index), true) + 4;
        }

        if (pEnemies != null)
        {
            enemyList = new ReorderableList(serializedObject, pEnemies, true, true, true, true);
            enemyList.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "Enemy Spawn Table");

            enemyList.drawElementCallback = (rect, index, active, focused) =>
            {
                var elem = pEnemies.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(rect, elem, GUIContent.none, true);
            };

            enemyList.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(pEnemies.GetArrayElementAtIndex(index), true) + 4;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var cfg = (BiomeConfig)target;

        DrawAllSections(cfg);

        serializedObject.ApplyModifiedProperties();

        DrawPreviewSection(cfg);
    }

    // ---------------------------------------------------------
    //  SECTIONS
    // ---------------------------------------------------------

    private void DrawAllSections(BiomeConfig cfg)
    {
        fInfo = DrawFold("Biome Info", fInfo, () =>
        {
            DrawProps("biomeID", "biomeName", "mapColor", "isGenerate", "useLowPoly");
        });

        fTerrain = DrawFold("Terrain", fTerrain, () =>
        {
            DrawProps("terrainType", "groundMaterial", "terrainScale", "heightMultiplier");

            if ((TerrainType)serializedObject.FindProperty("terrainType").enumValueIndex
                 == TerrainType.FractalMountains)
            {
                DrawProps("fractalOctaves", "fractalPersistence", "fractalLacunarity");
            }

            DrawProps("textureTiling", "blendStrength");
        });

        fEnvironment = DrawFold("Environment Objects", fEnvironment, () =>
        {
            envPrefabList.DoLayoutList();
            DrawProps("environmentDensity");
        });

        fResources = DrawFold("Resources", fResources, () =>
        {
            resourceList.DoLayoutList();
            DrawProps("resourceDensity", "resourceSpawnYOffset", "resourceEdgeFalloff");
        });

        fQuests = DrawFold("Quests", fQuests, () =>
        {
            questList.DoLayoutList();
            DrawProps("questTargetsMin", "questTargetsMax");
        });

        fEnemies = DrawFold("Enemies", fEnemies, () =>
        {
            enemyList.DoLayoutList();
            DrawProps("enemyDensity", "enemyRespawnDelay");
        });

        fSkyFog = DrawFold("Skybox / UI / Fog", fSkyFog, () =>
        {
            DrawProps("skyboxMaterial", "skyTopColor", "skyBottomColor",
                      "skyExposure", "uiColor", "fogLightColor",
                      "fogHeavyColor", "fogGradientScale");
        });

        fFogSettings = DrawFold("Fog Settings", fFogSettings, () =>
        {
            DrawProps("enableFog", "fogMode", "fogColor",
                      "fogDensity", "fogLinearStart", "fogLinearEnd");
        });

        fWeather = DrawFold("Weather", fWeather, () =>
        {
            DrawProps("rainPrefab", "dustPrefab", "firefliesPrefab", "weatherIntensity");
        });

        fWater = DrawFold("Water", fWater, () =>
        {
            DrawProps("useWater", "waterType", "seaLevel",
                      "waterMaterial", "swampWaterMaterial",
                      "lakeWaterMaterial", "oceanWaterMaterial");
        });

        fLakes = DrawFold("Lakes", fLakes, () =>
        {
            DrawProps("generateLakes", "lakeLevel", "lakeNoiseScale");
        });

        fRivers = DrawFold("Rivers", fRivers, () =>
        {
            DrawProps("generateRivers");

            if (serializedObject.FindProperty("generateRivers").boolValue)
                DrawProps("riverNoiseScale", "riverWidth", "riverDepth");
        });

        fSize = DrawFold("Biome Area Size", fSize, () =>
        {
            DrawProps("width", "height");
        });
    }

    // ---------------------------------------------------------
    //  DRAW HELPERS
    // ---------------------------------------------------------

    private bool DrawFold(string title, bool value, System.Action content)
    {
        GUILayout.Space(4);
        value = EditorGUILayout.Foldout(value, title, true, EditorStyles.foldoutHeader);

        if (value)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(4);
            content();
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }

        return value;
    }

    private void DrawProps(params string[] props)
    {
        foreach (var p in props) DrawProp(p);
    }

    private void DrawProp(string prop)
    {
        var p = serializedObject.FindProperty(prop);
        if (p != null)
            EditorGUILayout.PropertyField(p, true);
        else
            EditorGUILayout.HelpBox($"Property '{prop}' not found!", MessageType.Error);
    }

    // ---------------------------------------------------------
    // PREVIEW
    // ---------------------------------------------------------

    private void DrawPreviewSection(BiomeConfig cfg)
    {
        GUILayout.Space(15);

        if (_previewDirty || _preview == null)
            UpdatePreview(cfg);

        EditorGUILayout.LabelField("Biome Preview", EditorStyles.boldLabel);

        Rect r = GUILayoutUtility.GetRect(PreviewSize, PreviewSize);
        if (_preview != null)
            EditorGUI.DrawPreviewTexture(r, _preview, null, ScaleMode.StretchToFill);

        if (GUILayout.Button("Regenerate Preview"))
        {
            UpdatePreview(cfg);
        }
    }

    private void UpdatePreview(BiomeConfig cfg)
    {
        if (_preview == null)
            _preview = new Texture2D(PreviewSize, PreviewSize);

        try
        {
            GeneratePreview(cfg, _preview);
        }
        catch
        {
            // ignore preview errors
        }

        _previewDirty = false;
    }

    private void GeneratePreview(BiomeConfig cfg, Texture2D tex)
    {
        int w = tex.width;
        int h = tex.height;

        float maxH = Mathf.Max(0.001f, cfg.heightMultiplier);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float nx = (float)x / (w - 1) * cfg.width;
                float nz = (float)y / (h - 1) * cfg.height;

                float height = BiomeHeightUtility.GetHeight(cfg, nx, nz);
                float t = height / maxH;

                Color c = Color.Lerp(cfg.mapColor * 0.4f, Color.white, t);
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
    }
}
