using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;
using Features.UI;
using FishNet.Object.Synchronizing;

namespace Features.Buffs.UI
{
    public sealed class BuffHUD : PlayerBoundUIView
    {
        [Header("UI")]
        [SerializeField] private Transform container;
        [SerializeField] private BuffIconUI iconPrefab;

        private BuffSystem buffSystem;

        // KEY = buffId (уникально на стороне сервера)
        private readonly Dictionary<string, BuffIconUI> icons = new();

        // ======================================================
        // PLAYER BIND
        // ======================================================

        protected override void OnPlayerBound(GameObject player)
        {
            buffSystem = player.GetComponentInChildren<BuffSystem>(true);
            if (buffSystem == null)
            {
                Debug.LogError("[BuffHUD] BuffSystem not found on player", this);
                return;
            }

            buffSystem.ActiveBuffIds.OnChange += OnBuffIdsChanged;

            // ✅ late join / reconnect safe
            RebuildFromServer();
        }

        protected override void OnPlayerUnbound(GameObject player)
        {
            Unbind();
        }

        // ======================================================
        // UNBIND
        // ======================================================

        private void Unbind()
        {
            if (buffSystem != null)
            {
                buffSystem.ActiveBuffIds.OnChange -= OnBuffIdsChanged;
            }

            buffSystem = null;
            ClearAll();
        }

        // ======================================================
        // SYNC FROM SERVER
        // ======================================================

        private void OnBuffIdsChanged(
            SyncListOperation op,
            int index,
            string oldItem,
            string newItem,
            bool asServer)
        {
            if (asServer)
                return;

            RebuildFromServer();
        }

        private void RebuildFromServer()
        {
            if (buffSystem == null)
                return;

            var serverIds = new HashSet<string>(buffSystem.ActiveBuffIds);

            // === REMOVE UI ===
            var toRemove = icons.Keys
                .Where(id => !serverIds.Contains(id))
                .ToList();

            foreach (var id in toRemove)
            {
                if (icons.TryGetValue(id, out var ui) && ui != null)
                    Destroy(ui.gameObject);

                icons.Remove(id);
            }

            // === ADD UI ===
            foreach (var id in serverIds)
            {
                if (icons.ContainsKey(id))
                    continue;

                var inst = FindInstanceById(id);
                if (inst != null)
                    CreateIcon(inst);
            }

            Resort();
        }

        private BuffInstance FindInstanceById(string buffId)
        {
            if (buffSystem?.Active == null)
                return null;

            foreach (var inst in buffSystem.Active)
            {
                if (inst?.Config != null && inst.Config.buffId == buffId)
                    return inst;
            }

            return null;
        }

        // ======================================================
        // UI CREATE
        // ======================================================

        private void CreateIcon(BuffInstance inst)
        {
            if (inst == null || inst.Config == null)
                return;

            if (container == null || iconPrefab == null)
                return;

            var ui = Instantiate(iconPrefab, container);
            ui.Bind(inst);

            var tt = ui.GetComponent<BuffTooltipTrigger>();
            if (tt != null)
                tt.Bind(inst);

            icons[inst.Config.buffId] = ui;
        }

        // ======================================================
        // HELPERS
        // ======================================================

        private void Resort()
        {
            if (icons.Count <= 1)
                return;

            var sorted = icons
                .Select(kv => new
                {
                    id = kv.Key,
                    ui = kv.Value,
                    inst = FindInstanceById(kv.Key)
                })
                .Where(x => x.inst != null)
                .OrderBy(x => x.inst.Config.isDebuff ? 0 : 1)
                .ThenByDescending(x => x.inst.Remaining)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                sorted[i].ui.transform.SetSiblingIndex(i);
        }

        private void ClearAll()
        {
            foreach (var ui in icons.Values)
            {
                if (ui != null)
                    Destroy(ui.gameObject);
            }

            icons.Clear();
        }
    }
}
