using UnityEngine;
using Features.Input;
using Features.Player.UnityIntegration;
using Features.Camera.UnityIntegration;
using Features.Player;
using Features.Player.UI;
using Features.Interaction.UnityIntegration;
using Features.Stats.UnityIntegration;

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
        NetworkPlayer.OnLocalPlayerSpawned += Bind;
    }

    private void OnDisable()
    {
        NetworkPlayer.OnLocalPlayerSpawned -= Bind;
    }

    // ======================================================
    // BIND / UNBIND
    // ======================================================

    public void Bind(NetworkPlayer player)
    {
        // Защита: вдруг по ошибке прилетит не-owner
        if (!player.IsOwner)
        {
            Debug.LogWarning("[LocalPlayerController] Bind called for non-owner player, ignoring", player);
            return;
        }

        if (boundPlayer == player)
            return;

        Debug.Log($"[LocalPlayerController] Bind to {player.name}");

        // unbind старого локального
        if (boundPlayer != null)
            Unbind(boundPlayer);

        boundPlayer = player;

        if (inputContext != null)
        {
            inputContext.Enable();
            InputModeManager.I.Bind(inputContext);
            InputModeManager.I.SetMode(InputMode.Gameplay);
        }

        player.Controller.BindInput(inputContext);

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

        // Статы можно инициализировать тут, но у тебя они уже Init в NetworkPlayer.
        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.Init();
        else
            Debug.LogError("[LocalPlayerController] PlayerStats not found on player!");

        if (PlayerUIRoot.I != null)
            PlayerUIRoot.I.Bind(player.gameObject);

        Debug.Log($"[LocalPlayerController] Bound to {player.name}");
    }

    public void Unbind(NetworkPlayer player)
    {
        if (boundPlayer == null || boundPlayer != player)
            return;

        var consumers = this.GetComponents<IInputContextConsumer>();
        foreach (var c in consumers)
            c.UnbindInput(inputContext);

        boundPlayer.Controller.UnbindInput(inputContext);

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
