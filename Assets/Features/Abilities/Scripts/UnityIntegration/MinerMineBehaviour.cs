using System.Collections.Generic;
using FishNet.Object;
using Features.Buffs.Domain;
using Features.Effects.Application;
using Features.Effects.Domain;
using UnityEngine;
using System.Collections;

namespace Features.Abilities.UnityIntegration
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(SpawnedObjectContext))]
    public sealed class MinerMineBehaviour : NetworkBehaviour
    {
        private static readonly Dictionary<GameObject, LinkedList<MinerMineBehaviour>> MinesBySource = new();

        [Header("Explosion")]
        [SerializeField] private float damage = 40f;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private DamageType damageType = DamageType.Explosion;
        [SerializeField] private LayerMask targetMask;

        [Header("Ownership")]
        [SerializeField] private int maxOwnedMines = 5;

        [Header("Visuals")]
        [SerializeField] private RadiusCircleVisual radiusCircleVisual;

        [Header("Explosion Visuals")]
        [SerializeField] private GameObject explosionVfxPrefab;
        [SerializeField] private float explosionVfxLifetime = 4f;
        [SerializeField] private bool scaleExplosionVfxByRadius = true;
        [SerializeField] private float explosionVfxScaleMultiplier = 1f;

        private NetworkObject networkObject;
        private SpawnedObjectContext spawnedContext;
        private GameObject sourceObject;
        private IBuffSource source;
        private bool exploded;

        public static void DetonateOwnedMines(GameObject sourceObject)
        {
            if (sourceObject == null || !MinesBySource.TryGetValue(sourceObject, out var mines))
                return;

            var snapshot = new MinerMineBehaviour[mines.Count];
            mines.CopyTo(snapshot, 0);

            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i] != null && snapshot[i].IsServerInitialized)
                    snapshot[i].Explode();
            }
        }

        private void Awake()
        {
            networkObject = GetComponent<NetworkObject>();
            spawnedContext = GetComponent<SpawnedObjectContext>();

            var trigger = GetComponent<SphereCollider>();
            trigger.isTrigger = true;

            if (targetMask.value == 0)
                targetMask = LayerMask.GetMask("Enemy");

            if (radiusCircleVisual != null)
                radiusCircleVisual.SetRadius(explosionRadius, transform.position);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            sourceObject = spawnedContext != null ? spawnedContext.Source : null;
            source = sourceObject != null ? sourceObject.GetComponent<IBuffSource>() : null;

            RegisterOwnedMine();
        }

        public override void OnStopServer()
        {
            UnregisterOwnedMine();
            base.OnStopServer();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServerInitialized || exploded)
                return;

            if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
                return;

            var target = other.GetComponentInParent<IBuffTarget>();
            if (target == null || !target.IsReady)
                return;

            if (sourceObject != null &&
                target.Transform != null &&
                target.Transform.root.gameObject == sourceObject)
            {
                return;
            }

            Explode();
        }

        [Server]
        public void Explode()
        {
            if (exploded)
                return;

            exploded = true;

            EffectExecutor.Instance?.Execute(
                new EffectDefinition
                {
                    type = EffectType.DealDamage,
                    targetMode = TargetMode.Area,
                    radius = explosionRadius,
                    layerMask = targetMask,
                    value = damage,
                    damageType = damageType,
                    ownership = OwnershipFilter.DifferentOwner
                },
                new EffectContext(
                    source,
                    null,
                    transform.position,
                    Vector3.up
                )
            );

            PlayExplosionVfxRpc(transform.position, transform.rotation, explosionRadius);

            StartCoroutine(DespawnAfterVfxRpc());
        }

        [ObserversRpc]
        private void PlayExplosionVfxRpc(Vector3 position, Quaternion rotation, float radius)
        {
            if (explosionVfxPrefab == null)
                return;

            GameObject vfx = Instantiate(
                explosionVfxPrefab,
                position,
                rotation
            );

            if (scaleExplosionVfxByRadius)
            {
                float scale = radius * explosionVfxScaleMultiplier;
                vfx.transform.localScale = Vector3.one * scale;
            }

            Destroy(vfx, explosionVfxLifetime);
        }

        [Server]
        private IEnumerator DespawnAfterVfxRpc()
        {
            yield return null;

            DespawnSelf();
        }

        [Server]
        private void RegisterOwnedMine()
        {
            if (sourceObject == null)
                return;

            if (!MinesBySource.TryGetValue(sourceObject, out var mines))
            {
                mines = new LinkedList<MinerMineBehaviour>();
                MinesBySource[sourceObject] = mines;
            }

            mines.AddLast(this);

            while (mines.Count > maxOwnedMines)
            {
                var oldestNode = mines.First;
                var oldest = oldestNode?.Value;
                if (oldestNode == null || oldest == null)
                    break;

                mines.RemoveFirst();

                if (oldest == this)
                    continue;

                oldest.DespawnSelf();
            }
        }

        [Server]
        private void UnregisterOwnedMine()
        {
            if (sourceObject == null || !MinesBySource.TryGetValue(sourceObject, out var mines))
                return;

            mines.Remove(this);
            if (mines.Count == 0)
                MinesBySource.Remove(sourceObject);
        }

        [Server]
        private void DespawnSelf()
        {
            if (networkObject != null && networkObject.IsSpawned)
                networkObject.Despawn();
            else
                Destroy(gameObject);
        }
    }
}
