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
        ItemObject item = NearbyInteractables.instance.GetBestItem(playerCamera);
        targetItem = item;

        if (item != null && item.isWorldObject == true)
        {
            interactionPromptText.text = $"{item.itemData.itemName}\nНажмите [E] чтобы подобрать";
            canInteract = true;
        }
        else
        {
            interactionPromptText.text = "";
            canInteract = false;
        }
    }

    public void TryInteract()
    {
        Debug.Log(">>> TryInteract() called");
        if (!canInteract) return;

        // Взаимодействие с объектом
        // здесь больше НЕ должно быть проверки lastHit

        // Подбор предмета
        if (targetItem != null)
        {
            InventoryManager.instance.AddItem(
                targetItem.itemData,
                targetItem.quantity);
            NearbyInteractables.instance.Unregister(targetItem);
            Destroy(targetItem.gameObject);
            interactionPromptText.text = "";
        }
    }
    // Метод для выброса предмета 
    public void DropCurrentItem(bool dropFullStack)
    {
        if (dropFullStack)
            InventoryManager.instance.DropFullStackFromSelectedSlot();
        else
            InventoryManager.instance.DropItemFromSelectedSlot();
    }
}
