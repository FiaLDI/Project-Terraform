// Assets/Features/Abilities/Scripts/UnityIntegration/Handlers/AbilityHandler.cs
using UnityEngine;
using Features.Abilities.Domain;
using FishNet;

namespace Features.Abilities.UnityIntegration
{
    public abstract class AbilityHandler<TAbility> : IAbilityHandler
        where TAbility : AbilitySO
    {
        public System.Type AbilityType => typeof(TAbility);

        // ===== ENTRY POINT (CALLED BY AbilityExecutor) =====
        public void ExecuteServer(AbilitySO ability, AbilityContext ctx)
        {
            if (!InstanceFinder.IsServer)
            {
                Debug.LogError(
                    $"[{GetType().Name}] ExecuteServer called not on server"
                );
                return;
            }

            if (ability is not TAbility typedAbility)
            {
                Debug.LogError(
                    $"[{GetType().Name}] Wrong ability type: {ability?.GetType().Name}"
                );
                return;
            }

            if (!TryResolveOwner(ctx.Owner, out var owner))
                return;

            ExecuteInternal(typedAbility, ctx, owner);
        }

        protected abstract void ExecuteInternal(
            TAbility ability,
            AbilityContext ctx,
            GameObject owner);

        protected static bool TryResolveOwner(object owner, out GameObject go)
        {
            go = owner switch
            {
                GameObject g => g,
                Component c => c.gameObject,
                _ => null
            };

            if (go == null)
            {
                Debug.LogError(
                    "[AbilityHandler] AbilityContext.Owner is invalid"
                );
                return false;
            }

            return true;
        }
    }
}
