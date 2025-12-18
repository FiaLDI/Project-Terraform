using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;
using Features.Player.UnityIntegration;

namespace Features.Buffs.UI
{
    public class BuffHUD : MonoBehaviour
    {
        [Header("UI")]
        public Transform container;
        public BuffIconUI iconPrefab;

        private BuffSystem buffSystem;
        private readonly Dictionary<BuffInstance, BuffIconUI> icons = new();

        private bool bound = false;

        private void Start()
        {
            TryAutoBind();
        }

        private void Update()
        {
            if (!bound)
                TryAutoBind();
        }

        // ==========================================================
        // AUTO-BIND
        // ==========================================================
        private void TryAutoBind()
        {
            if (PlayerRegistry.Instance == null)
                return;

            var player = PlayerRegistry.Instance.LocalPlayer;
            if (player == null)
                return;

            var bs = player.GetComponent<BuffSystem>();
            if (bs == null)
                return;

            // Подождать один кадр после инициализации BuffSystem
            if (!bs.ServiceReady)
                return;

            Bind(bs);
        }


        // ==========================================================
        // BIND
        // ==========================================================
        private void Bind(BuffSystem bs)
        {
            buffSystem = bs;
            bound = true;

            buffSystem.OnBuffAdded += HandleAdd;
            buffSystem.OnBuffRemoved += HandleRemove;

            // Добавляем уже активные баффы
            if (buffSystem.Active != null)
            {
                foreach (var inst in buffSystem.Active)
                    HandleAdd(inst);
            }
        }

        // ==========================================================
        // ADD / REMOVE ICONS
        // ==========================================================
        private void HandleAdd(BuffInstance inst)
        {
            if (inst == null || inst.Config == null) return;
            if (icons.ContainsKey(inst)) return;

            var ui = Instantiate(iconPrefab, container);
            ui.Bind(inst);

            // Tooltip
            var tt = ui.GetComponent<BuffTooltipTrigger>();
            if (tt != null) tt.Bind(inst);

            icons[inst] = ui;
            Resort();
        }

        private void HandleRemove(BuffInstance inst)
        {
            if (!icons.TryGetValue(inst, out var ui)) return;

            Destroy(ui.gameObject);
            icons.Remove(inst);

            Resort();
        }

        // ==========================================================
        // SORT
        // ==========================================================
        private void Resort()
        {
            var sorted = icons
                .OrderBy(kv => kv.Key.Config.isDebuff ? 0 : 1) // дебаффы слева
                .ThenByDescending(kv => kv.Key.Remaining)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                sorted[i].Value.transform.SetSiblingIndex(i);
        }
    }
}
