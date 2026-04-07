using UnityEngine;
using FishNet;
using Features.Effects.Domain;
using Features.Buffs.Domain;
using Features.Stats.Domain;

namespace Features.Effects.Application
{
    public sealed class MineNetworkResourceEffect : IEffect
    {
        private readonly float _value;
        private readonly float _range;
        private readonly LayerMask _mask;

        public MineNetworkResourceEffect(
            float value,
            float range,
            LayerMask mask)
        {
            _value = value;
            _range = range;
            _mask = mask;
        }

        public void Apply(EffectContext context)
        {
            if (!InstanceFinder.IsServer)
                return;

            Debug.Log("[MINE] Apply called");

            if (context.Targets == null)
            {
                Debug.Log("[MINE] Targets NULL");
                return;
            }

            Debug.Log($"[MINE] Targets count: {context.Targets.Length}");

            foreach (var t in context.Targets)
            {
                if (t == null)
                {
                    Debug.Log("[MINE] Target NULL");
                    continue;
                }

                Debug.Log($"[MINE] Target: {t.Transform.name}");

                var node = t.Transform.GetComponentInParent<ResourceNodeNetwork>();

                if (node != null)
                {
                    Debug.Log($"[MINE] FOUND NODE: {node.name}");
                    node.Mine_Server(_value, 1f);
                }
                else
                {
                    Debug.Log("[MINE] ResourceNodeNetwork NOT FOUND");
                }
            }
        }
    }
}