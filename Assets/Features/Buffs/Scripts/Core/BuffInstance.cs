using UnityEngine;

[System.Serializable]
public class BuffInstance
{
    public BuffType Type { get; }
    public float Value { get; }
    public float Duration { get; }

    public float EndTime { get; private set; }
    public Sprite Icon { get; }

    /// <summary>
    /// Сколько осталось времени до конца баффа.
    /// </summary>
    public float Remaining => Mathf.Max(0f, EndTime - Time.time);

    /// <summary>
    /// Прогресс баффа в интервале 0→1 (для UI индикаторов).
    /// </summary>
    public float Progress01 => Mathf.Clamp01(Remaining / Duration);

    public bool IsExpired => Time.time >= EndTime;

    public BuffInstance(BuffType type, float value, float duration, Sprite icon)
    {
        Type = type;
        Value = value;
        Duration = duration;
        Icon = icon;

        EndTime = Time.time + duration;
    }

    /// <summary>
    /// Пролонгация баффа (например, если он стакается или обновляется)
    /// </summary>
    public void Refresh(float newDuration)
    {
        EndTime = Time.time + newDuration;
    }
}
