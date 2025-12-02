using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;

[CustomEditor(typeof(BiomeConfig))]
public class BiomeConfigEditor : Editor
{
    private const int PreviewSize = 128;

    private Texture2D _preview;
    private bool _previewDirty = true;

    // Foldouts
    private bool showAdvancedPreview = true;
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

    // Lists
    private ReorderableList questList;
    private ReorderableList envPrefabList;
    private ReorderableList resourceList;

    private SerializedProperty pQuests;
    private SerializedProperty pEnvPrefabs;
    private SerializedProperty pResources;

    private void OnEnable()
    {
        pQuests = serializedObject.FindProperty("possibleQuests");
        pEnvPrefabs = serializedObject.FindProperty("environmentPrefabs");
        pResources = serializedObject.FindProperty("possibleResources");

        SetupReorderableLists();
    }

    private void SetupReorderableLists()
    {
        questList = new ReorderableList(serializedObject, pQuests, true, true, true, true);
        questList.drawElementCallback = (rect, index, active, focused) =>
        {
            EditorGUI.PropertyField(rect, pQuests.GetArrayElementAtIndex(index), GUIContent.none);
        };
        questList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Possible Quests");

        envPrefabList = new ReorderableList(serializedObject, pEnvPrefabs, true, true, true, true);
        envPrefabList.drawElementCallback = (rect, index, active, focused) =>
        {
            EditorGUI.PropertyField(rect, pEnvPrefabs.GetArrayElementAtIndex(index), GUIContent.none);
        };
        envPrefabList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Environment Prefabs");

        resourceList = new ReorderableList(serializedObject, pResources, true, true, true, true);
        resourceList.drawElementCallback = (rect, index, active, focused) =>
        {
            EditorGUI.PropertyField(rect, pResources.GetArrayElementAtIndex(index), GUIContent.none);
        };
        resourceList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Possible Resources");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var config = (BiomeConfig)target;

        EditorGUI.BeginChangeCheck();
        DrawInspector(config);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            _previewDirty = true;
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }

        GUILayout.Space(10);

        if (_previewDirty || _preview == null)
            UpdatePreview(config);

        DrawPreview();

        GUILayout.Space(10);

        if (GUILayout.Button("Regenerate Preview"))
        {
            UpdatePreview(config);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Test Chunk (64×64)"))
        {
            GenerateTestChunk((BiomeConfig)target, 64, 64);
        }

        if (GUILayout.Button("Generate Test Chunk (128×128)"))
        {
            GenerateTestChunk((BiomeConfig)target, 128, 128);
        }
    }

    // ------------------------
    //   INSPECTOR SECTIONS
    // ------------------------

    private void DrawInspector(BiomeConfig config)
    {
        fInfo = DrawFoldout("Biome Info", fInfo, () =>
        {
            DrawProps("biomeName", "mapColor", "isGenerate", "useLowPoly");
        });

        fTerrain = DrawFoldout("Terrain", fTerrain, () =>
        {
            DrawProps("terrainType", "groundMaterial", "terrainScale", "heightMultiplier");

            if ((TerrainType)serializedObject.FindProperty("terrainType").enumValueIndex == TerrainType.FractalMountains)
            {
                DrawProps("fractalOctaves", "fractalPersistence", "fractalLacunarity");
            }

            DrawHeaderSmall("Texture / UV");
            DrawProps("textureTiling");

            DrawHeaderSmall("Blending");
            DrawProps("blendStrength");
        });

        fEnvironment = DrawFoldout("Environment Objects", fEnvironment, () =>
        {
            envPrefabList.DoLayoutList();
            DrawProps("environmentDensity");
        });

        fResources = DrawFoldout("Resources", fResources, () =>
        {
            resourceList.DoLayoutList();
            DrawProps("resourceDensity", "resourceSpawnYOffset", "resourceEdgeFalloff");

            if (serializedObject.FindProperty("resourceDensity").floatValue <= 0f)
                EditorGUILayout.HelpBox("Resource density = 0, ресурсы спавниться НЕ будут.", MessageType.Warning);
        });

        fQuests = DrawFoldout("Quests", fQuests, () =>
        {
            questList.DoLayoutList();
            DrawProps("questTargetsMin", "questTargetsMax");
        });

        fEnemies = DrawFoldout("Enemies", fEnemies, () =>
        {
            DrawProps("enemyTable", "enemyDensity", "enemyRespawnDelay");
        });

        fSkyFog = DrawFoldout("Skybox / UI / Fog Colors", fSkyFog, () =>
        {
            DrawProps("skyboxMaterial", "skyTopColor", "skyBottomColor", "skyExposure",
                      "uiColor", "fogLightColor", "fogHeavyColor", "fogGradientScale");
        });

        fFogSettings = DrawFoldout("Fog Settings", fFogSettings, () =>
        {
            DrawProps("enableFog", "fogMode", "fogColor", "fogDensity", "fogLinearStart", "fogLinearEnd");
        });

        fWeather = DrawFoldout("Weather", fWeather, () =>
        {
            DrawProps("rainPrefab", "dustPrefab", "firefliesPrefab", "weatherIntensity");
        });

        fWater = DrawFoldout("Water", fWater, () =>
        {
            DrawProps("useWater", "waterType", "seaLevel",
                      "waterMaterial", "swampWaterMaterial", "lakeWaterMaterial", "oceanWaterMaterial");

            if (!serializedObject.FindProperty("useWater").boolValue)
                EditorGUILayout.HelpBox("Water disabled in this biome.", MessageType.Info);
        });

        fLakes = DrawFoldout("Lakes", fLakes, () =>
        {
            DrawProps("generateLakes", "lakeLevel", "lakeNoiseScale");
        });

        fRivers = DrawFoldout("Rivers", fRivers, () =>
        {
            DrawProps("generateRivers");

            if (serializedObject.FindProperty("generateRivers").boolValue)
                DrawProps("riverNoiseScale", "riverWidth", "riverDepth");
        });

        fSize = DrawFoldout("Biome Area Size", fSize, () =>
        {
            DrawProps("width", "height");
        });
    }

    // ------------------------
    //   DRAW HELPERS
    // ------------------------

    private bool DrawFoldout(string title, bool state, System.Action content)
    {
        GUILayout.Space(6);

        state = EditorGUILayout.Foldout(state, title, true, EditorStyles.foldoutHeader);

        if (state)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(4);
            content();
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }

        return state;
    }


    private void DrawHeaderSmall(string title)
    {
        GUILayout.Space(4);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }

    private void DrawProps(params string[] names)
    {
        foreach (var n in names)
            DrawProp(n);
    }

    private void DrawProp(string name)
    {
        var p = serializedObject.FindProperty(name);
        if (p == null)
        {
            EditorGUILayout.HelpBox($"Property '{name}' not found!", MessageType.Error);
            return;
        }

        EditorGUILayout.PropertyField(p, true);
    }

    // ------------------------
    //   PREVIEW GENERATION
    // ------------------------

    private void UpdatePreview(BiomeConfig config)
    {
        if (_preview == null)
            _preview = new Texture2D(PreviewSize, PreviewSize);

        if (showAdvancedPreview)
            GeneratePreviewTextureAdvanced(config, _preview);
        else
            GeneratePreviewTexture(config, _preview);
        _previewDirty = false;
    }

    private void DrawPreview()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Biome Preview", EditorStyles.boldLabel);

        Rect r = GUILayoutUtility.GetRect(PreviewSize, PreviewSize);
        EditorGUI.DrawPreviewTexture(r, _preview, null, ScaleMode.StretchToFill);
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

    private void GeneratePreviewTextureAdvanced(BiomeConfig config, Texture2D tex)
    {
        int w = tex.width;
        int h = tex.height;

        float maxH = Mathf.Max(0.001f, config.heightMultiplier);

        float seaLevel = config.seaLevel;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float bx = (float)x / (w - 1) * config.width;
                float bz = (float)y / (h - 1) * config.height;

                float height = BiomeHeightUtility.GetHeight(config, bx, bz);
                float t = height / maxH;

                Color c;

                // --- ВОДА ---
                if (config.useWater && height <= seaLevel)
                {
                    c = Color.Lerp(
                        new Color(0, 0.1f, 0.3f),
                        new Color(0.1f, 0.3f, 0.6f),
                        Mathf.InverseLerp(seaLevel - 5, seaLevel, height));
                }
                else
                {
                    // --- НОРМАЛЬНЫЙ ЛАНДШАФТ ---
                    Color low = config.skyBottomColor * 0.75f;
                    Color high = Color.Lerp(config.skyTopColor, Color.white, 0.5f);

                    c = Color.Lerp(low, high, t);

                    // выделение склонов
                    if (config.terrainType == TerrainType.FractalMountains)
                    {
                        float slope = Mathf.Abs(
                            BiomeHeightUtility.GetHeight(config, bx + 0.1f, bz) - height
                        );
                        c = Color.Lerp(c, Color.gray, slope * 2f);
                    }
                }

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
    }

    private void GenerateTestChunk(BiomeConfig config, int size, int resolution)
    {
        GameObject root = GameObject.Find("BiomePreview");
        if (root == null)
            root = new GameObject("BiomePreview");

        while (root.transform.childCount > 0)
            DestroyImmediate(root.transform.GetChild(0).gameObject);

        GameObject chunkObj = new GameObject($"PreviewChunk_{size}");
        chunkObj.transform.SetParent(root.transform);
        chunkObj.transform.position = Vector3.zero;

        MeshFilter mf = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();
        mr.sharedMaterial = config.groundMaterial;

        Mesh mesh = GenerateMeshFromBiome(config, size, resolution);
        mf.sharedMesh = mesh;

        var col = chunkObj.AddComponent<MeshCollider>();
        col.sharedMesh = mesh;

        Selection.activeObject = chunkObj;

        Debug.Log($"Generated preview chunk ({size}m, res {resolution})");
    }

    private Mesh GenerateMeshFromBiome(BiomeConfig biome, int size, int res)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        int vCount = (res + 1) * (res + 1);
        Vector3[] verts = new Vector3[vCount];
        Vector2[] uvs = new Vector2[vCount];
        int[] tris = new int[res * res * 6];

        int i = 0;
        for (int y = 0; y <= res; y++)
        {
            for (int x = 0; x <= res; x++)
            {
                // ВАЖНО: биомные координаты, а не world-size
                float wx = (float)x / res * biome.width;
                float wz = (float)y / res * biome.height;

                float h = BiomeHeightUtility.GetHeight(biome, wx, wz);

                verts[i] = new Vector3(
                    (float)x / res * size,
                    h,
                    (float)y / res * size
                );

                uvs[i] = new Vector2((float)x / res, (float)y / res);
                i++;
            }
        }

        int t = 0;
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                int a = y * (res + 1) + x;
                int b = a + 1;
                int c = a + (res + 1);
                int d = c + 1;

                tris[t++] = a; tris[t++] = d; tris[t++] = c;
                tris[t++] = a; tris[t++] = b; tris[t++] = d;
            }
        }

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        return mesh;
    }



}
