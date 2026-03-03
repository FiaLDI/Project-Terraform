using UnityEngine;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;
using Features.Tools.Domain;

namespace Features.Tools.Application
{
    public sealed class ToolEffectPipeline
    {
        private readonly IBuffSource _source;
        private readonly ToolRuntimeStats _stats;
        private readonly EffectDefinition[] _definitions;

        public ToolEffectPipeline(
            IBuffSource source,
            ToolRuntimeStats stats,
            EffectDefinition[] definitions)
        {
            _source = source;
            _stats = stats;
            _definitions = definitions;
        }

        public void Execute(Vector3 origin, Vector3 direction)
        {
            if (_definitions == null || _definitions.Length == 0)
                return;

            var baseContext = new EffectContext(
                source: _source,
                targets: null,
                origin: origin,
                direction: direction
            );

            foreach (var def in _definitions)
            {
                var modified = ModifyByRuntimeStats(def);
                EffectExecutor.Instance.Execute(modified, baseContext);
            }
        }
        
        private EffectDefinition ModifyByRuntimeStats(EffectDefinition def)
        {
            var result = def;

            switch (result.type)
            {
                case EffectType.DealDamage:
                    result.value *= _stats[ToolStat.Damage];
                    break;

                case EffectType.MineNetworkResource:
                    result.value *= _stats[ToolStat.MiningSpeed];
                    break;
            }

            if (result.radius > 0f)
            {
                result.radius = def.radius + _stats[ToolStat.Range];
            }

            return result;
        }
    }
}
