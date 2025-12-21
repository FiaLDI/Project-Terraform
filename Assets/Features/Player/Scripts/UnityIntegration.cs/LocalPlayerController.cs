using UnityEngine;
using Features.Input;
using Features.Player.UnityIntegration;
using Features.Camera.UnityIntegration;
using Features.Player;

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
        {
            Debug.LogError("[LocalPlayerController] PlayerInputContext missing");
        }
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
        if (boundPlayer == player)
            return;

        Unbind(boundPlayer);

        boundPlayer = player;

        // INPUT
        inputContext.Enable();
        InputModeManager.I.Bind(inputContext);
        InputModeManager.I.SetMode(InputMode.Gameplay);

        // PLAYER CONTROLLER
        player.Controller.BindInput(inputContext);

        // CAMERA — ВКЛЮЧАЕМ ТОЛЬКО У ЛОКАЛЬНОГО
        var cam = player.GetComponent<PlayerCameraController>();
        if (cam != null)
            cam.SetLocal(true);

        Debug.Log($"[LocalPlayerController] Bound to {player.name}");
    }

    public void Unbind(NetworkPlayer player)
    {
        if (boundPlayer != player || boundPlayer == null)
            return;

        boundPlayer.Controller.UnbindInput(inputContext);

        var cam = boundPlayer.GetComponent<PlayerCameraController>();
        if (cam != null)
            cam.SetLocal(false);

        inputContext.Disable();
        boundPlayer = null;

        Debug.Log("[LocalPlayerController] Unbound");
    }

    public NetworkPlayer BoundPlayer => boundPlayer;
}
