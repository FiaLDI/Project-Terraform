using System.Linq;
using Features.Items.Domain;
using Features.Resources.Application;
using Features.Resources.Data;
using Features.Resources.Domain;
using UnityEngine;

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

        public bool IsDepleted() => _model?.CurrentHp <= 0f;

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

        public void OnDepletedVisual()
        {
            Destroy(gameObject, 0.5f);
        }

        public void ApplyMining(float amount, float toolMultiplier)
        {
            bool depleted = _mining.Mine(_model, amount, toolMultiplier);
        }

        public ItemInstance[] RollDrops()
        {
            if (config.drops == null || config.drops.Length == 0)
                return new ItemInstance[0];

            var items = _drops.RollDrops(config.drops);

            return items
                .Where(i => i != null)
                .Select(i => new ItemInstance(i, 1)) 
                .ToArray();
        }

        public float GetCurrentHealth()
        {
            return _model?.CurrentHp ?? 0f;
        }

        public void SetHealthVisual(float health)
        {
            _ = health;
        }

    }
}
