using UnityEngine;
using Features.Combat.Devices;
using Features.Abilities.Domain;

namespace Features.Abilities.UnityIntegration
{
    public sealed class ShieldGridHandler
        : AbilityHandler<ShieldGridAbilitySO>
    {
        protected override void ExecuteInternal(
            ShieldGridAbilitySO ability,
            AbilityContext ctx,
            GameObject owner)
        {
            if (!ability.shieldGridPrefab)
            {
                Debug.LogWarning(
                    "[ShieldGridHandler] shieldGridPrefab is null"
                );
                return;
            }

            var gridObj = Object.Instantiate(
                ability.shieldGridPrefab,
                owner.transform.position,
                Quaternion.identity
            );

            if (gridObj.TryGetComponent(out ShieldGridBehaviour grid))
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
