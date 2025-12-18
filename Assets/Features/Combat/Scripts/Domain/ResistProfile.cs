namespace Features.Combat.Domain
{
    [System.Serializable]
    public class ResistProfile
    {
        public float ballistic = 0f;   // процент снижения
        public float explosive = 0f;
        public float fire = 0f;
        public float mining = 0f;
        public float melee = 0f;
        public float electric = 0f;
        public float poison = 0f;
        public float frost = 0f;
        public float acid = 0f;

        public float Get(DamageType type)
        {
            return type switch
            {
                DamageType.Ballistic => ballistic,
                DamageType.Explosive => explosive,
                DamageType.Fire => fire,
                DamageType.Mining => mining,
                DamageType.Melee => melee,
                DamageType.Electric => electric,
                DamageType.Poison => poison,
                DamageType.Frost => frost,
                DamageType.Acid => acid,
                _ => 0
            };
        }
    }
}
