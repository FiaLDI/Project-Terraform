using Features.Abilities.Application;
using Features.Abilities.Domain;
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(AbilityCaster))]
public sealed class AbilityCasterNetAdapter : NetworkBehaviour
{
    private AbilityCaster caster;

    public override void OnStartServer()
    {
        caster = GetComponent<AbilityCaster>();
    }

    public override void OnStartClient()
    {
        caster = GetComponent<AbilityCaster>();
    }

    // ===== INPUT от владельца =====
    public void Cast(int index)
    {
        if (caster == null || !caster.IsReady || !Owner.IsLocalClient)
            return;

        Cast_Server(index);
    }

    // ===== SERVER =====
    [ServerRpc]
    private void Cast_Server(int index)
    {
        if (caster == null || !caster.IsReady)
            return;

        if (!caster.TryCastWithContext(index, out AbilitySO ability, out AbilityContext ctx))
            return;

        string abilityId = ability.id;   // поле id в AbilitySO
        Cast_Client(index, abilityId, ctx);
    }

    // ===== CLIENTS (визуал) =====
    [ObserversRpc]
    private void Cast_Client(int index, string abilityId, AbilityContext ctx)
    {
        if (caster == null || !caster.IsReady)
            return;

        var ability = caster.FindAbilityById(abilityId);
        if (ability == null)
        {
            Debug.LogWarning($"[AbilityCasterNetAdapter] Ability {abilityId} not found in library");
            return;
        }

        caster.PlayRemoteCast(ability, index, ctx);
    }
}
