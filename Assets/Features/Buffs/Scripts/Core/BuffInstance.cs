using UnityEngine;

[System.Serializable]
public class BuffInstance
{
    public BuffSO Config { get; }
    public IBuffTarget Target { get; }

    public float Duration { get; private set; }
    public float EndTime { get; private set; }

    public int StackCount { get; set; } = 1;

    public float Remaining => Mathf.Max(0f, EndTime - Time.time);
    public float Progress01 => Mathf.Clamp01(Remaining / Duration);
    public bool IsExpired => Time.time >= EndTime;

    public BuffInstance(BuffSO config, IBuffTarget target)
    {
        Config = config;
        Target = target;

        Duration = config.duration;
        EndTime = Time.time + Duration;
    }

    public void Refresh(float newDuration)
    {
        Duration = newDuration;
        EndTime = Time.time + newDuration;
    }
}
