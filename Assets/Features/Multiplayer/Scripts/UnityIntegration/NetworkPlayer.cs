using FishNet.Object;
using UnityEngine;
using Features.Player.UnityIntegration;

public sealed class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private PlayerController playerController;

    public PlayerController Controller => playerController;

    public static event System.Action<NetworkPlayer> OnLocalPlayerSpawned;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner && Owner.IsLocalClient)
        {
            OnLocalPlayerSpawned?.Invoke(this);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (IsOwner && Owner.IsLocalClient && LocalPlayerController.I != null)
        {
            LocalPlayerController.I.Unbind(this);
        }
    }
}
