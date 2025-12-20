using UnityEngine;
using Features.Player;
using Features.Input;

public sealed class LocalPlayerController : MonoBehaviour
{
    public static LocalPlayerController I { get; private set; }

    private PlayerInputContext inputContext;
    private GameObject boundPlayer;

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

    private void Start()
    {
        if (InputModeManager.I == null)
        {
            Debug.LogError("[LocalPlayerController] InputModeManager not found");
            return;
        }

        InputModeManager.I.Bind(inputContext);
    }

    public void Bind(GameObject player)
    {
        Debug.Log("LocalPlayerController.Bind CALLED", player);
        boundPlayer = player;

        var consumers = player.GetComponentsInChildren<IInputContextConsumer>(true);
        foreach (var consumer in consumers)
        {
            Debug.Log("LocalPlayerController.consumer CALLED");
            consumer.BindInput(inputContext);
        }

        Debug.Log("[LocalPlayerController] Input bound to Player");
    }

    public GameObject BoundPlayer => boundPlayer;
}
