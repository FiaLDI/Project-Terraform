using UnityEngine;
using UnityEditor;
using Features.Biomes.Domain;

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

        // ----- INFO -----
        DrawHeader("Biome Info");
        DrawProps("biomeName", "mapColor", "isGenerate", "useLowPoly");

        DrawBiomePreview(config);

        // ----- TERRAIN -----
        DrawHeader("Terrain");
        DrawProps("terrainType", "groundMaterial", "terrainScale", "heightMultiplier");

        if ((TerrainType)serializedObject.FindProperty("terrainType").enumValueIndex ==
            TerrainType.FractalMountains)
        {
            DrawProps("fractalOctaves", "fractalPersistence", "fractalLacunarity");
        }

        DrawHeader("Texture / UV");
        DrawProps("textureTiling");

        DrawHeader("Blending");
        DrawProps("blendStrength");

        // ----- ENVIRONMENT -----
        DrawHeader("Environment Objects");
        DrawProps("environmentPrefabs", "environmentDensity");

        // ----- RESOURCES -----
        DrawHeader("Resources");
        DrawProps("possibleResources", "resourceDensity", "resourceSpawnYOffset", "resourceEdgeFalloff");

        // ----- QUESTS -----
        DrawHeader("Quests");
        EditorGUILayout.PropertyField(questsProp, true);
        DrawProps("questTargetsMin", "questTargetsMax");

        // ----- ENEMY -----
        DrawHeader("Enemies");
        DrawProps("enemyTable", "enemyDensity", "enemyRespawnDelay");

        // ----- SKYBOX / FOG -----
        DrawHeader("Skybox / UI / Fog Gradient");
        DrawProps("skyboxMaterial", "skyTopColor", "skyBottomColor", "skyExposure",
                  "uiColor", "fogLightColor", "fogHeavyColor", "fogGradientScale");

        DrawHeader("Fog Settings");
        DrawProps("enableFog", "fogMode", "fogColor", "fogDensity", "fogLinearStart", "fogLinearEnd");

        // ----- WEATHER -----
        DrawHeader("Weather");
        DrawProps("rainPrefab", "dustPrefab", "firefliesPrefab", "weatherIntensity");

        // ----- WATER -----
        DrawHeader("Water");
        DrawProps("useWater", "waterType", "seaLevel",
                  "waterMaterial", "swampWaterMaterial", "lakeWaterMaterial", "oceanWaterMaterial");

        // ----- LAKES -----
        DrawHeader("Lakes");
        DrawProps("generateLakes", "lakeLevel", "lakeNoiseScale");

        // ----- RIVERS -----
        DrawHeader("Rivers");
        DrawProps("generateRivers", "riverNoiseScale", "riverWidth", "riverDepth");

        // ----- SIZE -----
        DrawHeader("Biome Area Size");
        DrawProps("width", "height");

        serializedObject.ApplyModifiedProperties();
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

    private void DrawBiomePreview(BiomeConfig config)
    {
        if (_preview == null)
            _preview = new Texture2D(PreviewSize, PreviewSize);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mini-map Preview", EditorStyles.boldLabel);

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
}
