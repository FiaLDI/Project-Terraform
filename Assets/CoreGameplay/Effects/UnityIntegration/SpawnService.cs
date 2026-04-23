using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using Features.Effects.Domain;
using Features.Buffs.Domain;

namespace Features.Effects.Application
{
    public sealed class SpawnService : MonoBehaviour
    {
        public static SpawnService Instance { get; private set; }

        [SerializeField] private SpawnPrefabRegistry registry;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Spawn(
            string prefabId,
            float lifetime,
            bool useSourcePosition,
            EffectContext effectContext)
        {
            if (!InstanceFinder.IsServer)
            {
                Debug.LogError("Spawn blocked: not server");
                return;
            }

            var prefab = registry.Get(prefabId);
            if (prefab == null)
            {
                Debug.LogError($"Spawn failed: prefab '{prefabId}' not found");
                return;
            }

            Vector3 pos = useSourcePosition
                ? ResolveSourcePosition(effectContext.Source)
                : effectContext.Origin;

            var go = Instantiate(prefab, pos, Quaternion.identity);

            var spawnedContext = go.GetComponent<SpawnedObjectContext>();
            if (spawnedContext == null)
                spawnedContext = go.AddComponent<SpawnedObjectContext>();

            spawnedContext.Source = ResolveSourceGameObject(effectContext.Source);
            spawnedContext.Target = ResolveFirstTargetGameObject(effectContext.Targets);
            spawnedContext.Lifetime = lifetime;

            if (!go.TryGetComponent(out NetworkObject netObj))
            {
                Debug.LogError($"Spawn failed: prefab '{prefabId}' has no NetworkObject");
                Destroy(go);
                return;
            }

            InstanceFinder.ServerManager.Spawn(
                netObj.gameObject,
                ResolveOwner(effectContext.Source)
            );

            if (lifetime > 0f)
            {
                StartCoroutine(DespawnAfter(netObj, lifetime));
            }
        }

        private System.Collections.IEnumerator DespawnAfter(NetworkObject netObj, float time)
        {
            yield return new WaitForSeconds(time);

            if (netObj != null && netObj.IsSpawned)
            {
                InstanceFinder.ServerManager.Despawn(netObj);
            }
        }

        private static Vector3 ResolveSourcePosition(IBuffSource source)
        {
            if (source is Component c)
                return c.transform.position;

            return Vector3.zero;
        }

        private static GameObject ResolveSourceGameObject(IBuffSource source)
        {
            if (source is Component c)
                return c.gameObject;

            return null;
        }

        private static GameObject ResolveFirstTargetGameObject(IBuffTarget[] targets)
        {
            if (targets == null || targets.Length == 0)
                return null;

            if (targets[0] is Component c)
                return c.gameObject;

            return null;
        }

        private static NetworkConnection ResolveOwner(IBuffSource source)
        {
            if (source is Component c && c.TryGetComponent(out NetworkObject no))
                return no.Owner;

            return null;
        }

        public void SpawnAtPosition(
            string prefabId,
            Vector3 position,
            Quaternion rotation,
            float lifetime,
            NetworkConnection owner = null)
        {
            if (!InstanceFinder.IsServer)
            {
                Debug.LogError("Spawn blocked: not server");
                return;
            }

            var prefab = registry.Get(prefabId);
            if (prefab == null)
            {
                Debug.LogError($"Spawn failed: prefab '{prefabId}' not found");
                return;
            }

            var go = Instantiate(prefab, position, rotation);

            if (!go.TryGetComponent(out NetworkObject netObj))
            {
                Debug.LogError($"Spawn failed: prefab '{prefabId}' has no NetworkObject");
                Destroy(go);
                return;
            }

            InstanceFinder.ServerManager.Spawn(netObj.gameObject, owner);

            if (lifetime > 0f)
            {
                StartCoroutine(DespawnAfter(netObj, lifetime));
            }
        }
    }
}
