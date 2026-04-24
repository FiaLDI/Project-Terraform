using FishNet;
using UnityEngine;

namespace Features.Multiplayer.SceneBinding
{
    public sealed class SceneBoundNetworkSpawner : MonoBehaviour
    {
        [Header("Binding")]
        [SerializeField] private SceneBoundViewBase boundView;

        [Header("Network")]
        [SerializeField] private SceneBoundNetworkControllerBase controllerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Vector3 spawnOffset;
        [SerializeField] private bool spawnOnStart = true;

        private SceneBoundNetworkControllerBase spawnedController;
        private bool ownsSpawnedController;

        private void Awake()
        {
            if (boundView == null)
                boundView = GetComponentInParent<SceneBoundViewBase>();
        }

        private void Start()
        {
            if (!spawnOnStart)
                return;

            SpawnController();
        }

        private void Update()
        {
            if (!spawnOnStart || spawnedController != null || !InstanceFinder.IsServerStarted)
                return;

            SpawnController();
        }

        public void SpawnController()
        {
            if (spawnedController != null || !InstanceFinder.IsServerStarted)
                return;

            if (boundView == null)
            {
                Debug.LogError($"[{name}] BoundView is missing.", this);
                return;
            }

            if (controllerPrefab == null)
            {
                Debug.LogError($"[{name}] Controller prefab is missing.", this);
                return;
            }

            if (SceneBoundRegistry.TryGetController(boundView.BoundKey, out var existingController))
            {
                spawnedController = existingController;
                ownsSpawnedController = false;
                Debug.Log($"[SceneBound] Reusing controller key={boundView.BoundKey} for view={boundView.name}", this);
                return;
            }

            Transform point = spawnPoint != null ? spawnPoint : transform;
            Vector3 spawnPosition = point.TransformPoint(spawnOffset);

            spawnedController = Instantiate(
                controllerPrefab,
                spawnPosition,
                point.rotation
            );

            spawnedController.InitBinding(boundView);
            InstanceFinder.ServerManager.Spawn(spawnedController.gameObject);
            ownsSpawnedController = true;

            Debug.Log(
                $"[SceneBound] Spawned controller prefab={controllerPrefab.name} key={boundView.BoundKey} for view={boundView.name}",
                this
            );
        }

        private void OnDestroy()
        {
            if (!InstanceFinder.IsServerStarted || !ownsSpawnedController)
                return;

            if (spawnedController != null && spawnedController.IsSpawned)
                InstanceFinder.ServerManager.Despawn(spawnedController.gameObject);
        }
    }
}
