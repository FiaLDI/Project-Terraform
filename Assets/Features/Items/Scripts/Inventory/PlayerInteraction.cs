using UnityEngine;
using TMPro;
using Features.Camera.UnityIntegration;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;

    private TextMeshProUGUI interactionPromptText;

    private bool canInteract = false;
    private ItemObject targetItem;

    private Camera cam;


    private void Start()
    {
        // === AUTO CAMERA BIND ===
        cam = CameraRegistry.Instance?.CurrentCamera;
        if (cam == null)
        {
            Debug.LogWarning("[PlayerInteraction] Camera is not registered yet. Will auto-assign.");
        }

        // === AUTO UI PROMPT SEARCH ===
        if (interactionPromptText == null)
        {
            GameObject promptObj = GameObject.Find("InteractionPrompt_Text");
            if (promptObj != null)
                interactionPromptText = promptObj.GetComponent<TextMeshProUGUI>();
            else
                Debug.LogWarning("PlayerInteraction: объект 'InteractionPrompt_Text' не найден!");
        }

        if (interactionPromptText != null)
            interactionPromptText.text = "";
    }


    private void Update()
    {
        // === Always track latest camera ===
        if (cam == null || cam != CameraRegistry.Instance?.CurrentCamera)
        {
            cam = CameraRegistry.Instance?.CurrentCamera;
            if (cam == null) return; // camera still not available
        }

        // === 1) Items (NearbyInteractables) ===
        ItemObject item = NearbyInteractables.instance.GetBestItem(cam);
        targetItem = item;

        if (item != null && item.isWorldObject)
        {
            interactionPromptText.text =
                $"{item.itemData.itemName}\nНажмите [E] чтобы подобрать";

            canInteract = true;
            return;
        }

        // === 2) Generic interactables ===
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactionPromptText.text =
                    $"Нажмите [E] чтобы: {interactable.InteractionPrompt}";

                canInteract = true;
                targetItem = null;
                return;
            }
        }

        // === 3) Nothing found ===
        interactionPromptText.text = "";
        canInteract = false;
        targetItem = null;
    }


    // =====================================================================
    // INTERACTION LOGIC
    // =====================================================================

    public void TryInteract()
    {
        if (!canInteract) return;

        // Ensure camera
        cam = CameraRegistry.Instance?.CurrentCamera;
        if (cam == null) return;

        // === 1) Item pickup ===
        if (targetItem != null)
        {
            InventoryManager.instance.AddItem(
                targetItem.itemData,
                targetItem.quantity
            );

            NearbyInteractables.instance.Unregister(targetItem);
            Destroy(targetItem.gameObject);

            interactionPromptText.text = "";
            return;
        }

        // === 2) IInteractable objects ===
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
                interactionPromptText.text = "";
            }
        }
    }


    // =====================================================================
    // DROP ITEMS
    // =====================================================================

    public void DropCurrentItem(bool dropFullStack)
    {
        if (dropFullStack)
            InventoryManager.instance.DropFullStackFromSelectedSlot();
        else
            InventoryManager.instance.DropItemFromSelectedSlot();
    }
}
