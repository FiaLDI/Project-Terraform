using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public static class EffectFactory
    {
        public static IEffect Create(EffectDefinition def)
        {
            return def.type switch
            {
                EffectType.DealDamage =>
                    new DealDamageEffect(def.value, def.damageType),

                EffectType.HitscanDamage =>
                    new HitscanDamageEffect(
                        def.value,
                        def.radius,
                        def.layerMask,
                        def.damageType
                    ),

                EffectType.HealInstant =>
                    new HealInstantEffect(def.value),

                EffectType.ApplyBuff =>
                    new ApplyBuffEffect(def.buff),

                EffectType.RemoveBuffSource =>
                    new RemoveBuffSourceEffect(
                        def.onlySpecificBuff,
                        def.buffId
                    ),

                EffectType.SpawnPrefab =>
                    new SpawnPrefabEffect(
                        def.prefabId,
                        def.lifetime,
                        def.useSourcePosition
                    ),

                EffectType.MineNetworkResource =>
                    new MineNetworkResourceEffect(
                        def.value,
                        def.radius,
                        def.layerMask
                    ),

                EffectType.Continuous =>
                    new ContinuousEffect(def.value, def.childEffects),

                EffectType.StopContinuous =>
                    new StopContinuousEffect(),

                EffectType.Scan =>
                    new ScanEffect(def.value),

                _ => null
            };
        }
    }
}
