using System;

public static class NetworkQuestEventBus
{
    public static event Action<string> OnQuestEvent;

    public static void Publish(string evt)
    {
        OnQuestEvent?.Invoke(evt);
    }
}
