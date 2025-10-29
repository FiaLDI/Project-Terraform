using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    private RaycastHit lastHit;
    private bool canInteract = false;
    private ItemObject targetItem;

    private void Start()
    {
        interactionPromptText.text = "";
    }

    private void Update()
    {
        // Луч от центра экрана
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out lastHit, interactionDistance))
        {
            IInteractable interactable = lastHit.collider.GetComponent<IInteractable>();
            targetItem = lastHit.collider.GetComponent<ItemObject>();

            // Подсказка для IInteractable
            if (interactable != null)
            {
                interactionPromptText.text = interactable.InteractionPrompt;
                canInteract = true;
                return;
            }

            // Подсказка для предмета
            if (targetItem != null)
            {
                interactionPromptText.text = "Нажмите [E], чтобы подобрать";
                canInteract = true;
                return;
            }
        }

        // Если ничего не нашли — убираем подсказку
        canInteract = false;
        targetItem = null;
        interactionPromptText.text = "";
    }

    // Метод, вызываемый из PlayerController 
    public void TryInteract()
    {
        Debug.Log(">>> TryInteract() called");
        if (!canInteract) return;

        // Взаимодействие с объектом
        if (lastHit.collider == null) return;

        var interactable = lastHit.collider.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact();
            interactionPromptText.text = "";
            return;
        }

        // Подбор предмета
        if (targetItem != null)
        {
            InventoryManager.instance.AddItem(
                targetItem.itemData,
                targetItem.quantity);
                //targetItem.ammoInMagazine);

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
