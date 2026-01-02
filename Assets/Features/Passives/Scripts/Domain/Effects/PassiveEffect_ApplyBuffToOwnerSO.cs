using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;
using FishNet.Object;
using Features.Buffs.Application;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/Effect/Apply Buff To Owner")]
    public sealed class PassiveEffect_ApplyBuffToOwnerSO : PassiveEffectSO
    {
        public BuffSO buff;

        public override void Apply(GameObject owner)
        {
            var system =
                owner.GetComponent<BuffSystem>() ??
                owner.GetComponentInChildren<BuffSystem>() ??
                owner.GetComponentInParent<BuffSystem>();

            if (system == null)
            {
                Debug.LogError("[PASSIVES] BuffSystem not found", owner);
                return;
            }

            if (!owner.TryGetComponent(out NetworkObject net) || !net.IsServer)
                return;


            Debug.Log(
                $"[PASSIVES] ApplyBuff {buff.name} | IsServer={net?.IsServer}",
                owner
            );

            var runtime = new PassiveRuntime();

            runtime.Buff = system.Add(
                buff,
                source: runtime,
                lifetimeMode: BuffLifetimeMode.WhileSourceAlive
            );

            PassiveRuntimeRegistry.Store(owner, this, runtime);
        }

        public override void Remove(GameObject owner)
        {
            if (!owner.TryGetComponent(out BuffSystem system))
                return;

            if (!owner.TryGetComponent(out NetworkObject net) || !net.IsServer)
                return;

            var runtime = PassiveRuntimeRegistry.Take(owner, this);
            if (runtime?.Buff != null)
                system.Remove(runtime.Buff);
        }
    }
}
