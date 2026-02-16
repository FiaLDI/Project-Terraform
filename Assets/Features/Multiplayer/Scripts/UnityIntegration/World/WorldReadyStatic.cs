using UnityEngine;
using FishNet;
using Multiplayer.Domain;

public sealed class WorldReadyStatic : MonoBehaviour
{
    private bool signaled;

    private void Start()
    {
        if (!InstanceFinder.IsServer)
            return;

        SignalReady();
    }

    private void SignalReady()
    {
        if (signaled)
            return;

        signaled = true;

        var flow = ServerCompositionRoot.I.Flow;

        // Если сервер ещё не в стадии загрузки сцены
        if (flow.CurrentState == ServerGameState.Starting ||
            flow.CurrentState == ServerGameState.LoadingScene)
        {
            flow.NotifySceneLoaded();
            flow.NotifyWorldPrepared();
        }

        Debug.Log("[WorldReadyStatic] Hub is RUNNING");
    }
}
