using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections.Generic;
using Features.Passives.Domain;
using Features.Passives.UnityIntegration;
using Features.Passives.Data; // Наш Registry

namespace Features.Passives.Net
{
    public class PassiveNetAdapter : NetworkBehaviour
    {
        private PassiveSystem _system;
        
        // Синхронизируем только ID (строки)
        public readonly SyncList<string> EquippedIds = new SyncList<string>();

        private void Awake()
        {
            _system = GetComponent<PassiveSystem>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            EquippedIds.OnChange += OnIdsChanged;
            
            // Инициализация при входе (если уже есть данные)
            if (EquippedIds.Count > 0)
                UpdateClientVisuals();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            EquippedIds.OnChange -= OnIdsChanged;
        }

        private void OnIdsChanged(SyncListOperation op, int index, string oldItem, string newItem, bool asServer)
        {
            if (asServer) return; // Сервер сам знает, что он поменял
            UpdateClientVisuals();
        }

        private void UpdateClientVisuals()
        {
            var list = new List<PassiveSO>();
            
            foreach (var id in EquippedIds)
            {
                var so = PassiveRegistrySO.Instance.GetById(id);
                if (so != null) list.Add(so);
                else Debug.LogWarning($"[PassiveNet] Unknown passive ID: {id}");
            }

            // Обновляем ТОЛЬКО визуал (список для UI), без логики
            _system.SetPassivesVisuals(list.ToArray());
        }

        // --- SERVER API ---

        public void ServerSetPassives(PassiveSO[] passives)
        {
            if (!IsServer) return;
            Debug.Log("[PASSIVES] ServerSetPassives()", this);


            // 1. Применяем логику (баффы и т.д.)
            _system.SetPassivesLogic(passives);

            // 2. Обновляем сеть
            EquippedIds.Clear();
            if (passives != null)
            {
                foreach (var p in passives)
                {
                    if (p == null) continue;
                    
                    var id = PassiveRegistrySO.Instance.GetId(p);
                    if (!string.IsNullOrEmpty(id))
                    {
                        EquippedIds.Add(id);
                    }
                    else
                    {
                        Debug.LogError($"[PassiveNet] Passive {p.name} not found in Registry! Cannot sync.");
                    }
                }
            }
        }
    }
}
