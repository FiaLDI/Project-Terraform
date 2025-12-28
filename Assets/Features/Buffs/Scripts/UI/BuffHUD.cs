// BuffHUD.cs - ИСПРАВЛЕННЫЙ

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
        // PLAYER BIND (FROM BASE)
        // ======================================================

        protected override void OnPlayerBound(GameObject player)
        {
            Debug.Log("[BuffHUD] OnPlayerBound called for " + player.name, this);

            var bs = player.GetComponent<BuffSystem>();
            if (bs == null)
            {
                Debug.LogError("[BuffHUD] BuffSystem not found on player!", this);
                return;
            }

            Debug.Log($"[BuffHUD] Found BuffSystem, ServiceReady={bs.ServiceReady}", this);

            // 🟢 ИСПРАВЛЕНИЕ: ждём инициализацию BuffSystem
            if (!bs.ServiceReady)
            {
                Debug.Log("[BuffHUD] BuffSystem not ready, waiting...", this);
                waitCoroutine = StartCoroutine(WaitForBuffSystem(bs));
                return;
            }

            Bind(bs);
        }

        protected override void OnPlayerUnbound(GameObject player)
        {
            Debug.Log("[BuffHUD] OnPlayerUnbound called", this);
            Unbind();
        }

        // ======================================================
        // WAIT FOR READY
        // ======================================================

        private System.Collections.IEnumerator WaitForBuffSystem(BuffSystem bs)
        {
            int maxWait = 100; // максимум 10 кадров
            int waited = 0;

            while (bs != null && !bs.ServiceReady && waited < maxWait)
            {
                Debug.Log($"[BuffHUD] Waiting for BuffSystem... ({waited})", this);
                yield return null;
                waited++;
            }

            waitCoroutine = null;

            if (bs == null)
            {
                Debug.LogError("[BuffHUD] BuffSystem was destroyed while waiting", this);
                yield break;
            }

            if (!bs.ServiceReady)
            {
                Debug.LogError("[BuffHUD] BuffSystem still not ready after waiting!", this);
                yield break;
            }

            Debug.Log("[BuffHUD] BuffSystem is ready, binding...", this);
            Bind(bs);
        }

        // ======================================================
        // BIND / UNBIND
        // ======================================================

        private void Bind(BuffSystem bs)
        {
            Debug.Log("[BuffHUD] Binding to BuffSystem", this);

            buffSystem = bs;

            // 🟢 ВАЖНО: подписываемся на события
            buffSystem.OnBuffAdded += HandleAdd;
            buffSystem.OnBuffRemoved += HandleRemove;

            Debug.Log($"[BuffHUD] Subscribed. Current buffs count: {buffSystem.Active?.Count ?? 0}", this);

            // 🟢 ИСПРАВЛЕНИЕ: отобразить уже активные бафы
            if (buffSystem.Active != null && buffSystem.Active.Count > 0)
            {
                Debug.Log($"[BuffHUD] Found {buffSystem.Active.Count} existing buffs, adding icons...", this);
                
                foreach (var inst in buffSystem.Active)
                {
                    Debug.Log($"[BuffHUD] Adding existing buff icon: {inst.Config?.buffId}", this);
                    HandleAdd(inst);
                }
            }
            else
            {
                Debug.Log("[BuffHUD] No existing buffs found", this);
            }
        }

        private void Unbind()
        {
            Debug.Log("[BuffHUD] Unbinding", this);

            if (waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }

            if (buffSystem != null)
            {
                buffSystem.OnBuffAdded -= HandleAdd;
                buffSystem.OnBuffRemoved -= HandleRemove;
            }

            buffSystem = null;
            ClearAll();
        }

        // ======================================================
        // ADD / REMOVE
        // ======================================================

        private void HandleAdd(BuffInstance inst)
        {
            Debug.Log($"[BuffHUD] HandleAdd: {inst?.Config?.buffId}", this);

            if (inst == null || inst.Config == null)
            {
                Debug.LogWarning("[BuffHUD] Invalid buff instance", this);
                return;
            }

            if (icons.ContainsKey(inst))
            {
                Debug.LogWarning($"[BuffHUD] Buff {inst.Config.buffId} already has icon", this);
                return;
            }

            if (iconPrefab == null)
            {
                Debug.LogError("[BuffHUD] iconPrefab is null!", this);
                return;
            }

            if (container == null)
            {
                Debug.LogError("[BuffHUD] container is null!", this);
                return;
            }

            var ui = Instantiate(iconPrefab, container);
            ui.name = $"BuffIcon_{inst.Config.buffId}";
            ui.Bind(inst);

            var tt = ui.GetComponent<BuffTooltipTrigger>();
            if (tt != null)
            {
                tt.Bind(inst);
                Debug.Log($"[BuffHUD] Bound tooltip trigger for {inst.Config.buffId}", this);
            }
            else
            {
                Debug.LogWarning("[BuffHUD] BuffTooltipTrigger not found on icon prefab", this);
            }

            icons[inst] = ui;
            Resort();

            Debug.Log($"[BuffHUD] Added buff icon. Total icons: {icons.Count}", this);
        }

        private void HandleRemove(BuffInstance inst)
        {
            Debug.Log($"[BuffHUD] HandleRemove: {inst?.Config?.buffId}", this);

            if (!icons.TryGetValue(inst, out var ui))
            {
                Debug.LogWarning($"[BuffHUD] No icon found for buff {inst?.Config?.buffId}", this);
                return;
            }

            if (ui != null)
            {
                Destroy(ui.gameObject);
                Debug.Log($"[BuffHUD] Destroyed buff icon", this);
            }

            icons.Remove(inst);
            Resort();
        }

        // ======================================================
        // SORT / CLEAR
        // ======================================================

        private void Resort()
        {
            var sorted = icons
                .OrderBy(kv => kv.Key.Config.isDebuff ? 0 : 1) // дебаффы первыми
                .ThenByDescending(kv => kv.Key.Remaining)       // потом по оставшемуся времени
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                sorted[i].Value.transform.SetSiblingIndex(i);

            Debug.Log($"[BuffHUD] Resorted {sorted.Count} buff icons", this);
        }

        private void ClearAll()
        {
            Debug.Log("[BuffHUD] Clearing all buff icons", this);

            foreach (var ui in icons.Values)
            {
                if (ui != null)
                    Destroy(ui.gameObject);
            }

            icons.Clear();
        }
    }
}
