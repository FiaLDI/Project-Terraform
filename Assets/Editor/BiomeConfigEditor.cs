using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BiomeConfig))]
public class BiomeConfigEditor : Editor
{
    GameObject lastGenerated;

    public override void OnInspectorGUI()
    {
        BiomeConfig config = (BiomeConfig)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚙️ Biome Configuration", EditorStyles.boldLabel);

        config.biomeName = EditorGUILayout.TextField("Biome Name", config.biomeName);
        config.mapColor = EditorGUILayout.ColorField("Map Color", config.mapColor);

        EditorGUILayout.Space();

        config.width = EditorGUILayout.IntField("Width", config.width);
        config.height = EditorGUILayout.IntField("Height", config.height);

        config.groundMaterial = (Material)EditorGUILayout.ObjectField("Ground Material", config.groundMaterial, typeof(Material), false);

        config.terrainScale = EditorGUILayout.FloatField("Terrain Scale", config.terrainScale);
        config.heightMultiplier = EditorGUILayout.FloatField("Height Multiplier", config.heightMultiplier);

        EditorGUILayout.Space();

        SerializedProperty envPrefabs = serializedObject.FindProperty("environmentPrefabs");
        EditorGUILayout.PropertyField(envPrefabs, true);
        config.environmentDensity = EditorGUILayout.Slider("Environment Density", config.environmentDensity, 0f, 1f);

        SerializedProperty resPrefabs = serializedObject.FindProperty("resourcePrefabs");
        EditorGUILayout.PropertyField(resPrefabs, true);
        config.resourceDensity = EditorGUILayout.Slider("Resource Density", config.resourceDensity, 0f, 1f);

        SerializedProperty questPrefabs = serializedObject.FindProperty("questPrefabs");
        EditorGUILayout.PropertyField(questPrefabs, true);
        config.questSpawnChance = EditorGUILayout.Slider("Quest Spawn Chance", config.questSpawnChance, 0f, 1f);

        EditorGUILayout.Space();

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

        serializedObject.ApplyModifiedProperties();
    }

    private GameObject GenerateBiome(BiomeConfig config)
    {
        GameObject biomeRoot = new GameObject(config.biomeName + "_Generated");
        Undo.RegisterCreatedObjectUndo(biomeRoot, "Generate Biome");

        int width = config.width;
        int height = config.height;

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        int[] triangles = new int[width * height * 6];

        for (int z = 0, i = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float y = Mathf.PerlinNoise(
                    x * config.terrainScale * 0.01f,
                    z * config.terrainScale * 0.01f
                ) * config.heightMultiplier;

                vertices[i] = new Vector3(x, y, z);
            }
        }

        for (int z = 0, vert = 0, tris = 0; z < height; z++, vert++)
        {
            for (int x = 0; x < width; x++, vert++, tris += 6)
            {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter mf = biomeRoot.AddComponent<MeshFilter>();
        MeshRenderer mr = biomeRoot.AddComponent<MeshRenderer>();

        mf.sharedMesh = mesh;

        if (config.groundMaterial != null)
            mr.sharedMaterial = config.groundMaterial;

        return biomeRoot;
    }
}
