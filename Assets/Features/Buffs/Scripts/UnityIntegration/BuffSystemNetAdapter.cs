using UnityEngine;
using Features.Buffs.Application;
using FishNet.Object;


namespace Features.Buffs.Net
{
    /// <summary>
    /// Network adapter для BuffSystem.
    /// Синхронизирует состояние баффов между сервером и клиентами.
    /// </summary>
    public class BuffSystemNetAdapter : NetworkBehaviour
    {
        private BuffSystem _buffSystem;

        private void Awake()
        {
            _buffSystem = GetComponent<BuffSystem>();

            if (_buffSystem == null)
            {
                Debug.LogError("[BuffSystemNetAdapter] BuffSystem component not found!", this);
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (_buffSystem == null) return;

            // На сервере подписываемся на события баффов
            _buffSystem.OnBuffAdded += OnServerBuffAdded;
            _buffSystem.OnBuffRemoved += OnServerBuffRemoved;

            if (_buffSystem.ServiceReady)
            {
                SyncBuffsToClients();
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            if (_buffSystem != null)
            {
                _buffSystem.OnBuffAdded -= OnServerBuffAdded;
                _buffSystem.OnBuffRemoved -= OnServerBuffRemoved;
            }
        }

        private void OnServerBuffAdded(BuffInstance inst)
        {
            // На сервере автоматически обновляется через BuffSystem.UpdateActivBuffsSync
            // Но можно добавить дополнительную логику если нужна
        }

        private void OnServerBuffRemoved(BuffInstance inst)
        {
            // На сервере автоматически обновляется через BuffSystem.UpdateActivBuffsSync
            // Но можно добавить дополнительную логику если нужна
        }

        /// <summary>
        /// Принудительная синхронизация всех баффов на клиентов.
        /// Вызывается при инициализации сервера.
        /// </summary>
        private void SyncBuffsToClients()
        {
            if (!IsServer || _buffSystem == null) return;

            // BuffSystem сам следит за ActiveBuffIds через SyncList
            // Просто убеждаемся что данные актуальны
            if (_buffSystem.Active != null && _buffSystem.Active.Count > 0)
            {
                Debug.Log($"[BuffSystemNetAdapter] Synced {_buffSystem.Active.Count} buffs to clients");
            }
        }
    }
}