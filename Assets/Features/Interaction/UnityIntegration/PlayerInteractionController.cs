using Features.Game;
using Features.Input;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;
using Features.Inventory.Domain;
using Features.Items.UnityIntegration;
using Features.Player;
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
    private const double INTERACT_COOLDOWN = 0.2;

    // ================= UNITY =================

    private void Start()
    {
        // сразу пробуем взять player и nearby
        var player = BootstrapRoot.I?.LocalPlayer;
        if (player != null)
        {
            nearby = player.GetComponent<INearbyInteractables>();
            Debug.Log($"[PIC] Start: nearby={(nearby != null ? nearby.ToString() : "NULL")}", this);
        }

        if (InteractionServiceProvider.Ray != null)
            InitResolver(InteractionServiceProvider.Ray);
        else
            InteractionServiceProvider.OnRayInitialized += InitResolver;
    }

    private void OnEnable()  => TrySubscribe();
    private void OnDisable() => Unsubscribe();
    private void OnDestroy() => Unsubscribe();

    // ================= INPUT BIND =================

    public void BindInput(PlayerInputContext ctx)
    {
        input = ctx;
        if (input == null)
            return;

        var playerMap = input.Actions.Player;
        interactAction = playerMap.FindAction("Interact", true);

        TrySubscribe();

        Debug.Log("[PlayerInteractionController] BindInput OK", this);
    }

    public void UnbindInput(PlayerInputContext ctx)
    {
        if (input != ctx)
            return;

        Unsubscribe();
        interactAction = null;
        input = null;
    }

    // ================= SUBSCRIBE =================

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

    // ================= ACTIONS =================

    private void OnInteract(InputAction.CallbackContext _)
    {
        TryInteract();
    }

    // ================= INTERACTION =================

    private void TryInteract()
    {
        var player = BootstrapRoot.I?.LocalPlayer;
        if (player == null)
        {
            Debug.LogWarning("[PlayerInteractionController] TryInteract: LocalPlayer is NULL", this);
            return;
        }

        // гарантированно есть nearby
        if (nearby == null)
        {
            nearby = player.GetComponent<INearbyInteractables>();
            Debug.Log($"[PIC] TryInteract: reacquired nearby={(nearby != null ? nearby.ToString() : "NULL")}", this);

            // если резолвер уже был создан с null-nearby – пересоздаём
            if (resolver != null && nearby != null)
                resolver = new InteractionResolver(InteractionServiceProvider.Ray, nearby);
        }

        double currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastInteractTime < INTERACT_COOLDOWN)
        {
            Debug.Log("[PlayerInteractionController] TryInteract blocked by cooldown", this);
            return;
        }
        lastInteractTime = currentTime;

        if (interactionBlocked || resolver == null || nearby == null)
        {
            Debug.Log("[PlayerInteractionController] TryInteract: resolver or nearby is NULL", this);
            return;
        }

        var cam = UnityEngine.Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[PlayerInteractionController] TryInteract: Camera.main is NULL", this);
            return;
        }

        var target = resolver.Resolve(cam);
        Debug.Log($"[PlayerInteractionController] TryInteract resolved: {target.Type}", this);

        switch (target.Type)
        {
            case InteractionTargetType.Pickup:
                Debug.Log($"[PlayerInteractionController] PICKUP action triggered, item={target.WorldItem?.name}", this);
                PickupWorldItem(player, target.WorldItem);
                break;

            case InteractionTargetType.Interactable:
                Debug.Log("[PlayerInteractionController] INTERACTABLE action triggered", this);
                target.Interactable?.Interact();
                break;
        }
    }

    private void PickupWorldItem(GameObject player, WorldItemNetwork worldItem)
    {
        if (worldItem == null)
        {
            Debug.LogWarning("[PlayerInteractionController] PickupWorldItem: worldItem is NULL", this);
            return;
        }

        var invNet = player.GetComponent<InventoryStateNetwork>();
        if (invNet == null)
        {
            Debug.LogError("[PlayerInteractionController] PickupWorldItem: InventoryStateNetwork NOT FOUND on player", this);
            return;
        }

        // берем данные из синхронизированных свойств
        string itemId = worldItem.ItemId;
        int qty       = worldItem.Quantity;
        int lvl       = worldItem.Level;

        Debug.Log(
            $"[PlayerInteractionController] PickupWorldItem → send command | " +
            $"ItemId={itemId}, Qty={qty}, Level={lvl}, NetId={worldItem.NetworkObject.ObjectId}",
            this
        );

        var cmd = new InventoryCommandData
        {
            Command        = InventoryCommand.PickupWorldItem,
            WorldItemNetId = (uint)worldItem.NetworkObject.ObjectId,
            ItemId         = itemId,
            PickupQuantity = qty,
            PickupLevel    = lvl
        };

        invNet.RequestInventoryCommand(cmd);
    }

    // ================= EXTERNAL =================

    public void SetInteractionBlocked(bool blocked)
    {
        interactionBlocked = blocked;
    }

    // ================= RESOLVER =================

    private void InitResolver(InteractionRayService ray)
    {
        // здесь nearby уже должен быть установлен
        resolver = new InteractionResolver(ray, nearby);
        Debug.Log($"[INTERACTION] Resolver initialized, nearby={(nearby != null ? nearby.ToString() : "NULL")}", this);
    }
}
