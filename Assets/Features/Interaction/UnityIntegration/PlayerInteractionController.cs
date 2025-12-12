using UnityEngine;
using UnityEngine.InputSystem;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;

public class PlayerInteractionController : MonoBehaviour
{
    private InteractionRayService rayService;
    private InteractionService interactionService;

    private InputSystem_Actions input;

    private void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.Enable();

        input.Player.Interact.performed += ctx => TryInteract();
        input.Player.Drop.performed     += ctx => DropCurrentItem(false);
    }

    private void Start()
    {
        var provider = FindObjectOfType<CameraRayProvider>();

        rayService = new InteractionRayService(
            provider,
            LayerMask.GetMask("Default", "Interactable", "Item"),
            LayerMask.GetMask("Player")
        );

        interactionService = new InteractionService();
    }

    // ============================================================
    //  PUBLIC → теперь доступно для InventoryInputHandler
    // ============================================================

    public void TryInteract()
    {
        Camera cam = FindObjectOfType<CameraRayProvider>()
                        .GetComponentInChildren<Camera>();

        // --- ITEM PICKUP ---
        var bestItem = NearbyInteractables.instance.GetBestItem(cam);

        if (bestItem != null)
        {
            InventoryManager.instance.AddItem(bestItem.itemData, bestItem.quantity);
            NearbyInteractables.instance.Unregister(bestItem);
            Destroy(bestItem.gameObject);
            return;
        }

        // --- IInteractable ---
        var hit = rayService.Raycast();

        if (interactionService.TryGetInteractable(hit, out var interactable))
        {
            interactable.Interact();
        }
    }

    // ============================================================
    //  PUBLIC DropCurrentItem — необходимо для старой логики
    // ============================================================

    public void DropCurrentItem(bool dropFullStack)
    {
        if (dropFullStack)
            InventoryManager.instance.DropFullStackFromSelectedSlot();
        else
            InventoryManager.instance.DropItemFromSelectedSlot();
    }
}
