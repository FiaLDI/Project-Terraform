using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Data;
using Features.Buffs.Client;
using Features.UI;
using Features.Buffs.Domain;

namespace Features.Buffs.UI
{
    public sealed class BuffHUD : PlayerBoundUIView
    {
        [Header("UI")]
        [SerializeField] private Transform container;
        [SerializeField] private BuffIconUI iconPrefab;

        private ClientBuffView buffView;

        // KEY = buffId
        private readonly Dictionary<string, BuffIconUI> icons = new();

        // ======================================================
        // PLAYER BIND
        // ======================================================

        protected override void OnPlayerBound(GameObject player)
        {
            buffView = player.GetComponent<ClientBuffView>();
            if (buffView == null)
            {
                Debug.LogError("[BuffHUD] ClientBuffView missing", this);
                return;
            }

            buffView.BuffsChanged += Rebuild;
            buffView.Bind();

            Rebuild();
        }

        protected override void OnPlayerUnbound(GameObject player)
        {
            if (buffView != null)
            {
                buffView.BuffsChanged -= Rebuild;
                buffView.Unbind();
            }

            buffView = null;
            ClearAll();
        }

        // ======================================================
        // UI BUILD
        // ======================================================

        private void Rebuild()
        {
            ClearAll();

            if (buffView == null)
                return;

            foreach (var view in buffView.Active)
                CreateIcon(view.buffId, view.stacks);
        }

        private void CreateIcon(string buffId, int stacks)
        {
            var ui = Instantiate(iconPrefab, container);
            ui.Bind(buffId, stacks);

            var tt = ui.GetComponentInChildren<BuffTooltipTrigger>();
            if (tt != null)
                tt.Bind(buffId, stacks);

            icons[buffId] = ui;
        }

        // ======================================================
        // CLEANUP
        // ======================================================

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
