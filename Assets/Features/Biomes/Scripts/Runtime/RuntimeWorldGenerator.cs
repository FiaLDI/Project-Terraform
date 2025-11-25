using UnityEngine;
using System.Collections;

public class RuntimeWorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    public WorldConfig worldConfig;

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Systems Prefab")]
    public GameObject systemsPrefab;

    [Header("Chunk Streaming")]
    public int loadDistance = 5;
    public int unloadDistance = 8;

    [Header("Spawn Settings")]
    public float spawnDistanceForward = 5f;
    public float spawnHeightCheck = 500f;

    private ChunkManager manager;
    private GameObject playerInstance;
    private GameObject systemsInstance;

    private bool worldReady = false;

    public static GameObject PlayerInstance { get; private set; }
    public static WorldConfig World { get; private set; }

    void Start()
    {
        manager = new ChunkManager(worldConfig);
        World = worldConfig;

        manager.UpdateChunks(Vector3.zero, loadDistance, unloadDistance);

        StartCoroutine(SpawnSequence());
    }

    void Update()
    {
        if (worldReady && playerInstance != null)
            manager.UpdateChunks(playerInstance.transform.position, loadDistance, unloadDistance);
    }

    private IEnumerator SpawnSequence()
    {
        yield return new WaitForSeconds(0.5f);

        Vector3 spawnPos = new Vector3(0, 2000, 0);
        if (Physics.Raycast(spawnPos, Vector3.down, out var hit, 5000f))
            spawnPos = hit.point + Vector3.up * 2f;

        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        PlayerInstance = playerInstance;

        if (systemsPrefab != null)
            SpawnSystemsInFront();

        worldReady = true;
    }

    private void SpawnSystemsInFront()
    {
        Vector3 forwardPos = playerInstance.transform.position +
                             playerInstance.transform.forward * spawnDistanceForward +
                             Vector3.up * 2000f;

        if (Physics.Raycast(forwardPos, Vector3.down, out var hit, 5000f))
        {
            systemsInstance = Instantiate(systemsPrefab, hit.point + Vector3.up * 1f, Quaternion.identity);
            systemsInstance.name = "GameSystems";
        }
    }
}
