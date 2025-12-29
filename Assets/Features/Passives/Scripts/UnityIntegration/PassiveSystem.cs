using UnityEngine;
using Features.Passives.Domain;
using Features.Passives.Application;

namespace Features.Passives.UnityIntegration
{
    [DefaultExecutionOrder(-50)]
    public class PassiveSystem : MonoBehaviour
    {
        [Header("Equipped Passives (debug only)")]
        public PassiveSO[] equipped;

        private PassiveService _service;

        private void Awake()
        {
            _service = new PassiveService(gameObject);
        }

        private void OnDisable()
        {
            // Очистка при выключении (например, смерть или выход)
            _service?.DeactivateAll();
        }

        // --- PUBLIC API ДЛЯ АДАПТЕРА ---

        /// <summary>
        /// ТОЛЬКО СЕРВЕР: Применяет пассивки с их эффектами (баффами и т.д.)
        /// </summary>
        public void SetPassivesLogic(PassiveSO[] passives)
        {
            if (_service == null) _service = new PassiveService(gameObject);

            // 1. Очищаем старые эффекты
            _service.DeactivateAll();

            // 2. Запоминаем список
            equipped = passives;

            // 3. Накладываем новые эффекты
            if (equipped != null && equipped.Length > 0)
            {
                _service.ActivateAll(equipped);
            }
        }

        /// <summary>
        /// ТОЛЬКО КЛИЕНТ: Просто обновляет список для UI, но НЕ накладывает эффекты.
        /// Эффекты придут через синхронизацию баффов/статов от сервера.
        /// </summary>
        public void SetPassivesVisuals(PassiveSO[] passives)
        {
            if (_service == null) _service = new PassiveService(gameObject);

            // На клиенте мы НЕ вызываем ActivateAll/DeactivateAll, 
            // чтобы не дублировать логику.
            // Просто обновляем массив, чтобы UI мог его прочитать.
            equipped = passives;

            // Если у тебя в будущем появятся чисто визуальные пассивки (particles),
            // здесь можно вызвать метод _service.ActivateVisualsOnly(passives)
        }

        // Оставим старый метод как "wrapper" для локального тестирования (если нужно)
        // Но в сетевой игре используй методы выше через Adapter.
        public void SetPassives(PassiveSO[] passives)
        {
            SetPassivesLogic(passives);
        }
    }
}
