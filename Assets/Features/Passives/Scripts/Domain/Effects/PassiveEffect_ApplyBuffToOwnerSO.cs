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
            if (buff == null)
                return;

            var system = owner.GetComponent<BuffSystem>();
            if (system == null)
                return;

            var netObj = owner.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsServer)
                return;

            system.Add(
                buff,
                source: this,
                lifetimeMode: BuffLifetimeMode.WhileSourceAlive
            );
        }

        public override void Remove(GameObject owner)
        {
            var system = owner.GetComponent<BuffSystem>();
            if (system == null)
                return;

            var netObj = owner.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsServer)
                return;

            system.RemoveBySource(this);
        }
    }
}
