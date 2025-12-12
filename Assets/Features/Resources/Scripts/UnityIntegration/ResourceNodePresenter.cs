using UnityEngine;
using Features.Resources.Domain;
using Features.Resources.Application;
using Features.Resources.Data;
using Features.Items.Data;

namespace Features.Resources.UnityIntegration
{
    [RequireComponent(typeof(Collider))]
    public class ResourceNodePresenter : MonoBehaviour
    {
        [Header("Resource Config")]
        public ResourceSO config;

        private ResourceNodeModel _model;
        private MiningService _mining;
        private ResourceDropService _drops;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("[ResourceNodePresenter] Config is NULL!", this);
                enabled = false;
                return;
            }

            _model  = new ResourceNodeModel(config.maxHealth);
            _mining = new MiningService();
            _drops  = new ResourceDropService();
        }

        // -------------------------
        //   Mining Interaction
        // -------------------------
        public void ApplyMining(float amount)
        {
            ApplyMining(amount, 1f);
        }

        public void ApplyMining(float amount, float toolMultiplier)
        {
            bool depleted = _mining.Mine(_model, amount, toolMultiplier);

            if (depleted)
                OnDepleted();
            else
                PlayHitVFX();
        }

        // -------------------------
        //   VFX
        // -------------------------
        private void PlayHitVFX()
        {
            if (config.hitEffect == null) return;

            Instantiate(
                config.hitEffect,
                transform.position,
                Quaternion.identity
            );
        }

        private void PlayDestroyVFX()
        {
            if (config.destroyEffect == null) return;

            Instantiate(
                config.destroyEffect,
                transform.position,
                Quaternion.identity
            );
        }

        // -------------------------
        //   On Depletion (Drops + VFX)
        // -------------------------
        private void OnDepleted()
        {
            PlayDestroyVFX();

            if (config.drops != null && config.drops.Length > 0)
            {
                var items = _drops.RollDrops(config.drops);
                foreach (var item in items)
                    SpawnItem(item);
            }

            Destroy(gameObject);
        }

        // -------------------------
        //   Spawning Dropped Items
        // -------------------------
        private void SpawnItem(Item item)
        {
            if (item == null || item.worldPrefab == null)
            {
                Debug.LogWarning($"[ResourceNodePresenter] Cannot spawn item: {item?.name}");
                return;
            }

            var go = Instantiate(
                item.worldPrefab,
                transform.position + Vector3.up,
                Random.rotation
            );

            if (go.TryGetComponent<NearbyItemPresenter>(out var presenter))
            {
                presenter.Initialize(item, 1);
            }
        }

    }
}
