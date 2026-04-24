using Features.Player.UnityIntegration;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Biomes.UnityIntegration
{
    public sealed class WorldCheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnHeightOffset = 1.25f;

        private void OnTriggerEnter(Collider other)
        {
            var nm = InstanceFinder.NetworkManager;
            if (nm == null || !nm.IsServerStarted)
                return;

            var player = other.GetComponentInParent<NetworkPlayer>();
            if (player == null)
                return;

            var nob = player.GetComponent<NetworkObject>();
            if (nob == null || nob.Owner == null)
                return;

            Transform point = spawnPoint != null ? spawnPoint : transform;
            Vector3 position = point.position + Vector3.up * spawnHeightOffset;
            Quaternion rotation = Quaternion.Euler(0f, point.eulerAngles.y, 0f);

            PlayerSpawnRegistry.I?.SetPlayerSpawnPoint(nob.Owner.ClientId, position, rotation);
        }
    }
}
