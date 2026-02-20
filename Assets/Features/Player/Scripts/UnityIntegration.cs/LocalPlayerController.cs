using UnityEngine;
using Features.Input;
using Features.Player.UnityIntegration;
using Features.Player;
using Features.Player.UI;
using Features.Interaction.UnityIntegration;

public sealed class LocalPlayerController : MonoBehaviour
{
    public static LocalPlayerController I { get; private set; }

    private PlayerInputContext inputContext;
    private NetworkPlayer boundPlayer;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        inputContext = GetComponent<PlayerInputContext>();
        if (inputContext == null)
            Debug.LogError("[LocalPlayerController] PlayerInputContext missing");
    }

    private void OnEnable()
    {
        PlayerRegistry.SubscribeLocalPlayerReady(OnLocalReady);
    }

    private void OnDisable()
    {
        PlayerRegistry.UnsubscribeLocalPlayerReady(OnLocalReady);
    }

    private void OnLocalReady(PlayerRegistry reg)
    {
        var player = reg.LocalPlayer;
        if (player == null)
            return;

        var net = player.GetComponent<NetworkPlayer>();
        if (net == null || !net.IsOwner)
            return;

        Bind(net);
    }


    // ======================================================
    // BIND / UNBIND
    // ======================================================

    public void Bind(NetworkPlayer player)
    {
        Debug.Log($"[fix-net] LocalPlayerController.Bind -> {player.name}");

        if (!player.IsOwner)
        {
            Debug.LogWarning("[LocalPlayerController] Bind called for non-owner player, ignoring", player);
            return;
        }

        if (boundPlayer == player)
            return;

        Debug.Log($"[LocalPlayerController] Bind to {player.name}");

        if (boundPlayer != null)
            Unbind(boundPlayer);

        boundPlayer = player;

        Debug.Log("[LocalPlayerController] Binding UI...", this);
        if (PlayerUIRoot.I != null)
            PlayerUIRoot.I.Bind(player.gameObject);
        else
            Debug.LogError("[LocalPlayerController] PlayerUIRoot.I is null!", this);

        if (inputContext != null)
        {
            inputContext.Enable();
            InputModeManager.I.Bind(inputContext);
            InputModeManager.I.SetMode(InputMode.Gameplay);
        }

        var consumers = this.GetComponents<IInputContextConsumer>();
        foreach (var c in consumers)
            c.BindInput(inputContext);

        var camController = player.GetComponent<PlayerCameraController>();
        if (camController != null)
            camController.SetLocal(true);

        var cam = Camera.main;
        if (cam != null)
        {
            var rayProvider = cam.GetComponent<CameraRayProvider>();
            if (rayProvider != null)
            {
                InteractionServiceProvider.Init(rayProvider);
                Debug.Log("[LocalPlayerController] InteractionRayService initialized");
            }
            else
            {
                Debug.LogError("[LocalPlayerController] CameraRayProvider NOT FOUND");
            }
        }
        else
        {
            Debug.LogError("[LocalPlayerController] Camera.main NOT FOUND");
        }
        Debug.Log($"[LocalPlayerController] Bound to {player.name}");
    }


    public void Unbind(NetworkPlayer player)
    {
        if (boundPlayer == null || boundPlayer != player)
            return;

        var consumers = this.GetComponents<IInputContextConsumer>();
        foreach (var c in consumers)
            c.UnbindInput(inputContext);

        var cam = boundPlayer.GetComponent<PlayerCameraController>();
        if (cam != null)
            cam.SetLocal(false);

        if (inputContext != null)
            inputContext.Disable();

        boundPlayer = null;

        if (PlayerUIRoot.I != null)
            PlayerUIRoot.I.Unbind();

        Debug.Log("[LocalPlayerController] Unbound");
    }

    public NetworkPlayer BoundPlayer => boundPlayer;
}
