using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;
    //[SerializeField] 
    private TextMeshProUGUI interactionPromptText;

    private RaycastHit lastHit;
    private bool canInteract = false;
    private ItemObject targetItem;

    private void Start()
    {
        // === Автопоиск объекта InteractionPrompt_Text ===
        if (interactionPromptText == null)
        {
            GameObject promptObj = GameObject.Find("InteractionPrompt_Text");
            if (promptObj != null)
                interactionPromptText = promptObj.GetComponent<TextMeshProUGUI>();
            else
                Debug.LogWarning("PlayerInteraction: объект 'InteractionPrompt_Text' не найден на сцене!");
        }

        interactionPromptText.text = "";
    }

    private void Update()
    {
        // === 1) Предметы (как раньше) ===
        ItemObject item = NearbyInteractables.instance.GetBestItem(playerCamera);
        targetItem = item;

        if (item != null && item.isWorldObject)
        {
            interactionPromptText.text =
                $"{item.itemData.itemName}\nНажмите [E] чтобы подобрать";
            canInteract = true;
            return;
        }

        // === 2) Станки / двери / любые IInteractable ===
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactionPromptText.text =
                    $"Нажмите [E] чтобы: {interactable.InteractionPrompt}";
                canInteract = true;
                targetItem = null; // чтобы TryInteract понял что это НЕ ItemObject
                return;
            }
        }

        // === 3) Ничего нет ===
        interactionPromptText.text = "";
        canInteract = false;
        targetItem = null;
    }


    public void TryInteract()
    {
        Debug.Log(">>> TryInteract() called");

        if (!canInteract) return;

        // === 1) Если это предмет ===
        if (targetItem != null)
        {
            InventoryManager.instance.AddItem(
                targetItem.itemData,
                targetItem.quantity);

            NearbyInteractables.instance.Unregister(targetItem);
            Destroy(targetItem.gameObject);

            interactionPromptText.text = "";
            return;
        }

        // === 2) Если это IInteractable объект ===
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

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


    public void DropCurrentItem(bool dropFullStack)
    {
        if (dropFullStack)
            InventoryManager.instance.DropFullStackFromSelectedSlot();
        else
            InventoryManager.instance.DropItemFromSelectedSlot();
    }
}
