using UnityEngine;

public static class PhaseAssert
{
    /// <summary>
    /// Проверяет, что объект достиг нужной фазы.
    /// ЛОГИРУЕТ ошибку, но не ломает выполнение.
    /// </summary>
    public static bool Require(
        ServerGamePhase phase,
        GamePhase required,
        Object context = null)
    {
        if (phase == null)
            return true;

        if (!phase.IsAtLeast(required))
        {
            Debug.LogError(
                $"[PHASE ASSERT] Required phase: {required}, current: {phase.Current}",
                context
            );
            return false;
        }

        return true;
    }

    /// <summary>
    /// То же самое, но кидает исключение (для dev/debug).
    /// </summary>
    public static void RequireOrThrow(
        ServerGamePhase phase,
        GamePhase required,
        Object context = null)
    {
        if (phase == null)
            return;

        if (!phase.IsAtLeast(required))
            throw new System.InvalidOperationException(
                $"[PHASE ASSERT] Required {required}, current {phase.Current}"
            );
    }
}
