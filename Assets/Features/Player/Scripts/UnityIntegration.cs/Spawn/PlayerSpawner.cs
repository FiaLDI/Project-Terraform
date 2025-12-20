using UnityEngine;
using System.Collections;
using Features.Player.UnityIntegration;

public sealed class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float retryDelay = 0.5f;

    private bool spawned;

    private void Start()
    {
        Debug.Log($"[PlayerSpawner] Start | id={GetInstanceID()}", this);
        StartCoroutine(WaitForSpawnPoint());
    }

    private IEnumerator WaitForSpawnPoint()
    {
        ScenePlayerSpawnPoint spawn = null;

        while (!spawned && spawn == null)
        {
            spawn = Object.FindAnyObjectByType<ScenePlayerSpawnPoint>();

            if (spawn == null)
            {
                Debug.LogWarning("[PlayerSpawner] Spawn point not found, retrying...");
                yield return new WaitForSeconds(retryDelay);
            }
        }

        if (spawned)
            yield break;

        SpawnPlayer(spawn);
    }

    private void SpawnPlayer(ScenePlayerSpawnPoint spawn)
    {
        if (spawned)
        {
            Debug.LogWarning("[PlayerSpawner] Player already spawned, ignoring");
            return;
        }

        spawned = true;

        Debug.Log("[PlayerSpawner] Spawning player", this);

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

        if (Features.Player.UI.PlayerUIRoot.I != null)
        {
            Features.Player.UI.PlayerUIRoot.I.Bind(player);
        }
        else
        {
            Debug.LogError("[PlayerSpawner] PlayerUIRoot not found");
        }
    }
}
