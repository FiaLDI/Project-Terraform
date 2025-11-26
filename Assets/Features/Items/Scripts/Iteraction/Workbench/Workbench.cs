using UnityEngine;

public class Workbench : MonoBehaviour, IInteractable
{
    [Header("UI")]
    public WorkbenchUIController uiController;

    [Header("Prompt")]
    [SerializeField] private string prompt = "Открыть верстак";

    public string InteractionPrompt => prompt;

    private bool isOpen = false;


    private void Start()
    {
        // Если UI не назначен вручную — ищем автоматически
        if (uiController == null)
        {
            uiController = FindUIController();
        }

        if (uiController == null)
        {
            Debug.LogError($"[Workbench] UI Controller NOT FOUND for {name}");
            return;
        }

        uiController.Init(this);
        uiController.SetVisible(false);
    }


    public bool Interact()
    {
        if (uiController == null)
        {
            Debug.LogError($"[Workbench] Cannot interact, UIController == null");
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

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
    }


    // ================================
    // 🔍 АВТОПОИСК UI
    // ================================
    private WorkbenchUIController FindUIController()
    {
        // 1) Ищем в дочерних объектах
        var ui = GetComponentInChildren<WorkbenchUIController>(true);
        if (ui != null) return ui;

        // 2) Ищем у родителя
        ui = GetComponentInParent<WorkbenchUIController>(true);
        if (ui != null) return ui;

        // 3) Ищем во всей сцене (главный Canvas)
        ui = FindAnyObjectByType<WorkbenchUIController>();
        if (ui != null) return ui;

        return null;
    }
}
