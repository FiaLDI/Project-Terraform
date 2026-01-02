using UnityEngine;
using Features.Buffs.Application;
using FishNet.Object;

namespace Features.Buffs.Net
{
    /// <summary>
    /// Тонкий Network-адаптер над BuffSystem.
    /// BuffSystem сам полностью отвечает за синхронизацию баффов.
    /// Этот класс — только точка расширения (UI / логи / аналитика).
    /// </summary>
    [RequireComponent(typeof(BuffSystem))]
    public sealed class BuffSystemNetAdapter : NetworkBehaviour
    {
        private BuffSystem buffSystem;

        private void Awake()
        {
            buffSystem = GetComponent<BuffSystem>();

            if (buffSystem == null)
                Debug.LogError("[BuffSystemNetAdapter] BuffSystem not found!", this);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (buffSystem == null)
                return;

            buffSystem.OnBuffAdded += OnServerBuffAdded;
            buffSystem.OnBuffRemoved += OnServerBuffRemoved;
        }

        public override void OnStopServer()
        {
            if (buffSystem != null)
            {
                buffSystem.OnBuffAdded -= OnServerBuffAdded;
                buffSystem.OnBuffRemoved -= OnServerBuffRemoved;
            }

            base.OnStopServer();
        }

        // =====================================================
        // SERVER EVENTS (OPTIONAL)
        // =====================================================

        private void OnServerBuffAdded(BuffInstance inst)
        {
            // BuffSystem уже:
            // - применил баф
            // - добавил в ActiveBuffIds
            // - синхронизировал клиентам

            // Здесь ТОЛЬКО доп. логика при необходимости
            // (UI, аналитика, звук, achievements и т.д.)

            // Debug.Log($"[BuffNet] Buff added: {inst.Config.buffId}", this);
        }

        private void OnServerBuffRemoved(BuffInstance inst)
        {
            // Аналогично — только хуки

            // Debug.Log($"[BuffNet] Buff removed: {inst.Config.buffId}", this);
        }
    }
}
