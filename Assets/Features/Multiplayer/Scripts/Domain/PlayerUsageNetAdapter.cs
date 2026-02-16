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

    // ======================================================
    // HANDS
    // ======================================================

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

    // ======================================================
    // PRIMARY
    // ======================================================

    public void PrimaryStart()
    {
        if (!IsOwner) return;

        clientPrimaryHeld = true;
        rightHand?.OnUsePrimary_Start();

        if (rightHand is ILocalOnlyUsable)
            return;

        if (IsServerInitialized)
        {
            serverPrimaryHeld = true;
            (rightHand as IServerUsable)?.ServerPrimaryStart();
            return;
        }

        PrimaryStart_Server();
    }

    public void PrimaryStop()
    {
        if (!IsOwner) return;

        clientPrimaryHeld = false;
        rightHand?.OnUsePrimary_Stop();

        if (IsServerInitialized)
        {
            serverPrimaryHeld = false;
            (rightHand as IServerUsable)?.ServerPrimaryStop();
            return;
        }

        PrimaryStop_Server();
    }

    [ServerRpc]
    private void PrimaryStart_Server()
    {
        serverPrimaryHeld = true;
        (rightHand as IServerUsable)?.ServerPrimaryStart();
    }

    [ServerRpc]
    private void PrimaryStop_Server()
    {
        serverPrimaryHeld = false;
        (rightHand as IServerUsable)?.ServerPrimaryStop();
    }

    // ======================================================
    // SECONDARY
    // ======================================================

    public void SecondaryStart()
    {
        if (!IsOwner) return;

        clientSecondaryHeld = true;
        rightHand?.OnUseSecondary_Start();

        if (IsServerInitialized)
        {
            serverSecondaryHeld = true;
            (rightHand as IServerUsable)?.ServerSecondaryStart();
            return;
        }

        SecondaryStart_Server();
    }

    public void SecondaryStop()
    {
        if (!IsOwner) return;

        clientSecondaryHeld = false;
        rightHand?.OnUseSecondary_Stop();

        if (IsServerInitialized)
        {
            serverSecondaryHeld = false;
            (rightHand as IServerUsable)?.ServerSecondaryStop();
            return;
        }

        SecondaryStop_Server();
    }

    [ServerRpc]
    private void SecondaryStart_Server()
    {
        serverSecondaryHeld = true;
        (rightHand as IServerUsable)?.ServerSecondaryStart();
    }

    [ServerRpc]
    private void SecondaryStop_Server()
    {
        serverSecondaryHeld = false;
        (rightHand as IServerUsable)?.ServerSecondaryStop();
    }

    // ======================================================
    // RELOAD
    // ======================================================

    public void Reload()
    {
        if (!IsOwner) return;

        if (IsServerInitialized)
        {
            (rightHand as IServerUsable)?.ServerReload();
            return;
        }

        Reload_Server();
    }

    [ServerRpc]
    private void Reload_Server()
    {
        (rightHand as IServerUsable)?.ServerReload();
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void Update()
    {
        // SERVER authoritative tick
        if (IsServerInitialized)
        {
            if (serverPrimaryHeld)
            {
                (rightHand as IServerUsable)?.ServerPrimaryHold();
            }
        }

        // CLIENT owner tick
        if (!IsOwner)
            return;

        if (clientPrimaryHeld)
            rightHand?.OnUsePrimary_Hold();

        if (clientSecondaryHeld)
            rightHand?.OnUseSecondary_Hold();

        if (clientPrimaryHeld || clientSecondaryHeld)
            SendAimToServerThrottled();
    }

    // ======================================================
    // AIM
    // ======================================================

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
        serverAimForward =
            forward.sqrMagnitude > 0.0001f
                ? forward.normalized
                : Vector3.forward;

        hasServerAim = true;
    }
}
