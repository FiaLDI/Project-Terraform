using FishNet.Object;
using UnityEngine;
using Features.Abilities.Application;

[RequireComponent(typeof(AbilityCaster))]
public sealed class AbilityCasterNetAdapter : NetworkBehaviour
{
    private AbilityCaster caster;

    private void Awake()
    {
        caster = GetComponent<AbilityCaster>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Локальный клиент НЕ кастует напрямую
        caster.enabled = false;
    }

    // ================= INPUT =================

    public void Cast(int index)
    {
        // Host (server + local)
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
        caster.TryCast(index);
    }
}
