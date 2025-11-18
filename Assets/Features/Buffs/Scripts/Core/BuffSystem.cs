using UnityEngine;
using System.Collections.Generic;

public class BuffSystem : MonoBehaviour
{
    private readonly List<BuffInstance> activeBuffs = new();
    public IReadOnlyList<BuffInstance> ActiveBuffs => activeBuffs;

    public event System.Action<BuffInstance> OnBuffAdded;
    public event System.Action<BuffInstance> OnBuffRemoved;

    private void Update()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (activeBuffs[i].IsExpired)
                RemoveBuffInstance(activeBuffs[i]);
        }
    }

    public BuffInstance AddBuff(BuffType type, float value, float duration, Sprite icon)
    {
        var buff = new BuffInstance(type, value, duration, icon);
        activeBuffs.Add(buff);
        OnBuffAdded?.Invoke(buff);
        return buff;
    }

    public void RemoveBuffInstance(BuffInstance buff)
    {
        if (activeBuffs.Remove(buff))
            OnBuffRemoved?.Invoke(buff);
    }

    public float GetTotal(BuffType type)
    {
        float total = 0;

        foreach (var b in activeBuffs)
        {
            if (b.Type == type && !b.IsExpired)
                total += b.Value;
        }

        return total;
    }
}
