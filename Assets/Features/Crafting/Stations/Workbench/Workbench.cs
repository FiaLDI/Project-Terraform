using UnityEngine;

public class Workbench : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Открыть верстак";

    public bool Interact()
    {
        InteractionDebug.Log("Workbench.Interact() called", this);

        WorkbenchUI ui = UIRegistry.I?.Get<WorkbenchUI>();
        if (ui == null)
        {
            Debug.LogError("[Workbench] WorkbenchUI not registered", this);
            return false;
        }

        ui.Open();
        InteractionDebug.Log("Workbench UI opened", this);
        return true;
    }
}
