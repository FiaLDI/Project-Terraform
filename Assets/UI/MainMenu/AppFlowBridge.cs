using Multiplayer.Application;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class AppFlowBridge : MonoBehaviour
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
        if (state == ClientGameState.Connecting)
        {
            // показать Loading UI
        }

        if (state == ClientGameState.Playing)
        {
            // скрыть меню
            MainMenuUIManager.Instance.gameObject.SetActive(false);
        }

        if (state == ClientGameState.Disconnected)
        {
            MainMenuUIManager.Instance.gameObject.SetActive(true);
            MainMenuFSM.Instance.Switch(MainMenuStateId.Play);
        }
    }
}