using UnityEngine;
using UnityEngine.InputSystem;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player;

public class PlayerInteractionController : MonoBehaviour
{
    private bool interactionBlocked;

    private InteractionRayService rayService;
    private InteractionService interactionService;
    private InputSystem_Actions input;

    private INearbyInteractables nearby;

    // ======================================================
    // LIFECYCLE
    // ======================================================

    private void Awake()
    {
        interactionService = new InteractionService();

        input = new InputSystem_Actions();
        input.Player.Enable();

        input.Player.Interact.performed += _ => TryInteract();
        input.Player.Drop.performed     += _ => DropCurrentItem(false);
    }

    private void Start()
    {
        rayService = InteractionServiceProvider.Ray;
        if (rayService == null)
        {
            Debug.LogError("[PlayerInteractionController] InteractionRayService NOT FOUND");
            enabled = false;
            return;
        }

        nearby = LocalPlayerContext.Get<NearbyInteractables>();
        if (nearby == null)
            Debug.LogWarning("[PlayerInteractionController] NearbyInteractables NOT FOUND");
    }

    // ======================================================
    // INTERACTION
    // ======================================================

    public void TryInteract()
    {
        if (interactionBlocked || rayService == null)
            return;

        // 1️⃣ Pickup nearby item
        if (nearby != null && Camera.main != null)
        {
            var best = nearby.GetBestItem(Camera.main);
            if (best != null)
            {
                PickupItem(best);
                return;
            }
        }

        // 2️⃣ Ray interactable (станки, двери и т.п.)
        var hit = rayService.Raycast();
        if (interactionService.TryGetInteractable(hit, out var interactable))
        {
            interactable.Interact();
        }
    }

    // ======================================================
    // PICKUP
    // ======================================================

    private void PickupItem(NearbyItemPresenter presenter)
    {
        var inst = presenter.GetInstance();
        if (inst == null || inst.itemDefinition == null)
            return;

        var inventory = LocalPlayerContext.Inventory;
        if (inventory == null)
            return;

        // ✅ Переносим СУЩЕСТВУЮЩИЙ ItemInstance
        inventory.Service.AddItem(inst);

        nearby?.Unregister(presenter);
        Destroy(presenter.gameObject);
    }

    // ======================================================
    // DROP
    // ======================================================

    public void DropCurrentItem(bool dropFullStack)
    {
        var inventory = LocalPlayerContext.Inventory;
        if (inventory == null || Camera.main == null)
            return;

        var model   = inventory.Model;
        var service = inventory.Service;

        var slot = model.hotbar[model.selectedHotbarIndex];
        if (slot.item == null)
            return;

        var inst = slot.item;

        int dropAmount = dropFullStack ? inst.quantity : 1;

        var droppedInst = new ItemInstance(inst.itemDefinition, dropAmount);

        Vector3 pos = Camera.main.transform.position
                    + Camera.main.transform.forward * 1.2f;

        var prefab = inst.itemDefinition.worldPrefab;
        if (prefab == null)
            return;

        var dropped = Instantiate(prefab, pos, Quaternion.identity);

        var holder = dropped.GetComponent<ItemRuntimeHolder>()
                     ?? dropped.AddComponent<ItemRuntimeHolder>();

        holder.SetInstance(droppedInst);

        if (dropped.TryGetComponent<IItemModeSwitch>(out var mode))
            mode.SetWorldMode();

        service.TryRemove(inst.itemDefinition, dropAmount);
    }

    // ======================================================
    // STATE
    // ======================================================

    public void SetInteractionBlocked(bool blocked)
    {
        interactionBlocked = blocked;
    }
}
