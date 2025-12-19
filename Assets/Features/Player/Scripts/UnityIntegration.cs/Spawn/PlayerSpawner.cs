using UnityEngine;
using Features.Player.UnityIntegration;

public sealed class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        Debug.Log($"[PlayerSpawner] Start | id={GetInstanceID()}", this);
        var spawn = FindObjectOfType<ScenePlayerSpawnPoint>();
        if (spawn == null)
        {
            Debug.LogError("[PlayerSpawner] No spawn point");
            return;
        }

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
