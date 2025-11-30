using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;

namespace Features.Buffs.UI
{
    public class BuffHUD : MonoBehaviour
    {
        public Transform container;
        public BuffIconUI iconPrefab;

        private BuffSystem buffSystem;
        private readonly Dictionary<BuffInstance, BuffIconUI> icons = new();

        private void Update()
        {
            if (buffSystem == null)
                TryBind();
        }

        private void TryBind()
        {
            var target = FindFirstObjectByType<PlayerBuffTarget>();
            if (!target) return;

            buffSystem = target.GetComponent<BuffSystem>();
            if (buffSystem == null) return;

            buffSystem.OnBuffAdded += HandleAdd;
            buffSystem.OnBuffRemoved += HandleRemove;

            // восстанавливаем уже активные баффы
            foreach (var inst in buffSystem.Active)
                HandleAdd(inst);
        }

        private void HandleAdd(BuffInstance inst)
        {
            if (icons.ContainsKey(inst)) return;

            var ui = Instantiate(iconPrefab, container);
            ui.Bind(inst);

            // добавляем TooltipTrigger
            var trigger = ui.gameObject.AddComponent<BuffTooltipTrigger>();
            trigger.Bind(inst);

            icons[inst] = ui;
            Resort();
        }

        private void HandleRemove(BuffInstance inst)
        {
            if (!icons.TryGetValue(inst, out var ui))
                return;

            Destroy(ui.gameObject);
            icons.Remove(inst);

            Resort();
        }

        private void Resort()
        {
            var sorted = icons
                .OrderBy(kv => kv.Key.Config.isDebuff ? 0 : 1)
                .ThenByDescending(kv => kv.Key.Remaining)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                sorted[i].Value.transform.SetSiblingIndex(i);
        }
    }
}
