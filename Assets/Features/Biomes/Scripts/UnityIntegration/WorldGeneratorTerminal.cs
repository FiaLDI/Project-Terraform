using UnityEngine;
using Features.World.UI;

public sealed class WorldGeneratorTerminal : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Открыть генератор мира";

    public bool Interact()
    {
        InteractionDebug.Log("WorldGeneratorTerminal.Interact()", this);

        var ui = UIRegistry.I?.Get<WorldGeneratorUI>();
        if (ui == null)
        {
            Debug.LogError("[WorldGeneratorTerminal] WorldGeneratorUI not registered");
            return false;
        }

        ui.Open();
        return true;
    }
}
