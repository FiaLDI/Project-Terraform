namespace Multiplayer.Domain
{
    public enum ServerGameState
    {
        Offline,
        Starting,
        LoadingScene,
        PreparingWorld,
        WorldReady,
        AcceptingConnections,
        Running,
        ShuttingDown
    }
}
