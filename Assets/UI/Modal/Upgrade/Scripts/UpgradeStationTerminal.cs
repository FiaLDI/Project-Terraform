using UnityEngine;
using Features.Player.UI;
using Features.Classes.Data;

public sealed class UpgradeStationTerminal : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Открыть станцию улучшений";

    public bool Interact()
    {
        var ui = UIRegistry.I.Get<ProgressionTreeUI>();
        if (ui == null)
        {
            Debug.LogError("[UpgradeStationTerminal] UI not registered");
            return false;
        }

        var player = PlayerUIRoot.I?.BoundPlayer;
        if (player == null)
            return false;

        // 🔥 берём из прогресса, а не из сервера
        var progress = PlayerProgressService.Instance.GetActiveCharacter();
        if (progress == null)
            return false;

        var library = Resources.Load<PlayerClassLibrarySO>("Databases/PlayerClassLibrary");
        if (library == null)
        {
            Debug.LogError("[UpgradeStationTerminal] ClassLibrary not found");
            return false;
        }

        var classConfig = library.FindById(progress.classId);
        if (classConfig == null)
        {
            Debug.LogError($"[UpgradeStationTerminal] Class '{progress.classId}' not found");
            return false;
        }

        ui.Build(classConfig);
        ui.Open();

        return true;
    }
}
