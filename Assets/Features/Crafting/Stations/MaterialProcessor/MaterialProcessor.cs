using UnityEngine;

public class MaterialProcessor : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Переработать материалы";

    public bool Interact()
    {
        MaterialProcessorUI ui = UIRegistry.I?.Get<MaterialProcessorUI>();
        if (ui == null)
        {
            Debug.LogError("[MaterialProcessor] MaterialProcessorUI not registered", this);
            return false;
        }

        ui.Open();
        return true;
    }
}
