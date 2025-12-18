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
    }

    // ======================================================
    // INTERACTION
    // ======================================================

    public void TryInteract()
    {
        if (interactionBlocked || rayService == null)
            return;

        // 1️⃣ Pickup world item
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
        var inst = presenter.GetInstance();
        if (inst == null || inst.itemDefinition == null)
            return;

        var inventory = LocalPlayerContext.Inventory;
        if (inventory == null)
            return;

        inventory.Service.AddItem(inst);

        // 🔥 ВАЖНО: просто уничтожаем world prefab
        Destroy(presenter.gameObject);
    }

    // ======================================================
    // DROP (быстрый дроп из хотбара)
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

        SpawnWorldItem(droppedInst, pos);

        service.TryRemove(inst.itemDefinition, dropAmount);
    }

    // ======================================================
    // WORLD SPAWN
    // ======================================================

    private void SpawnWorldItem(ItemInstance inst, Vector3 position)
    {
        var prefab = inst.itemDefinition.worldPrefab;
        if (prefab == null)
            return;

        var obj = Instantiate(prefab, position, Quaternion.identity);

        var holder = obj.GetComponent<ItemRuntimeHolder>()
                    ?? obj.AddComponent<ItemRuntimeHolder>();
        holder.SetInstance(inst);

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    // ======================================================
    // STATE
    // ======================================================

    public void SetInteractionBlocked(bool blocked)
    {
        interactionBlocked = blocked;
    }
}
