using System;
using System.Collections.Generic;
using UnityEngine;
using Features.Buffs.Application;
using Features.Buffs.Data;
using FishNet.Object.Synchronizing;
using Features.Buffs.Domain;
using Features.Buffs.Client;

namespace Features.Buffs.Client
{
    [DisallowMultipleComponent]
    public sealed class ClientBuffView : MonoBehaviour
    {
        public event Action BuffsChanged;

        private BuffSystem buffSystem;

        private readonly List<ActiveBuffView> active = new();
        public IReadOnlyList<ActiveBuffView> Active => active;

        private void Awake()
        {
            buffSystem = GetComponentInChildren<BuffSystem>(true);
        }

        public void Bind()
        {
            if (buffSystem == null)
                return;

            buffSystem.ActiveBuffIds.OnChange += OnBuffIdsChanged;
            Rebuild();
        }

        public void Unbind()
        {
            if (buffSystem != null)
                buffSystem.ActiveBuffIds.OnChange -= OnBuffIdsChanged;

            active.Clear();
        }

        private void OnBuffIdsChanged(
            SyncListOperation _,
            int __,
            string ___,
            string ____,
            bool asServer)
        {
            if (!asServer)
                Rebuild();
        }

        private void Rebuild()
        {
            active.Clear();

            foreach (var id in buffSystem.ActiveBuffIds)
            {
                active.Add(new ActiveBuffView(id));
            }

            BuffsChanged?.Invoke();
        }
    }
}
