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
        ipField.text = "localhost";
        portField.text = "7777";
        appMode = FindObjectOfType<AppModeController>();
    }

    public void OnHostPressed()
    {
        if (!ushort.TryParse(portField.text, out ushort port))
            return;

        LoadingScreenService.ShowHub("Launching server and loading hub...");
        appMode.StartServerAndClient(port);
    }

    public void OnJoinPressed()
    {
        if (!ushort.TryParse(portField.text, out ushort port))
            return;

        LoadingScreenService.ShowHub("Connecting to hub...");
        ClientConnectionController.I.Connect(ipField.text, port);
    }

    public void OnBackPressed()
    {
        MainMenuFSM.Instance.Switch(MainMenuStateId.ExpeditionSelect);
    }
}
