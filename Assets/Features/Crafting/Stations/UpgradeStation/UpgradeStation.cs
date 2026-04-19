using UnityEngine;

public class UpgradeStation : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Улучшить предмет";

    public bool Interact()
    {
        UpgradeStationUI ui = UIRegistry.I?.Get<UpgradeStationUI>();
        if (ui == null)
        {
            Debug.LogError("[UpgradeStation] UpgradeStationUI not registered", this);
            return false;
        }

        ui.Open();
        return true;
    }
}
