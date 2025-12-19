using UnityEngine;
using System.Collections;
using Features.Player.UnityIntegration;

public sealed class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float retryDelay = 0.5f;

    private void Start()
    {
        Debug.Log($"[PlayerSpawner] Start | id={GetInstanceID()}", this);
        StartCoroutine(WaitForSpawnPoint());
    }

    private IEnumerator WaitForSpawnPoint()
    {
        ScenePlayerSpawnPoint spawn = null;

        while (spawn == null)
        {
            spawn = FindObjectOfType<ScenePlayerSpawnPoint>();

            if (spawn == null)
            {
                Debug.LogWarning("[PlayerSpawner] Spawn point not found, retrying...");
                yield return new WaitForSeconds(retryDelay);
            }
        }

        SpawnPlayer(spawn);
    }

    private void SpawnPlayer(ScenePlayerSpawnPoint spawn)
    {
        var player = Instantiate(
            playerPrefab,
            spawn.transform.position,
            spawn.transform.rotation
        );

        Bind(player);
    }

    private void Bind(GameObject player)
    {
        if (LocalPlayerController.I == null)
        {
            Debug.LogError("[PlayerSpawner] LocalPlayerController not found");
            return;
        }

        LocalPlayerController.I.Bind(player);
    }
}
