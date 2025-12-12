using UnityEngine;

public class ModeSelectController : MonoBehaviour
{
    public void OnSingleplayerPressed()
        => MainMenuFSM.Instance.Switch(MainMenuStateId.CharacterSelect);

    public void OnMultiplayerPressed()
        => MainMenuFSM.Instance.Switch(MainMenuStateId.MultiplayerPlaceholder);

    public void OnBackPressed()
        => MainMenuFSM.Instance.Switch(MainMenuStateId.Play);
}
