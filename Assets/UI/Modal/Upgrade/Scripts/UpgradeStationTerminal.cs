
using Features.Class.Net;
using Features.Player.UI;
using UnityEngine;

public sealed class UpgradeStationTerminal : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Открыть станцию улучшений";

    public bool Interact()
    {
        var ui = UIRegistry.I.Get<ProgressionTreeUI>();

        var player = PlayerUIRoot.I.BoundPlayer;
        var adapter = player.GetComponent<PlayerClassController>();

        var classConfig = adapter.currentClassOut;

        ui.Build(classConfig);
        if (ui == null)
        {
            Debug.LogError("[UpgradeStationTerminal] UI not registered");
            return false;
        }

        ui.Open();
        return true;
    }
}