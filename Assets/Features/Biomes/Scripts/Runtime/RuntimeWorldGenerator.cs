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

    void Start()
    {
        if (!Validate())
        {
            enabled = false;
            return;
        }

        manager = new ChunkManager(worldConfig);

        // первичная генерация чанков
        manager.UpdateChunks(Vector3.zero, loadDistance, unloadDistance);

        // ждём полную генерацию → спавним игрока
        StartCoroutine(SpawnSequence());
    }

    void Update()
    {
        if (worldReady && playerInstance != null)
        {
            manager.UpdateChunks(playerInstance.transform.position, loadDistance, unloadDistance);
        }
    }

    private bool Validate()
    {
        if (worldConfig == null)
        {
            Debug.LogError("❌ WorldConfig not assigned!");
            return false;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("❌ Player Prefab not assigned!");
            return false;
        }

        return true;
    }

    // ============================
    //   MAIN SPAWN SEQUENCE
    // ============================
    private IEnumerator SpawnSequence()
    {
        // ⏳ ждём пока появятся коллайдеры чанков
        yield return new WaitForSeconds(0.5f);

        Vector3 spawnPos = Vector3.zero;

        // точка для проверки поверхности
        Vector3 rayStart = new Vector3(0, spawnHeightCheck, 0);

        bool foundGround = false;

        // несколько попыток — на случай async генерации
        for (int i = 0; i < 20; i++)
        {
            if (Physics.Raycast(rayStart, Vector3.down, out var hit, spawnHeightCheck * 2f))
            {
                spawnPos = hit.point + Vector3.up * 2f;
                foundGround = true;
                break;
            }

            yield return new WaitForSeconds(0.25f);
        }

        if (!foundGround)
        {
            Debug.LogWarning("⚠ Could not find terrain. Using fallback Y=10.");
            spawnPos = new Vector3(0, 10, 0);
        }

        // ========== SPAWN PLAYER ==========
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"✅ Player spawned at {spawnPos}");

        // ========== SPAWN SYSTEMS ==========
        if (systemsPrefab != null)
        {
            SpawnSystemsInFront();
        }

        worldReady = true;
    }

    // ============================
    //    SYSTEMS PREFAB SPAWN
    // ============================
    private void SpawnSystemsInFront()
    {
        if (playerInstance == null || systemsPrefab == null)
            return;

        Vector3 forwardPos = playerInstance.transform.position +
                             playerInstance.transform.forward * spawnDistanceForward +
                             Vector3.up * spawnHeightCheck;

        if (Physics.Raycast(forwardPos, Vector3.down, out var hit, spawnHeightCheck * 2f))
        {
            systemsInstance = Instantiate(systemsPrefab, hit.point + Vector3.up * 1f, Quaternion.identity);
            systemsInstance.name = "GameSystems";
        }
    }
}
