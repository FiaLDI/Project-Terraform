using UnityEngine;
using Features.Abilities.Domain;
using Features.Combat.Devices;

namespace Features.Abilities.UnityIntegration
{
    public class ShieldGridHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(ShieldGridAbilitySO);

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (ShieldGridAbilitySO)abilityBase;
            var owner = ctx.Owner;
            if (!owner) return;

            if (!ability.shieldGridPrefab)
            {
                Debug.LogWarning("[ShieldGridHandler] shieldGridPrefab is null.");
                return;
            }

            GameObject gridObj = Object.Instantiate(
                ability.shieldGridPrefab,
                owner.transform.position,
                Quaternion.identity
            );

            if (gridObj.TryGetComponent<ShieldGridBehaviour>(out var grid))
            {
                grid.Init(
                    ability.radius,
                    ability.duration,
                    ability.damageReductionPercent,
                    owner
                );
            }

            Object.Destroy(gridObj, ability.duration + 0.2f);
        }
    }
}
