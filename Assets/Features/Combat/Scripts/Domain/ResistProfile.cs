using Features.Effects.Domain;
using UnityEngine;

namespace Features.Combat.Domain
{
    [System.Serializable]
    public class ResistProfile
    {
        [Range(0f, 1f)] public float ballistic;
        [Range(0f, 1f)] public float explosion;
        [Range(0f, 1f)] public float energy;
        [Range(0f, 1f)] public float mining;
        [Range(0f, 1f)] public float melee;
        [Range(0f, 1f)] public float fire;
        [Range(0f, 1f)] public float electric;
        [Range(0f, 1f)] public float poison;
        [Range(0f, 1f)] public float frost;
        [Range(0f, 1f)] public float acid;

        public float Get(DamageType type)
        {
            return type switch
            {
                DamageType.Ballistic => ballistic,
                DamageType.Explosion => explosion,
                DamageType.Energy => energy,
                DamageType.Mining => mining,
                DamageType.Melee => melee,
                DamageType.Fire => fire,
                DamageType.Electric => electric,
                DamageType.Poison => poison,
                DamageType.Frost => frost,
                DamageType.Acid => acid,
                _ => 0f
            };
        }
    }
}
