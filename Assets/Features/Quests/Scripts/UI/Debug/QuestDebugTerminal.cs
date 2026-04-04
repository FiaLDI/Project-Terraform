using UnityEngine;

public sealed class QuestDebugTerminal : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Открыть отладку квестов";

    public bool Interact()
    {
        InteractionDebug.Log("QuestDebugTerminal.Interact()", this);

        var ui = UIRegistry.I?.Get<QuestDebugUI>();
        if (ui == null)
        {
            Debug.LogError("[QuestDebugTerminal] QuestDebugUI not registered");
            return false;
        }

        ui.Open();
        return true;
    }
}
