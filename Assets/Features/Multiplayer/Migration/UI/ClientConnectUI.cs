using Multiplayer.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ClientConnectUI : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField portField;

    [Header("Buttons")]
    [SerializeField] private Button connectButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private Button startLocalButton;   // ← добавили

    [Header("Status")]
    [SerializeField] private TMP_Text statusText;

    private ClientConnectionController controller;
    private ClientGameFlow flow;
    private AppModeController appMode;

    private void Start()
    {
        controller = ClientConnectionController.I;
        flow = controller.GetFlow();
        appMode = FindObjectOfType<AppModeController>();

        connectButton.onClick.AddListener(OnConnectClicked);
        disconnectButton.onClick.AddListener(OnDisconnectClicked);
        startLocalButton.onClick.AddListener(OnStartLocalClicked);

        flow.OnStateChanged += OnStateChanged;

        UpdateUI(flow.CurrentState);
    }

    private void OnDestroy()
    {
        if (flow != null)
            flow.OnStateChanged -= OnStateChanged;
    }

    // ======================================
    // BUTTONS
    // ======================================

    private void OnConnectClicked()
    {
        if (!ushort.TryParse(portField.text, out ushort port))
        {
            statusText.text = "Invalid port";
            return;
        }

        controller.Connect(ipField.text, port);
    }

    private void OnDisconnectClicked()
    {
        controller.Disconnect();
    }

   private void OnStartLocalClicked()
    {
        startLocalButton.interactable = false;

        if (!ushort.TryParse(portField.text, out ushort port))
        {
            statusText.text = "Invalid port";
            startLocalButton.interactable = true;
            return;
        }

        appMode.StartServerAndClient(port);
    }


    // ======================================
    // STATE HANDLING
    // ======================================

    private void OnStateChanged(ClientGameState state)
    {
        UpdateUI(state);
    }

    private void UpdateUI(ClientGameState state)
    {
        switch (state)
        {
            case ClientGameState.Disconnected:
                statusText.text = "Disconnected";
                connectButton.interactable = true;
                startLocalButton.interactable = true;
                disconnectButton.interactable = false;
                break;

            case ClientGameState.Connecting:
                statusText.text = "Connecting...";
                connectButton.interactable = false;
                startLocalButton.interactable = false;
                disconnectButton.interactable = false;
                break;

            case ClientGameState.Authenticating:
                statusText.text = "Authenticating...";
                break;

            case ClientGameState.WaitingForSpawn:
                statusText.text = "Spawning player...";
                break;

            case ClientGameState.Playing:
                statusText.text = "Connected ✔";
                disconnectButton.interactable = true;
                break;

            case ClientGameState.Disconnecting:
                statusText.text = "Disconnecting...";
                break;
        }
    }
}
