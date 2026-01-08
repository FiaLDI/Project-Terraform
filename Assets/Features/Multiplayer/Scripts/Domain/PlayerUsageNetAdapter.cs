using FishNet.Object;
using UnityEngine;
using Features.Equipment.Domain;

public sealed class PlayerUsageNetAdapter : NetworkBehaviour
{
    [SerializeField] private float aimSendRate = 20f;

    private IUsable rightHand;
    private IUsable leftHand;

    private bool clientPrimaryHeld;
    private bool clientSecondaryHeld;

    private bool serverPrimaryHeld;
    private bool serverSecondaryHeld;

    private Vector3 serverAimOrigin;
    private Vector3 serverAimForward = Vector3.forward;
    private bool hasServerAim;
    private float nextAimSendTime;

    public void OnHandsUpdated(IUsable left, IUsable right, bool twoHanded)
    {
        leftHand = left;
        rightHand = right;
    }

    public bool TryGetServerAim(out Ray ray)
    {
        ray = new Ray(serverAimOrigin, serverAimForward);
        return hasServerAim;
    }

    // ================= PRIMARY =================

    public void PrimaryStart()
    {
        if (!IsOwner) return;

        // FX на клиенте
        clientPrimaryHeld = true;
        rightHand?.OnUsePrimary_Start();

        // локальный-only — не идём на сервер
        if (rightHand is ILocalOnlyUsable)
            return;

        // ✅ хост: НЕ вызываем usable второй раз
        if (IsServerInitialized)
        {
            serverPrimaryHeld = true;
            return;
        }

        PrimaryStart_Server();
    }

    public void PrimaryStop()
    {
        if (!IsOwner) return;

        clientPrimaryHeld = false;
        rightHand?.OnUsePrimary_Stop();

        if (rightHand is ILocalOnlyUsable)
            return;

        if (IsServerInitialized)
        {
            serverPrimaryHeld = false;
            return;
        }

        PrimaryStop_Server();
    }

    [ServerRpc]
    private void PrimaryStart_Server() => serverPrimaryHeld = true;

    [ServerRpc]
    private void PrimaryStop_Server() => serverPrimaryHeld = false;

    // ================= SECONDARY =================

    public void SecondaryStart()
    {
        if (!IsOwner) return;

        clientSecondaryHeld = true;
        rightHand?.OnUseSecondary_Start();

        if (rightHand is ILocalOnlyUsable)
            return;

        if (IsServerInitialized)
        {
            serverSecondaryHeld = true;
            return;
        }

        SecondaryStart_Server();
    }

    public void SecondaryStop()
    {
        if (!IsOwner) return;

        clientSecondaryHeld = false;
        rightHand?.OnUseSecondary_Stop();

        if (rightHand is ILocalOnlyUsable)
            return;

        if (IsServerInitialized)
        {
            serverSecondaryHeld = false;
            return;
        }

        SecondaryStop_Server();
    }

    [ServerRpc]
    private void SecondaryStart_Server() => serverSecondaryHeld = true;

    [ServerRpc]
    private void SecondaryStop_Server() => serverSecondaryHeld = false;

    // ================= RELOAD =================

    public void Reload()
    {
        if (!IsOwner) return;

        // reload почти всегда gameplay → сервер
        if (rightHand is ILocalOnlyUsable)
            return;

        if (IsServerInitialized)
        {
            ServerReload_Exec();
            return;
        }

        Reload_Server();
    }

    [ServerRpc]
    private void Reload_Server() => ServerReload_Exec();

    // ✅ реально выполняется ТОЛЬКО на сервере (и на хосте)
    private void ServerReload_Exec()
    {
        if (rightHand is IReloadable r)
            r.OnReloadPressed();
    }

    // ================= UPDATE =================

    private void Update()
    {
        // SERVER authoritative tick (gameplay)
        if (IsServerInitialized)
        {
            if (serverPrimaryHeld)
                rightHand?.OnUsePrimary_Hold();

            if (serverSecondaryHeld)
                rightHand?.OnUseSecondary_Hold();

            return;
        }

        // CLIENT owner tick (FX + aim)
        if (!IsOwner)
            return;

        if (clientPrimaryHeld)
            rightHand?.OnUsePrimary_Hold();

        if (clientSecondaryHeld)
            rightHand?.OnUseSecondary_Hold();

        if (clientPrimaryHeld || clientSecondaryHeld)
            SendAimToServerThrottled();
    }

    private void SendAimToServerThrottled()
    {
        if (Time.time < nextAimSendTime)
            return;

        nextAimSendTime = Time.time + (1f / Mathf.Max(1f, aimSendRate));

        var cam = Camera.main;
        if (cam == null)
            return;

        UpdateAim_Server(cam.transform.position, cam.transform.forward);
    }

    [ServerRpc(RequireOwnership = true)]
    private void UpdateAim_Server(Vector3 origin, Vector3 forward)
    {
        serverAimOrigin = origin;
        serverAimForward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        hasServerAim = true;
    }
}
