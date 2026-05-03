using UnityEngine;

public sealed class CampaignShipUpgradeTerminal : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Улучшить корабль";

    public string InteractionPrompt => interactionPrompt;

    public bool Interact()
    {
        var ui = UIRegistry.I?.Get<CampaignShipUpgradeUI>();
        if (ui == null)
        {
            Debug.LogError("[CampaignShipUpgradeTerminal] CampaignShipUpgradeUI not registered", this);
            return false;
        }

        ui.Open();
        return true;
    }
}
