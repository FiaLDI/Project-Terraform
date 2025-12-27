using Features.Items.Data;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Resources.Application;
using Features.Resources.Data;
using Features.Resources.Domain;
using UnityEngine;
using System.Linq;

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

        // Для сети: считаем что нода мертва при здоровье <= 0
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

        // =========================
        //  VFX при полном разрушении
        // =========================
        public void OnDepletedVisual()
        {
            PlayDestroyVFX();   // VFX для всех клиентов

            // Только визуальное уничтожение объекта на сцене
            Destroy(gameObject, 0.5f); // небольшая задержка под VFX
        }

        // =========================
        //  Mining Interaction
        // =========================

        public void ApplyMining(float amount)
        {
            ApplyMining(amount, 1f);
        }

        public void ApplyMining(float amount, float toolMultiplier)
        {
            bool depleted = _mining.Mine(_model, amount, toolMultiplier);

            // Если не исчерпан — просто hit VFX
            if (!depleted)
                PlayHitVFX();

            // Если исчерпан — логика дропа и VFX обрабатывается снаружи (через ResourceNodeNetwork)
        }

        // =========================
        //  VFX
        // =========================

        private void PlayHitVFX()
        {
            if (config.hitEffect == null) 
                return;

            Instantiate(
                config.hitEffect,
                transform.position,
                Quaternion.identity
            );
        }

        private void PlayDestroyVFX()
        {
            if (config.destroyEffect == null) 
                return;

            Instantiate(
                config.destroyEffect,
                transform.position,
                Quaternion.identity
            );
        }

        // =========================
        //  Drops (для сервера)
        // =========================

        /// <summary>
        /// Раскатывает таблицу дропа и возвращает массив инстансов.
        /// Вызывать только на сервере (через ResourceNodeNetwork.ServerSpawnDrops).
        /// </summary>
        public ItemInstance[] RollDrops()
        {
            if (config.drops == null || config.drops.Length == 0)
                return new ItemInstance[0];

            var items = _drops.RollDrops(config.drops); // IEnumerable<Item>

            return items
                .Where(i => i != null)
                .Select(i => new ItemInstance(i, 1))   // ← создаём ItemInstance
                .ToArray();
        }

        // Если где-то понадобится локальный спавн (НЕ для сети)
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

            var inst = new ItemInstance(item, 1);

            var holder = go.GetComponent<ItemRuntimeHolder>()
                       ?? go.AddComponent<ItemRuntimeHolder>();
            holder.SetInstance(inst);
        }

        // =========================
        //  Health API для сети
        // =========================

        public float GetCurrentHealth()
        {
            return _model?.CurrentHp ?? 0f;
        }

        /// <summary>
        /// Обновление визуала здоровья на клиентах (цвет, хп-бар и т.п.).
        /// Вызывается из ResourceNodeNetwork.OnMined_Clients().
        /// </summary>
        public void SetHealthVisual(float health)
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                float healthPct = Mathf.Clamp01(health / config.maxHealth);
                renderer.material.color = Color.Lerp(Color.red, Color.green, healthPct);
            }
        }
    }
}
