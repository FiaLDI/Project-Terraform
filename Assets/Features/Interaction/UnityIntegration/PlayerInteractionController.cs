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

    private PlayerInputContext input;
    private INearbyInteractables nearby;

    private bool subscribed;

    // ======================================================
    // LIFECYCLE
    // ======================================================

    private void Awake()
    {
        interactionService = new InteractionService();
    }

    private void OnEnable()
    {
        if (input == null)
            input = GetComponent<PlayerInputContext>();

        if (input == null)
        {
            Debug.LogError(
                $"{nameof(PlayerInteractionController)}: PlayerInputContext not found",
                this);
            return;
        }

        var p = input.Actions.Player;

        p.Interact.performed += OnInteract;
        p.Drop.performed     += OnDrop;

        subscribed = true;
    }

    private void OnDisable()
    {
        if (!subscribed || input == null)
            return;

        var p = input.Actions.Player;

        p.Interact.performed -= OnInteract;
        p.Drop.performed     -= OnDrop;

        subscribed = false;
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

    private void OnInteract(InputAction.CallbackContext _)
    {
        TryInteract();
    }

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
    // DROP
    // ======================================================

    private void OnDrop(InputAction.CallbackContext _)
    {
        DropCurrentItem(false);
    }

    public void DropCurrentItem(bool dropFullStack)
    {
        var inventory = LocalPlayerContext.Inventory;
        if (inventory == null || Camera.main == null)
            return;

        var model   = inventory.Model;
        var service = inventory.Service;

        if (model.hotbar.Count == 0)
            return;

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

    private void PickupItem(NearbyItemPresenter presenter)
    {
        if (presenter == null)
            return;

        var inst = presenter.GetInstance();
        if (inst == null || inst.itemDefinition == null)
            return;

        var inventory = LocalPlayerContext.Inventory;
        if (inventory == null)
            return;

        inventory.Service.AddItem(inst);

        Destroy(presenter.gameObject);
    }

    // ======================================================
    // STATE
    // ======================================================

    public void SetInteractionBlocked(bool blocked)
    {
        interactionBlocked = blocked;
    }
}
