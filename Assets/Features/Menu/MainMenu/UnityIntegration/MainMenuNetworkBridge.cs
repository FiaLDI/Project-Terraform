using Multiplayer.Application;
using UnityEngine;

public sealed class MainMenuNetworkBridge : MonoBehaviour
{
    private ClientGameFlow flow;

    private void Start()
    {
        flow = ClientConnectionController.I.GetFlow();
        flow.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (flow != null)
            flow.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(ClientGameState state)
    {
        switch (state)
        {
            case ClientGameState.Connecting:
                MainMenuUIManager.Instance.Show(MainMenuStateId.MultiplayerPlaceholder);
                break;

            case ClientGameState.Playing:
                UnityEngine.SceneManagement.SceneManager.LoadScene("HubScene");
                break;

            case ClientGameState.Disconnected:
                MainMenuUIManager.Instance.Show(MainMenuStateId.Play);
                break;
        }
    }
}