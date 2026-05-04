using FishNet;
using UnityEngine;

public sealed class CampaignShipUpgradeTerminal : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Улучшить корабль";
    [SerializeField] private string hostOnlyPrompt = "Только хост может улучшать корабль";

    public string InteractionPrompt =>
        InstanceFinder.IsHostStarted
            ? interactionPrompt
            : hostOnlyPrompt;

    public bool Interact()
    {
        if (!InstanceFinder.IsHostStarted)
        {
            InteractionDebug.Log("CampaignShipUpgradeTerminal.Interact() rejected: only host can open this", this);
            return false;
        }

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
