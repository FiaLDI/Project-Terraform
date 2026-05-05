using FishNet;
using UnityEngine;

public sealed class WorldGeneratorTerminal : MonoBehaviour, IInteractable
{
    public string InteractionPrompt =>
        InstanceFinder.IsHostStarted
            ? "Открыть генератор мира"
            : "Только хост может открыть генератор мира";

    public bool Interact()
    {
        if (!InstanceFinder.IsHostStarted)
        {
            InteractionDebug.Log("WorldGeneratorTerminal.Interact() rejected: only host can open this", this);
            return false;
        }

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
