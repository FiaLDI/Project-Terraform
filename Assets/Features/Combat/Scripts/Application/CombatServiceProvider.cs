using UnityEngine;
using Features.Combat.Application;

namespace Features.Combat.Application
{
    /// <summary>
    /// Глобальный провайдер CombatService.
    /// Создаёт и хранит единственный экземпляр CombatService.
    /// </summary>
    public static class CombatServiceProvider
    {
        private static CombatService _service;

        /// <summary>
        /// Глобальный доступ к CombatService.
        /// </summary>
        public static CombatService Service
        {
            get
            {
                if (_service == null)
                    _service = new CombatService();

                return _service;
            }
        }

        /// <summary>
        /// Позволяет подменить сервис (тесты, bootstrap, DI).
        /// </summary>
        public static void SetService(CombatService service)
        {
            _service = service;
        }

        /// <summary>
        /// Очистка (например при смене сцены или перезапуске).
        /// </summary>
        public static void Reset()
        {
            _service = null;
        }
    }
}
