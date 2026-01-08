using Features.Abilities.Domain;
using FishNet.Object;
using UnityEngine;

namespace Features.Abilities.UnityIntegration
{
    public abstract class AbilityRuntimeBase : NetworkBehaviour
    {
        protected AbilityContext Context { get; private set; }

        private bool _initialized;

        /// <summary>
        /// Вызывается ТОЛЬКО сервером до Spawn
        /// </summary>
        public void Initialize(AbilityContext ctx)
        {
            if (_initialized)
                return;

            Context = ctx;
            _initialized = true;
        }

        // =====================================================
        // SERVER
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (!_initialized)
            {
                Debug.LogError(
                    $"[{name}] AbilityRuntime started without Initialize()",
                    this
                );
                return;
            }

            OnServerStarted();
        }

        /// <summary>
        /// Серверная логика способности (damage, aura, channel)
        /// </summary>
        protected abstract void OnServerStarted();

        /// <summary>
        /// Для Instant / Channel — вызывается один раз
        /// </summary>
        public virtual void ExecuteServer() { }

        // =====================================================
        // CLIENT
        // =====================================================

        public override void OnStartClient()
        {
            base.OnStartClient();
            OnClientStarted();
        }

        /// <summary>
        /// ТОЛЬКО ВИЗУАЛ (VFX, SFX)
        /// </summary>
        protected virtual void OnClientStarted() { }
    }
}
