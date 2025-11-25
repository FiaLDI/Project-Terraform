using UnityEngine;
using UnityEditor;

public class BiomeWorldWindow : EditorWindow
{
    private WorldConfig worldConfig;
    private Transform player;
    private ChunkManager runtimeManager;
    private GameObject systemsPrefab;

    private int loadDist = 5;
    private int unloadDist = 8;
    private bool autoUpdate = true;

    [MenuItem("Orbis/Biomes/World Generator")]
    public static void ShowWindow()
    {
        GetWindow<BiomeWorldWindow>("Biome World");
    }

    private void OnGUI()
    {
        GUILayout.Label("Biome World Generator", EditorStyles.boldLabel);
        GUILayout.Space(8);

        worldConfig = (WorldConfig)EditorGUILayout.ObjectField(
            "World Config",
            worldConfig,
            typeof(WorldConfig),
            false
        );

        player = (Transform)EditorGUILayout.ObjectField(
            "Player / Camera",
            player,
            typeof(Transform),
            true
        );

        systemsPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Systems Prefab",
            systemsPrefab,
            typeof(GameObject),
            false
        );

        if (worldConfig != null)
            EditorGUILayout.LabelField($"Chunk Size: {worldConfig.chunkSize}");

        loadDist = EditorGUILayout.IntSlider("Load Distance", loadDist, 1, 20);
        unloadDist = EditorGUILayout.IntSlider("Unload Distance", unloadDist, loadDist, 30);

        autoUpdate = EditorGUILayout.Toggle("Auto Update", autoUpdate);

        GUILayout.Space(10);

        if (GUILayout.Button("▶ Generate World", GUILayout.Height(28)))
            GenerateWorld();

        if (runtimeManager != null)
        {
            if (GUILayout.Button("⟳ Refresh"))
                ForceUpdate();

            if (GUILayout.Button("❌ Clear World"))
                ClearWorld();
        }
    }

    private void Update()
    {
        TerrainMeshGenerator.ExecutePendingUnityThreadTasks();

        if (autoUpdate && runtimeManager != null && player != null)
        {
            runtimeManager.UpdateChunks(player.position, loadDist, unloadDist);
            SceneView.RepaintAll();
        }
    }

    private void GenerateWorld()
    {
        ClearWorld();

        if (worldConfig == null)
        {
            Debug.LogError("❌ Назначи WorldConfig!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("❌ Назначи Player / Camera!");
            return;
        }

        runtimeManager = new ChunkManager(worldConfig);
        ForceUpdate();

        SpawnSystemsPrefabInFrontOfPlayer();
    }

    private void ForceUpdate()
    {
        if (runtimeManager == null || player == null)
            return;

        runtimeManager.UpdateChunks(player.position, loadDist, unloadDist);
        SceneView.RepaintAll();
    }

    private void ClearWorld()
    {
        foreach (var m in Object.FindObjectsByType<ChunkRootMarker>(FindObjectsSortMode.None))
            Object.DestroyImmediate(m.gameObject);

        GameObject oldSystems = GameObject.Find("GameSystems");
        if (oldSystems != null)
            Object.DestroyImmediate(oldSystems);

        runtimeManager = null;
        SceneView.RepaintAll();
    }

    /// <summary>
    /// Спавнит Systems Prefab строго перед игроком, по поверхности.
    /// </summary>
    private void SpawnSystemsPrefabInFrontOfPlayer()
    {
        if (systemsPrefab == null)
        {
            Debug.LogWarning("⚠ Systems Prefab not assigned.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("❌ Player not assigned!");
            return;
        }

        // точка перед игроком
        Vector3 forwardPos = player.position + player.forward * 5f + Vector3.up * 500f;

        if (Physics.Raycast(forwardPos, Vector3.down, out RaycastHit hit, 2000f))
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(systemsPrefab);
            obj.transform.position = hit.point + Vector3.up * 1f;
            obj.name = "GameSystems";

            Debug.Log("⚙ GameSystems placed in front of the player on terrain.");
        }
        else
        {
            Debug.LogWarning("⚠ Could not find ground to place SystemsPrefab!");
        }
    }
}
