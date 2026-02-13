using FishNet.Broadcast;
using FishNet.Serializing;

public struct LoginMessage: IBroadcast
{
    public string PersistentId;
}
