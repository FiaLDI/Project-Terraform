using UnityEngine;

public static class WorldSession
{
    public static int WorldVersion { get; private set; }
    public static int Seed { get; set; }

    public static void NewWorld()
    {
        WorldVersion++;
        Debug.Log($"[WorldSession] New world version = {WorldVersion}");
    }
}
