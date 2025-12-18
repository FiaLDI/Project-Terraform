namespace Features.Combat.Application
{
    public class ResistanceService
    {
        public float ApplyResistance(float damage, float resist)
        {
            // resist = 0.3f → reduces damage by 30%
            return damage * (1f - resist);
        }

        public float ApplyArmorPenetration(float damage, float penetration)
        {
            return damage + penetration;
        }
    }
}
