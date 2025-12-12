using UnityEngine;
using UnityEngine.InputSystem;
using Features.Interaction.Application;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;
using Features.Items.Domain;
using Features.Player;
using Features.Camera.UnityIntegration;

public class PlayerInteractionController : MonoBehaviour
{
    private bool interactionBlocked;

    private InteractionRayService rayService;
    private InteractionService interactionService;
    private InputSystem_Actions input;

    private INearbyInteractables nearby;

    private Camera cam;
    private CameraRayProvider rayProvider;

    // ======================================================
    // LIFECYCLE
    // ======================================================

    private void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.Enable();

        input.Player.Interact.performed += _ => TryInteract();
        input.Player.Drop.performed += _ => DropCurrentItem(false);
    }

    private void Start()
    {
        interactionService = new InteractionService();
        nearby = LocalPlayerContext.Get<NearbyInteractables>();

        if (nearby == null)
            Debug.LogWarning("[PlayerInteractionController] NearbyInteractables NOT FOUND");

        InitFromCameraRegistry();
    }

    private void OnDestroy()
    {
        if (CameraRegistry.Instance != null)
            CameraRegistry.Instance.OnCameraChanged -= OnCameraChanged;
    }

    // ======================================================
    // CAMERA INIT
    // ======================================================

    private void InitFromCameraRegistry()
    {
        var registry = CameraRegistry.Instance;
        if (registry == null)
        {
            Debug.LogError("[PlayerInteractionController] CameraRegistry NOT FOUND");
            enabled = false;
            return;
        }

        if (registry.CurrentCamera != null)
        {
            InitWithCamera(registry.CurrentCamera);
        }
        else
        {
            registry.OnCameraChanged += OnCameraChanged;
        }
    }

    private void OnCameraChanged(Camera camera)
    {
        if (camera == null) return;

        CameraRegistry.Instance.OnCameraChanged -= OnCameraChanged;
        InitWithCamera(camera);
    }

    private void InitWithCamera(Camera camera)
    {
        cam = camera;

        rayProvider = camera.GetComponentInParent<CameraRayProvider>();
        if (rayProvider == null)
        {
            Debug.LogError("[PlayerInteractionController] CameraRayProvider NOT FOUND");
            enabled = false;
            return;
        }

        rayService = new InteractionRayService(
            rayProvider,
            LayerMask.GetMask("Default", "Interactable", "Item"),
            LayerMask.GetMask("Player")
        );

        Debug.Log("[PlayerInteractionController] Initialized with camera");
    }

    // ======================================================
    // INTERACTION
    // ======================================================

    public void TryInteract()
    {
        if (interactionBlocked || cam == null || rayService == null)
            return;

        // 1️⃣ Pickup nearby item
        var best = nearby?.GetBestItem(cam);
        if (best != null)
        {
            PickupItem(best);
            return;
        }

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
        if (inventory == null || cam == null)
            return;

        var model = inventory.Model;
        var service = inventory.Service;

        var slot = model.hotbar[model.selectedHotbarIndex];
        if (slot.item == null)
            return;

        var inst = slot.item;

        Vector3 pos = cam.transform.position + cam.transform.forward * 1.2f;
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
