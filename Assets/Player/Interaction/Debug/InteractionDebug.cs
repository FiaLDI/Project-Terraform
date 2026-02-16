
using UnityEngine;

public static class InteractionDebug
{
    public const bool ENABLED = true; // ← выключишь, когда всё заработает

    public static void Log(string msg, Object ctx = null)
    {
        if (!ENABLED) return;
        Debug.Log($"<color=#00E5FF>[INTERACTION]</color> {msg}", ctx);
    }

    public static void Warn(string msg, Object ctx = null)
    {
        if (!ENABLED) return;
        Debug.LogWarning($"<color=#FFC107>[INTERACTION]</color> {msg}", ctx);
    }

    public static void Error(string msg, Object ctx = null)
    {
        if (!ENABLED) return;
        Debug.LogError($"<color=#FF5252>[INTERACTION]</color> {msg}", ctx);
    }
}
