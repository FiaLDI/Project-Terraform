using System;

namespace Multiplayer.Application
{
    public enum ClientGameState
    {
        Disconnected,
        Connecting,
        Connected,
        Authenticating,
        WaitingForSpawn,
        Playing,
        Disconnecting
    }

    public sealed class ClientGameFlow
    {
        public ClientGameState CurrentState { get; private set; }
            = ClientGameState.Disconnected;

        public event Action<ClientGameState> OnStateChanged;

        public void StartConnect()
        {
            SetState(ClientGameState.Connecting);
        }

        public void NotifyConnected()
        {
            SetState(ClientGameState.Connected);
            SetState(ClientGameState.Authenticating);
        }

        public void NotifyLoginSent()
        {
            SetState(ClientGameState.WaitingForSpawn);
        }

        public void NotifyPlayerSpawned()
        {
            SetState(ClientGameState.Playing);
        }

        public void NotifyDisconnected()
        {
            SetState(ClientGameState.Disconnecting);
            SetState(ClientGameState.Disconnected);
        }

        private void SetState(ClientGameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
