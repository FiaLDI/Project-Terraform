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
            case ClientGameState.Authenticating:
            case ClientGameState.WaitingForSpawn:
                MainMenuUIManager.Instance.gameObject.SetActive(true);
                break;

            case ClientGameState.Playing:
                MainMenuUIManager.Instance.gameObject.SetActive(false);
                break;

            case ClientGameState.Disconnected:
                MainMenuUIManager.Instance.gameObject.SetActive(true);
                MainMenuFSM.Instance.Switch(MainMenuStateId.Play);
                break;
        }
    }
}