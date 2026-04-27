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

            buffSystem.ActiveBuffStates.OnChange += OnBuffStatesChanged;
            Rebuild();
        }

        public void Unbind()
        {
            if (buffSystem != null)
                buffSystem.ActiveBuffStates.OnChange -= OnBuffStatesChanged;

            active.Clear();
        }

        private void OnBuffStatesChanged(
            SyncListOperation _,
            int __,
            string ___,
            string ____,
            bool asServer)
        {
            Rebuild();
        }

        private void Rebuild()
        {
            active.Clear();

            foreach (var state in buffSystem.ActiveBuffStates)
            {
                if (!ActiveBuffSyncCodec.TryDecode(state, out var buffId, out var stacks))
                    continue;

                active.Add(new ActiveBuffView(buffId, stacks));
            }

            BuffsChanged?.Invoke();
        }
    }
}
