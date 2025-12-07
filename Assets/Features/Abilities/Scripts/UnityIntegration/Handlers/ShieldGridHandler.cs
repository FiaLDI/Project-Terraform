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

            // ==== ADAPTATION: ctx.Owner is NOW object ====
            GameObject ownerGO = null;

            switch (ctx.Owner)
            {
                case GameObject go:
                    ownerGO = go;
                    break;

                case Component comp:
                    ownerGO = comp.gameObject;
                    break;

                default:
                    Debug.LogError("[ShieldGridHandler] AbilityContext.Owner is not GameObject or Component.");
                    return;
            }

            if (ownerGO == null)
                return;

            if (!ability.shieldGridPrefab)
            {
                Debug.LogWarning("[ShieldGridHandler] shieldGridPrefab is null.");
                return;
            }

            GameObject gridObj = Object.Instantiate(
                ability.shieldGridPrefab,
                ownerGO.transform.position,
                Quaternion.identity
            );

            if (gridObj.TryGetComponent<ShieldGridBehaviour>(out var grid))
            {
                grid.Init(
                    ability.radius,
                    ability.duration,
                    ability.damageReductionPercent,
                    ownerGO   // explicitly passing GO
                );
            }

            Object.Destroy(gridObj, ability.duration + 0.2f);
        }
    }
}
