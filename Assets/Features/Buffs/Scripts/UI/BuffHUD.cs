using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;
using Features.UI; // PlayerBoundUIView

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
            var bs = player.GetComponent<BuffSystem>();
            if (bs == null)
                return;

            // BuffSystem может быть не готов
            if (!bs.ServiceReady)
            {
                waitCoroutine = StartCoroutine(WaitForBuffSystem(bs));
                return;
            }

            Bind(bs);
        }

        protected override void OnPlayerUnbound(GameObject player)
        {
            Unbind();
        }

        // ======================================================
        // WAIT FOR READY
        // ======================================================

        private System.Collections.IEnumerator WaitForBuffSystem(BuffSystem bs)
        {
            while (bs != null && !bs.ServiceReady)
                yield return null;

            waitCoroutine = null;

            if (bs != null)
                Bind(bs);
        }

        // ======================================================
        // BIND / UNBIND
        // ======================================================

        private void Bind(BuffSystem bs)
        {
            buffSystem = bs;

            buffSystem.OnBuffAdded += HandleAdd;
            buffSystem.OnBuffRemoved += HandleRemove;

            if (buffSystem.Active != null)
            {
                foreach (var inst in buffSystem.Active)
                    HandleAdd(inst);
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
            }

            buffSystem = null;
            ClearAll();
        }

        // ======================================================
        // ADD / REMOVE
        // ======================================================

        private void HandleAdd(BuffInstance inst)
        {
            if (inst == null || inst.Config == null)
                return;

            if (icons.ContainsKey(inst))
                return;

            var ui = Instantiate(iconPrefab, container);
            ui.Bind(inst);

            var tt = ui.GetComponent<BuffTooltipTrigger>();
            if (tt != null)
                tt.Bind(inst);

            icons[inst] = ui;
            Resort();
        }

        private void HandleRemove(BuffInstance inst)
        {
            if (!icons.TryGetValue(inst, out var ui))
                return;

            if (ui != null)
                Destroy(ui.gameObject);

            icons.Remove(inst);
            Resort();
        }

        // ======================================================
        // SORT / CLEAR
        // ======================================================

        private void Resort()
        {
            var sorted = icons
                .OrderBy(kv => kv.Key.Config.isDebuff ? 0 : 1)
                .ThenByDescending(kv => kv.Key.Remaining)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                sorted[i].Value.transform.SetSiblingIndex(i);
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
