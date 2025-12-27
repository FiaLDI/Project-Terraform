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
    private double lastInteractTime = 0;
    private double lastPickupTime = 0;
    private const double INTERACT_COOLDOWN = 0.2;
    private WorldItemNetwork lastPickedUpItem;

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
        double currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastInteractTime < INTERACT_COOLDOWN)
        {
            Debug.Log("[PlayerInteractionController] TryInteract blocked by cooldown", this);
            return;
        }
        lastInteractTime = currentTime;
        if (interactionBlocked || resolver == null)
            return;

        var cam = Camera.main;
        if (cam == null)
            return;

        var target = resolver.Resolve(cam);
        
        // ✅ ЗАЩИТА: Логируй что было разрешено
        Debug.Log($"[PlayerInteractionController] TryInteract resolved: {target.Type}", this);

        switch (target.Type)
        {
            case InteractionTargetType.Pickup:
                Debug.Log($"[PlayerInteractionController] PICKUP action triggered, item={target.WorldItem?.name}", this);
                PickupWorldItem(target.WorldItem);
                break;

            case InteractionTargetType.Interactable:
                Debug.Log($"[PlayerInteractionController] INTERACTABLE action triggered", this);
                target.Interactable?.Interact();
                break;
        }
    }

    private void PickupWorldItem(WorldItemNetwork worldItem)
    {
        if (worldItem == null)
        {
            Debug.LogWarning("[PlayerInteractionController] PickupWorldItem: worldItem is NULL", this);
            return;
        }

        // ✅ ЗАЩИТА от дублирования: проверь что это не тот же предмет за одну секунду
        if (worldItem == lastPickedUpItem && (Time.realtimeSinceStartup - lastPickupTime) < 0.5f)
        {
            Debug.Log($"[PlayerInteractionController] Duplicate pickup attempt blocked: {worldItem.name}", this);
            return;
        }
        
        lastPickedUpItem = worldItem;
        lastPickupTime = Time.realtimeSinceStartup;

        var invNet = GetComponent<InventoryStateNetwork>();
        if (invNet == null)
            return;

        // ✅ Получи кэшированный экземпляр
        var cachedInst = worldItem.GetCachedInstance();
        
        string itemId = cachedInst?.itemDefinition?.id ?? "UNKNOWN";
        int qty = cachedInst?.quantity ?? worldItem.Quantity;
        int lvl = cachedInst?.level ?? worldItem.Level;

        Debug.Log($"[PlayerInteractionController] PickupWorldItem: ItemId={itemId}, Qty={qty}, Level={lvl}, NetId={worldItem.NetworkObject.ObjectId}", this);

        invNet.RequestInventoryCommand(new InventoryCommandData
        {
            Command = InventoryCommand.PickupWorldItem,
            WorldItemNetId = (uint)worldItem.NetworkObject.ObjectId,
            ItemId = itemId,
            PickupQuantity = qty,
            PickupLevel = lvl
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
