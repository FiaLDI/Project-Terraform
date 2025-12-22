using UnityEngine;
using UnityEngine.InputSystem;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player;
using Features.Input;

public sealed class PlayerInteractionController
    : MonoBehaviour, IInputContextConsumer
{
    private PlayerInputContext input;
    private InteractionResolver resolver;
    private INearbyInteractables nearby;

    private InputAction interactAction;
    private InputAction dropAction;

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
        dropAction     = player.FindAction("Drop", true);

        TrySubscribe();

        Debug.Log("[PlayerInteractionController] BindInput OK");
    }

    public void UnbindInput(PlayerInputContext ctx)
    {
        if (input != ctx)
            return;

        Unsubscribe();

        interactAction = null;
        dropAction = null;
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
        dropAction.performed     += OnDrop;

        subscribed = true;

        Debug.Log("[PlayerInteractionController] Actions subscribed");
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        interactAction.performed -= OnInteract;
        dropAction.performed     -= OnDrop;

        subscribed = false;

        Debug.Log("[PlayerInteractionController] Actions unsubscribed");
    }

    // ======================================================
    // ACTIONS
    // ======================================================

    private void OnInteract(InputAction.CallbackContext _)
    {
        TryInteract();
    }

    private void OnDrop(InputAction.CallbackContext _)
    {
        DropCurrentItem(false);
    }

    // ======================================================
    // INTERACTION
    // ======================================================

    private void TryInteract()
    {
        if (interactionBlocked || resolver == null || Camera.main == null)
            return;

        var target = resolver.Resolve(Camera.main);

        switch (target.Type)
        {
            case InteractionTargetType.Pickup:
                PickupItem(target.Pickup);
                break;

            case InteractionTargetType.Interactable:
                target.Interactable.Interact();
                break;
        }
    }

    private void PickupItem(NearbyItemPresenter presenter)
    {
        if (presenter == null)
            return;

        var inst = presenter.GetInstance();
        if (inst == null)
            return;

        LocalPlayerContext.Inventory?.Service.AddItem(inst);
        Destroy(presenter.gameObject);

        Debug.Log("[INTERACTION] Item picked up");
    }

    // ======================================================
    // DROP
    // ======================================================

    private void DropCurrentItem(bool dropFullStack)
    {
        var inventory = LocalPlayerContext.Inventory;
        if (inventory == null || Camera.main == null)
            return;

        var model = inventory.Model;
        var service = inventory.Service;

        if (model.hotbar.Count == 0)
            return;

        var slot = model.hotbar[model.selectedHotbarIndex];
        if (slot.item == null)
            return;

        var inst = slot.item;
        int dropAmount = dropFullStack ? inst.quantity : 1;

        var droppedInst = new ItemInstance(inst.itemDefinition, dropAmount);

        Vector3 pos = Camera.main.transform.position +
                      Camera.main.transform.forward * 1.2f;

        SpawnWorldItem(droppedInst, pos);
        service.TryRemove(inst.itemDefinition, dropAmount);
    }

    private void SpawnWorldItem(ItemInstance inst, Vector3 position)
    {
        var prefab = inst.itemDefinition.worldPrefab;
        if (prefab == null)
            return;

        var obj = Instantiate(prefab, position, Quaternion.identity);

        var holder = obj.GetComponent<ItemRuntimeHolder>() ??
                     obj.AddComponent<ItemRuntimeHolder>();
        holder.SetInstance(inst);

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
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
