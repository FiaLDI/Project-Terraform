using System;
using Multiplayer.Domain;

namespace Multiplayer.Application
{
    public interface IServerGameFlow
    {
        ServerGameState CurrentState { get; }
        event Action<ServerGameState> OnStateChanged;

        void StartServer();
        void NotifyServerStarted();
        void NotifySceneLoaded();
        void NotifyWorldPrepared();
        void Shutdown();
    }
}
