using Features.Combat.Domain;
using UnityEngine;

namespace Features.Combat.Application
{
    public class DamageCalculationService
    {
        private readonly ResistanceService resistance = new ResistanceService();

        public float CalculateFinalDamage(
            HitInfo hit,
            ResistProfile resist,
            DamageModifiers modifiers)
        {
            float dmg = hit.damage;

            // apply damage multipliers (crit, buffs, etc)
            dmg *= modifiers.multiplier;

            // armor penetration
            dmg = resistance.ApplyArmorPenetration(dmg, modifiers.armorPenetration);

            // resistances
            float resistValue = resist.Get(hit.type);
            dmg = resistance.ApplyResistance(dmg, resistValue);

            // minimum damage safety
            dmg = Mathf.Max(0, dmg);

            return dmg;
        }
    }
}
