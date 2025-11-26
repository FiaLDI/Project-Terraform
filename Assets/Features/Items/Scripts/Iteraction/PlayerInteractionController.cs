using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactMask;

    private InputSystem_Actions input;
    private PlayerUsageController usageController;

    private ItemObject targetItem;
    private IInteractable targetInteractable;

    private void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.Enable();

        input.Player.Interact.performed += ctx => TryInteract();

        usageController = GetComponent<PlayerUsageController>();
        if (usageController == null)
            Debug.LogWarning("PlayerInteractionController: usageController not found");
    }

    private void Start()
    {
        if (promptText == null)
        {
            var obj = GameObject.Find("InteractionPrompt_Text");
            if (obj != null)
                promptText = obj.GetComponent<TextMeshProUGUI>();
        }

        if (promptText != null)
            promptText.text = "";
    }

    private void Update()
    {
        ScanForInteractionTarget();
    }

    private void ScanForInteractionTarget()
    {
        targetItem = null;
        targetInteractable = null;

        // 1) Проверяем предметы в NearbyInteractables
        ItemObject item = NearbyInteractables.instance.GetBestItem(playerCamera);

        if (item != null && item.isWorldObject)
        {
            targetItem = item;
            ShowPrompt($"{item.itemData.itemName}\n[E] Подобрать");
            return;
        }

        // 2) Проверяем IInteractable (верстаки, двери и т.п.)
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask))
        {
            var interactObj = hit.collider.GetComponentInParent<IInteractable>();

            if (interactObj != null)
            {
                targetInteractable = interactObj;
                ShowPrompt($"[E] {interactObj.InteractionPrompt}");
                return;
            }
        }

        // 3) Ничего нет
        HidePrompt();
    }

    private void TryInteract()
    {
        if (targetItem != null)
        {
            // Подобрать предмет
            InventoryManager.instance.AddItem(targetItem.itemData, targetItem.quantity);
            NearbyInteractables.instance.Unregister(targetItem);
            Destroy(targetItem.gameObject);

            HidePrompt();
            return;
        }

        if (targetInteractable != null)
        {
            bool result = targetInteractable.Interact();

            if (result)
                HidePrompt();

            return;
        }
    }

    private void ShowPrompt(string text)
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(true);
        promptText.text = text;
    }

    private void HidePrompt()
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(false);
    }
}
