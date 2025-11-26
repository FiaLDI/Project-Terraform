using UnityEngine;

public abstract class InteractableUIObject : MonoBehaviour, IInteractable
{
    [Header("UI Canvas")]
    [SerializeField] protected Canvas uiCanvas;

    [Header("Prompt")]
    [SerializeField] protected string prompt = "Interact";

    public string InteractionPrompt => prompt;

    protected bool isOpen = false;

    protected virtual void Awake()
    {
        if (uiCanvas != null)
            uiCanvas.enabled = false;
    }

    public virtual bool Interact()
    {
        if (uiCanvas == null)
        {
            Debug.LogWarning($"[{name}] UI Canvas not assigned!");
            return false;
        }

        ToggleUI();
        return true;
    }

    protected virtual void ToggleUI()
    {
        isOpen = !isOpen;
        uiCanvas.enabled = isOpen;

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
