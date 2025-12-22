using Features.Input;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;
using Features.Inventory.Domain;
using Features.Items.UnityIntegration;
using Features.Player;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInteractionController :
    MonoBehaviour,
    IInputContextConsumer
{
    private PlayerInputContext input;
    private InteractionResolver resolver;
    private INearbyInteractables nearby;

    private InputAction interactAction;

    private bool interactionBlocked;
    private bool subscribed;

    // ======================================================
    // UNITY
    // ======================================================

    private void Start()
    {
        nearby = GetComponentInChildren<INearbyInteractables>();

        if (InteractionServiceProvider.Ray != null)
            InitResolver(InteractionServiceProvider.Ray);
        else
            InteractionServiceProvider.OnRayInitialized += InitResolver;
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    // ======================================================
    // INPUT BIND
    // ======================================================

    public void BindInput(PlayerInputContext ctx)
    {
        input = ctx;
        if (input == null)
            return;

        var player = input.Actions.Player;
        interactAction = player.FindAction("Interact", true);

        TrySubscribe();

        Debug.Log("[PlayerInteractionController] BindInput OK");
    }

    public void UnbindInput(PlayerInputContext ctx)
    {
        if (input != ctx)
            return;

        Unsubscribe();

        interactAction = null;
        input = null;
    }

    // ======================================================
    // SUBSCRIBE
    // ======================================================

    private void TrySubscribe()
    {
        if (subscribed || interactAction == null)
            return;

        interactAction.performed += OnInteract;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        if (interactAction != null)
            interactAction.performed -= OnInteract;

        subscribed = false;
    }

    // ======================================================
    // ACTIONS
    // ======================================================

    private void OnInteract(InputAction.CallbackContext _)
    {
        TryInteract();
    }

    // ======================================================
    // INTERACTION
    // ======================================================

    private void TryInteract()
    {
        if (interactionBlocked || resolver == null)
            return;

        var cam = Camera.main;
        if (cam == null)
            return;

        var target = resolver.Resolve(cam);

        switch (target.Type)
        {
            case InteractionTargetType.Pickup:
                PickupWorldItem(target.WorldItem);
                break;

            case InteractionTargetType.Interactable:
                target.Interactable?.Interact();
                break;
        }
    }

    private void PickupWorldItem(WorldItemNetwork worldItem)
    {
        var invNet = GetComponent<InventoryStateNetwork>();
        if (invNet == null)
            return;

        invNet.RequestInventoryCommand(new InventoryCommandData
        {
            Command = InventoryCommand.PickupWorldItem,
            WorldItemNetId = worldItem.NetworkObject.ObjectId
        });
    }


    // ======================================================
    // EXTERNAL
    // ======================================================

    public void SetInteractionBlocked(bool blocked)
    {
        interactionBlocked = blocked;
    }

    // ======================================================
    // RESOLVER
    // ======================================================

    private void InitResolver(InteractionRayService ray)
    {
        resolver = new InteractionResolver(ray, nearby);
        Debug.Log("[INTERACTION] Resolver initialized");
    }
}
