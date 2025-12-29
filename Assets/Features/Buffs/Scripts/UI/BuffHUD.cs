using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;
using Features.UI;

namespace Features.Buffs.UI
{
    public class BuffHUD : PlayerBoundUIView
    {
        [Header("UI")]
        [SerializeField] private Transform container;
        [SerializeField] private BuffIconUI iconPrefab;

        private BuffSystem buffSystem;
        private readonly Dictionary<BuffInstance, BuffIconUI> icons = new();

        private Coroutine waitCoroutine;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Awake()
        {
            // Проверка жизни UI
            Debug.Log($"[BuffHUD] AWAKE on {gameObject.name}", this);
        }

        // ======================================================
        // PLAYER BIND (FROM BASE)
        // ======================================================

        protected override void OnPlayerBound(GameObject player)
        {
            Debug.Log("[BuffHUD] OnPlayerBound called for " + player.name, this);

            // 1. Ищем BuffSystem ГЛУБОКО (на случай если он в дочерних объектах)
            var bs = player.GetComponentInChildren<BuffSystem>(includeInactive: true);

            if (bs == null)
            {
                Debug.LogError($"[BuffHUD] BuffSystem NOT FOUND on player {player.name} (checked children)!", this);
                return;
            }

            Debug.Log($"[BuffHUD] Found BuffSystem. ServiceReady={bs.ServiceReady}", this);

            // 2. Если система не готова - ждём (как в AbilityHUD)
            if (!bs.ServiceReady)
            {
                Debug.Log("[BuffHUD] BuffSystem found but NOT READY. Waiting...", this);
                
                // Если ты добавил event OnSystemReady в BuffSystem - лучше использовать его!
                // bs.OnSystemReady += HandleSystemReady;
                
                // Если нет - используем корутину (fallback)
                if (waitCoroutine != null) StopCoroutine(waitCoroutine);
                waitCoroutine = StartCoroutine(WaitForBuffSystem(bs));
                return;
            }

            // 3. Если готова - биндимся сразу
            Bind(bs);
        }

        protected override void OnPlayerUnbound(GameObject player)
        {
            Debug.Log("[BuffHUD] OnPlayerUnbound called", this);
            
            // Если была подписка на event - отписываемся здесь
            // if (buffSystem != null) buffSystem.OnSystemReady -= HandleSystemReady;
            
            Unbind();
        }

        // ======================================================
        // WAIT FOR READY (Fallback logic)
        // ======================================================

        private System.Collections.IEnumerator WaitForBuffSystem(BuffSystem bs)
        {
            int maxWait = 200; // Ждем до 2-3 секунд (при 60 fps)
            int waited = 0;

            // Ждем пока ServiceReady станет true
            while (bs != null && !bs.ServiceReady && waited < maxWait)
            {
                yield return null; 
                waited++;
            }

            waitCoroutine = null;

            if (bs == null)
            {
                Debug.LogWarning("[BuffHUD] BuffSystem destroyed while waiting", this);
                yield break;
            }

            if (!bs.ServiceReady)
            {
                Debug.LogError($"[BuffHUD] BuffSystem timed out after {maxWait} frames!", this);
                yield break;
            }

            Debug.Log($"[BuffHUD] BuffSystem became ready after {waited} frames. Binding...", this);
            Bind(bs);
        }

        // ======================================================
        // BIND / UNBIND
        // ======================================================

        private void Bind(BuffSystem bs)
        {
            Debug.Log("[BuffHUD] Binding to BuffSystem instance", this);

            buffSystem = bs;

            // Подписываемся на изменения
            buffSystem.OnBuffAdded += HandleAdd;
            buffSystem.OnBuffRemoved += HandleRemove;

            // Отображаем существующие бафы (Синхронизация стейта)
            var activeBuffs = buffSystem.Active; // Используем свойство
            int count = activeBuffs?.Count ?? 0;
            
            Debug.Log($"[BuffHUD] Subscribed. Current active buffs: {count}", this);

            if (count > 0)
            {
                foreach (var inst in activeBuffs)
                {
                    // Пропускаем дубликаты (на случай гонки)
                    if (!icons.ContainsKey(inst))
                    {
                        HandleAdd(inst);
                    }
                }
            }
        }

        private void Unbind()
        {
            if (waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }

            if (buffSystem != null)
            {
                buffSystem.OnBuffAdded -= HandleAdd;
                buffSystem.OnBuffRemoved -= HandleRemove;
                Debug.Log("[BuffHUD] Unbound from BuffSystem", this);
            }

            buffSystem = null;
            ClearAll();
        }

        // ======================================================
        // ADD / REMOVE HANDLERS
        // ======================================================

        private void HandleAdd(BuffInstance inst)
        {
            if (inst == null || inst.Config == null) return;

            if (icons.ContainsKey(inst)) return; // Уже есть

            if (container == null || iconPrefab == null)
            {
                Debug.LogError("[BuffHUD] References missing (Container or Prefab)!", this);
                return;
            }

            // Создаем иконку
            var ui = Instantiate(iconPrefab, container);
            ui.name = $"BuffIcon_{inst.Config.buffId}";
            ui.Bind(inst);

            // Настраиваем тултип (опционально)
            var tt = ui.GetComponent<BuffTooltipTrigger>();
            if (tt != null) tt.Bind(inst);

            icons[inst] = ui;
            Resort();
            
            Debug.Log($"[BuffHUD] Added icon: {inst.Config.buffId}", this);
        }

        private void HandleRemove(BuffInstance inst)
        {
            if (inst == null) return;

            if (icons.TryGetValue(inst, out var ui))
            {
                if (ui != null) Destroy(ui.gameObject);
                icons.Remove(inst);
                Resort();
                Debug.Log($"[BuffHUD] Removed icon: {inst.Config.buffId}", this);
            }
        }

        // ======================================================
        // HELPERS
        // ======================================================

        private void Resort()
        {
            if (icons.Count <= 1) return;

            var sorted = icons
                .OrderBy(kv => kv.Key.Config.isDebuff ? 0 : 1) // Дебаффы (красные) слева
                .ThenByDescending(kv => kv.Key.Remaining)       // Долгие слева
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].Value.transform.SetSiblingIndex(i);
            }
        }

        private void ClearAll()
        {
            foreach (var ui in icons.Values)
            {
                if (ui != null) Destroy(ui.gameObject);
            }
            icons.Clear();
        }
    }
}
