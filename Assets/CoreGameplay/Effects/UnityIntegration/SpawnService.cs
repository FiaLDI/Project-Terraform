using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using Features.Effects.Domain;
using Features.Buffs.Domain;
using Features.Stats.UnityIntegration;

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
            SpawnPositionMode spawnPositionMode,
            float forwardDistance,
            LayerMask surfaceMask,
            float heightOffset,
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

            Vector3 pos = ResolveSpawnPosition(
                effectContext,
                useSourcePosition,
                spawnPositionMode,
                forwardDistance,
                surfaceMask,
                heightOffset
            );
            Quaternion rotation = ResolveSpawnRotation(effectContext, spawnPositionMode);

            var go = Instantiate(prefab, pos, rotation);

            var spawnedContext = go.GetComponent<SpawnedObjectContext>();
            if (spawnedContext == null)
                spawnedContext = go.AddComponent<SpawnedObjectContext>();

            spawnedContext.Source = ResolveSourceGameObject(effectContext.Source);
            spawnedContext.Target = ResolveFirstTargetGameObject(effectContext.Targets);
            spawnedContext.Lifetime = lifetime;

            var turretStats = go.GetComponent<TurretStats>();
            if (turretStats != null && effectContext.Source != null)
                turretStats.InitOwner(effectContext.Source);

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

        private static Vector3 ResolveSpawnPosition(
            EffectContext effectContext,
            bool useSourcePosition,
            SpawnPositionMode spawnPositionMode,
            float forwardDistance,
            LayerMask surfaceMask,
            float heightOffset)
        {
            return spawnPositionMode switch
            {
                SpawnPositionMode.Source =>
                    ResolveSourcePosition(effectContext.Source) + Vector3.up * heightOffset,
                SpawnPositionMode.TargetPoint =>
                    effectContext.Origin + Vector3.up * heightOffset,
                SpawnPositionMode.SourceForwardGrounded =>
                    ResolveForwardGroundedPosition(
                        effectContext,
                        forwardDistance,
                        surfaceMask,
                        heightOffset
                    ),
                _ => useSourcePosition
                    ? ResolveSourcePosition(effectContext.Source) + Vector3.up * heightOffset
                    : effectContext.Origin + Vector3.up * heightOffset
            };
        }

        private static Quaternion ResolveSpawnRotation(
            EffectContext effectContext,
            SpawnPositionMode spawnPositionMode)
        {
            if (spawnPositionMode != SpawnPositionMode.SourceForwardGrounded)
                return Quaternion.identity;

            Vector3 forward = effectContext.Direction;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.0001f)
                return Quaternion.identity;

            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private static Vector3 ResolveForwardGroundedPosition(
            EffectContext effectContext,
            float forwardDistance,
            LayerMask surfaceMask,
            float heightOffset)
        {
            var sourceTransform = ResolveSourceTransform(effectContext.Source);
            Vector3 sourcePosition = sourceTransform != null
                ? sourceTransform.position
                : ResolveSourcePosition(effectContext.Source);

            Vector3 planarDirection = effectContext.Direction;
            planarDirection.y = 0f;

            if (planarDirection.sqrMagnitude < 0.0001f && sourceTransform != null)
            {
                planarDirection = sourceTransform.forward;
                planarDirection.y = 0f;
            }

            if (planarDirection.sqrMagnitude < 0.0001f)
                planarDirection = Vector3.forward;

            planarDirection.Normalize();

            float distance = forwardDistance > 0.01f ? forwardDistance : 3f;
            Vector3 intendedPosition = sourcePosition + planarDirection * distance;

            int raycastMask = surfaceMask.value != 0
                ? surfaceMask.value
                : Physics.DefaultRaycastLayers;

            Vector3 rayOrigin = intendedPosition + Vector3.up * 10f;
            if (Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                30f,
                raycastMask,
                QueryTriggerInteraction.Ignore))
            {
                return hit.point + Vector3.up * heightOffset;
            }

            return intendedPosition + Vector3.up * heightOffset;
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

        private static Transform ResolveSourceTransform(IBuffSource source)
        {
            if (source is Component c)
                return c.transform;

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
