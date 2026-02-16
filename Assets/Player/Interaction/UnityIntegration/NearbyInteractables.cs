using System.Collections.Generic;
using Features.Interaction.Domain;
using Features.Items.UnityIntegration;
using FishNet.Object; // Нужно только для доступа к типу NetworkObject, если он используется в полях
using UnityEngine;

namespace Features.Interaction.UnityIntegration
{
    /// <summary>
    /// Управляет списком предметов рядом с игроком.
    /// Инициализируется строго из NetworkPlayer.OnStartClient().
    /// </summary>
    public class NearbyInteractables : MonoBehaviour, INearbyInteractables
    {
        [Header("Tuning")]
        [SerializeField] private float maxDistance = 3.0f;
        [SerializeField] private float maxAngle = 45f;
        [SerializeField] private int cleanupInterval = 60;

        private readonly List<WorldItemNetwork> items = new();
        private int frameCounter = 0;
        
        // Флаг локального игрока. Устанавливается при инициализации.
        private bool isLocalPlayer = false;

        /* ================= INITIALIZATION ================= */

        /// <summary>
        /// ГЛАВНЫЙ МЕТОД ИНИЦИАЛИЗАЦИИ.
        /// Вызывать из NetworkPlayer.OnStartClient().
        /// </summary>
        /// <param name="isOwner">Является ли этот игрок локальным (IsOwner)</param>
        public void Initialize(bool isOwner)
        {
            isLocalPlayer = isOwner;

            // Включаем компонент ТОЛЬКО для локального игрока
            this.enabled = isOwner;

            if (isOwner)
            {
                Debug.Log($"[NearbyInteractables] Initialized for LOCAL player. Ready to scan items.", this);
            }
            else
            {
                Debug.Log($"[NearbyInteractables] Disabled for REMOTE player.", this);
                // Очищаем список на всякий случай, чтобы память не ел
                items.Clear();
            }
        }

        /* ================= PUBLIC API ================= */

        public WorldItemNetwork GetBestItem(UnityEngine.Camera cam)
        {
            // Двойная защита: если компонент выключен или не локальный игрок
            if (!this.enabled || !isLocalPlayer || cam == null)
                return null;

            // Периодическая очистка
            frameCounter++;
            if (frameCounter >= cleanupInterval)
            {
                CleanupDeadItems();
                frameCounter = 0;
            }

            WorldItemNetwork best = null;
            float bestScore = float.MaxValue;

            Vector3 camPos = cam.transform.position;
            Vector3 camForward = cam.transform.forward;

            foreach (var item in items)
            {
                if (item == null || !item.gameObject.activeSelf || !item.IsPickupAvailable)
                    continue;

                Vector3 toItem = item.transform.position - camPos;
                float distance = toItem.magnitude;
                
                if (distance > maxDistance) continue;

                float angle = Vector3.Angle(camForward, toItem);
                if (angle > maxAngle) continue;

                // Score: чем меньше, тем лучше. (Расстояние важнее угла)
                float score = distance + angle * 0.03f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = item;
                }
            }

            return best;
        }

        public void Register(WorldItemNetwork item)
        {
            // Принимаем предметы, только если мы локальный игрок и компонент активен
            if (!isLocalPlayer || !this.enabled) return;
            
            if (item != null && !items.Contains(item))
            {
                items.Add(item);
                // Debug.Log($"[Nearby] Registered: {item.name}", this); // Можно раскомментить для отладки
            }
        }

        public void Unregister(WorldItemNetwork item)
        {
            if (!isLocalPlayer || !this.enabled) return;

            if (item != null)
            {
                items.Remove(item);
            }
        }

        /* ================= PRIVATE HELPERS ================= */

        private void CleanupDeadItems()
        {
            items.RemoveAll(x => x == null || !x.gameObject.activeSelf);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }
#endif
    }
}
