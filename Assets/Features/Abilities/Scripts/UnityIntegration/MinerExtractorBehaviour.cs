using System.Collections.Generic;
using FishNet.Object;
using Features.Effects.Application;
using Features.Stats.Domain;
using UnityEngine;

namespace Features.Abilities.UnityIntegration
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(SpawnedObjectContext))]
    public sealed class MinerExtractorBehaviour : NetworkBehaviour
    {
        private readonly Collider[] hitBuffer = new Collider[64];

        [Header("Mining")]
        [SerializeField] private float mineRadius = 8f;
        [SerializeField] private float tickInterval = 1f;
        [SerializeField] private float baseMiningAmount = 12f;
        [SerializeField] private LayerMask resourceMask;

        private SpawnedObjectContext spawnedContext;
        private GameObject sourceObject;
        private float timer;

        private void Awake()
        {
            spawnedContext = GetComponent<SpawnedObjectContext>();

            if (resourceMask.value == 0)
                resourceMask = 1 << 6;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            sourceObject = spawnedContext != null ? spawnedContext.Source : null;
            timer = tickInterval;
        }

        private void Update()
        {
            if (!IsServerInitialized)
                return;

            timer -= Time.deltaTime;
            if (timer > 0f)
                return;

            timer = tickInterval;
            MineNearbyResources();
        }

        [Server]
        private void MineNearbyResources()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                mineRadius,
                hitBuffer,
                resourceMask,
                QueryTriggerInteraction.Collide
            );

            if (hitCount <= 0)
                return;

            float amount = baseMiningAmount * ResolveMiningMultiplier();
            var visited = new HashSet<ResourceNodeNetwork>();

            for (int i = 0; i < hitCount; i++)
            {
                var collider = hitBuffer[i];
                if (collider == null)
                    continue;

                var node = collider.GetComponentInParent<ResourceNodeNetwork>();
                if (node == null || !visited.Add(node))
                    continue;

                node.Mine_Server(amount, 1f);
            }
        }

        private float ResolveMiningMultiplier()
        {
            if (sourceObject == null)
                return 1f;

            var owner = sourceObject.GetComponent<IStatsOwner>();
            if (owner == null || !owner.IsReady || owner.Facade?.Mining == null)
                return 1f;

            return Mathf.Max(0.1f, owner.Facade.Mining.MiningPower);
        }
    }
}
