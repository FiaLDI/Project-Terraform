using UnityEngine;

public abstract class BuffSO : ScriptableObject
{
    [Header("Base")]
    public string buffId;
    public string displayName;
    public Sprite icon;

    [Tooltip("Длительность баффа. Для вечных баффов выставь Infinity.")]
    public float duration = 5f;

    public bool isDebuff = false;
    public bool isStackable = false;

    public virtual void OnApply(BuffInstance instance) { }
    public virtual void OnTick(BuffInstance instance, float dt) { }
    public virtual void OnExpire(BuffInstance instance) { }
}
