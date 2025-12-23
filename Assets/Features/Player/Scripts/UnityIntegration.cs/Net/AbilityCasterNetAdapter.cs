using FishNet.Object;
using UnityEngine;
using Features.Abilities.Application;

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
        caster.enabled = Owner.IsLocalClient;
    }

    public override void OnStopClient()
    {
        if (caster != null)
            caster.enabled = false;
    }

    // ================= INPUT =================

    public void Cast(int index)
    {
        if (caster == null || !caster.IsReady)
            return;

        // Host
        if (IsServerInitialized && Owner.IsLocalClient)
        {
            caster.TryCast(index);
            return;
        }

        // Client → Server
        if (Owner.IsLocalClient)
            Cast_Server(index);
    }

    [ServerRpc]
    private void Cast_Server(int index)
    {
        if (caster == null || !caster.IsReady)
            return;

        caster.TryCast(index);
    }
}
