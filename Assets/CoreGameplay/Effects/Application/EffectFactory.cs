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
                    new DealDamageEffect(def.value),

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

                _ => null
            };
        }
    }
}
