using System.Collections.Generic;
using Features.Energy.Domain;
using UnityEngine;

namespace Features.Energy.Application
{
    /// <summary>
    /// Централизованный расчёт стоимости энергии с учётом модификаторов.
    /// Каждый IEnergyCostModifier возвращает МНОЖИТЕЛЬ, а не финальную цену.
    /// </summary>
    public class EnergyCostService
    {
        private readonly List<IEnergyCostModifier> _mods = new();

        public void AddModifier(IEnergyCostModifier mod)
        {
            if (mod != null && !_mods.Contains(mod))
                _mods.Add(mod);
            
            Debug.Log(mod);
        }

        public void RemoveModifier(IEnergyCostModifier mod)
        {
            if (mod != null)
                _mods.Remove(mod);
        }

        /// <summary>
        /// Возвращает итоговую стоимость: 
        /// final = baseCost × product(all modifiers)
        /// </summary>
        public float ApplyModifiers(float baseCost)
        {
            float mult = 1f;

            foreach (var m in _mods)
                mult *= m.ModifyCost(baseCost);   // ← возвращает multiplier

            return baseCost * mult;
        }
    }
}
