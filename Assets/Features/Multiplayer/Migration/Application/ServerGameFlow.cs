using System;
using Multiplayer.Domain;

namespace Multiplayer.Application
{
    public sealed class ServerGameFlow : IServerGameFlow
    {
        public ServerGameState CurrentState { get; private set; }
            = ServerGameState.Offline;

        public event Action<ServerGameState> OnStateChanged;

        public void StartServer()
        {
            SetState(ServerGameState.Starting);
        }

        public void NotifyServerStarted()
        {
            SetState(ServerGameState.LoadingScene);
        }

        public void NotifySceneLoaded()
        {
            SetState(ServerGameState.PreparingWorld);
        }

        public void NotifyWorldPrepared()
        {
            SetState(ServerGameState.WorldReady);
            SetState(ServerGameState.AcceptingConnections);
            SetState(ServerGameState.Running);
        }

        public void Shutdown()
        {
            SetState(ServerGameState.ShuttingDown);
            SetState(ServerGameState.Offline);
        }

        private void SetState(ServerGameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
