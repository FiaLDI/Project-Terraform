using UnityEngine;

public class ImprovementStation : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private ImprovementStationUIController uiController;

    [Header("Prompt")]
    [SerializeField] private string prompt = "Открыть станцию улучшений";

    public string InteractionPrompt => prompt;

    private bool isOpen = false;


    private void Start()
    {
        // Ищем UI автоматически
        if (uiController == null)
            uiController = GetComponentInChildren<ImprovementStationUIController>(true);

        if (uiController == null)
        {
            Debug.LogError($"[ImprovementStation] UI Controller NOT FOUND on {name}");
            return;
        }

        uiController.Init(this);
        uiController.SetVisible(false);
    }


    public bool Interact()
    {
        if (uiController == null)
        {
            Debug.LogError("[ImprovementStation] Interact() called but uiController is NULL");
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

        Debug.Log(isOpen ? "[ImprovementStation] UI opened" : "[ImprovementStation] UI closed");
    }
}
