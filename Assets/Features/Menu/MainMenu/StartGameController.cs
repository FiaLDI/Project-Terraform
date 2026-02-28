using TMPro;
using UnityEngine;

public sealed class StartGameController : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField portField;

    private AppModeController appMode;

    private void Start()
    {
        appMode = FindObjectOfType<AppModeController>();
    }

    public void OnHostPressed()
    {
        if (!ushort.TryParse(portField.text, out ushort port))
            return;

        appMode.StartServerAndClient(port);
    }

    public void OnJoinPressed()
    {
        if (!ushort.TryParse(portField.text, out ushort port))
            return;

        ClientConnectionController.I.Connect(ipField.text, port);
    }

    public void OnBackPressed()
    {
        MainMenuFSM.Instance.Switch(MainMenuStateId.CharacterSelect);
    }
}