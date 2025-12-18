namespace Features.Combat.Domain
{
    public struct HitInfo
    {
        public float damage;
        public DamageType type;

        public UnityEngine.Vector3 point;
        public UnityEngine.Vector3 direction;

        public HitInfo(float dmg, DamageType type, UnityEngine.Vector3 point = default, UnityEngine.Vector3 dir = default)
        {
            this.damage = dmg;
            this.type = type;
            this.point = point;
            this.direction = dir;
        }
    }
}
