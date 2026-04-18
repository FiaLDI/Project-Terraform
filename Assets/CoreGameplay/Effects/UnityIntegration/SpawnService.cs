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
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Spawn(
            string prefabId,
            float lifetime,
            bool useSourcePosition,
            EffectContext ctx)
        {
            if (!InstanceFinder.IsServer)
            {
                Debug.LogError("Spawn blocked: not server");
                return;
            }


            var prefab = registry.Get(prefabId);
            if (prefab == null)
                return;

            Vector3 pos = useSourcePosition
                ? ResolveSourcePosition(ctx.Source)
                : ctx.Origin;

            var go = Instantiate(prefab, pos, Quaternion.identity);

            if (go.TryGetComponent(out NetworkObject netObj))
            {
                InstanceFinder.ServerManager.Spawn(
                    netObj.gameObject,
                    ResolveOwner(ctx.Source)
                );
            }

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
                netObj.Despawn();
            }
        }

        private static Vector3 ResolveSourcePosition(IBuffSource source)
        {
            if (source is Component c)
                return c.transform.position;

            return Vector3.zero;
        }

        private static NetworkConnection ResolveOwner(IBuffSource source)
        {
            if (source is Component c &&
                c.TryGetComponent(out NetworkObject no))
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
                return;

            var go = Instantiate(prefab, position, rotation);

            if (go.TryGetComponent(out NetworkObject netObj))
            {
                InstanceFinder.ServerManager.Spawn(go, owner);

                if (lifetime > 0f)
                    StartCoroutine(DespawnAfter(netObj, lifetime));
            }
        }
    }
}
