using UnityEngine;

public class MaterialProcessor : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private MaterialProcessorUIController uiController;

    [Header("Prompt")]
    [SerializeField] private string prompt = "Открыть переработчик";

    public string InteractionPrompt => prompt;

    private bool isOpen = false;


    private void Start()
    {
        // Автопоиск UI в дочерних объектах
        if (uiController == null)
            uiController = GetComponentInChildren<MaterialProcessorUIController>(true);

        if (uiController == null)
        {
            Debug.LogError($"[MaterialProcessor] UI Controller NOT FOUND on {name}");
            return;
        }

        uiController.Init(this);
        uiController.SetVisible(false);
    }


    public bool Interact()
    {
        if (uiController == null)
        {
            Debug.LogError("[MaterialProcessor] Interact() called but uiController is NULL");
            return false;
        }

        ToggleUI();
        return true;
    }


    private void ToggleUI()
    {
        isOpen = !isOpen;

        uiController.SetVisible(isOpen);

        PlayerUsageController.InteractionLocked = isOpen;

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        Debug.Log(isOpen ? "[MaterialProcessor] UI opened" : "[MaterialProcessor] UI closed");
    }
}
