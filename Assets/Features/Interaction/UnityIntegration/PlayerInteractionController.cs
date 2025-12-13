using UnityEngine;
using UnityEngine.InputSystem;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;
using Features.Items.Domain;
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
        input.Player.Drop.performed += _ => DropCurrentItem(false);
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

        // 2️⃣ Ray interactable
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
        var inst = presenter.instance;
        if (inst == null || inst.itemDefinition == null)
            return;

        var inventory = LocalPlayerContext.Inventory;
        inventory?.Service.AddItem(
            new ItemInstance(inst.itemDefinition, inst.quantity)
        );

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

        var model = inventory.Model;
        var service = inventory.Service;

        var slot = model.hotbar[model.selectedHotbarIndex];
        if (slot.item == null)
            return;

        var inst = slot.item;

        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 1.2f;
        var prefab = inst.itemDefinition.worldPrefab;

        if (prefab != null)
        {
            var dropped = Instantiate(prefab, pos, Quaternion.identity);
            if (dropped.TryGetComponent<NearbyItemPresenter>(out var presenter))
            {
                presenter.Initialize(
                    inst.itemDefinition,
                    dropFullStack ? inst.quantity : 1
                );
            }
        }

        service.TryRemove(
            inst.itemDefinition,
            dropFullStack ? inst.quantity : 1
        );
    }

    // ======================================================
    // STATE
    // ======================================================

    public void SetInteractionBlocked(bool blocked)
    {
        interactionBlocked = blocked;
    }
}
