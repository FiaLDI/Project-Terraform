using UnityEngine;

public abstract class BuffSO : ScriptableObject
{
    [Header("Base")]
    public string buffId;
    public string displayName;
    [TextArea(2, 4)]
    public string description;       // <--- добавили

    public Sprite icon;

    [Tooltip("Длительность баффа. Для вечных баффов выставь Infinity.")]
    public float duration = 5f;

    [Header("Flags")]
    public bool isDebuff = false;
    public bool isStackable = false;

    /// <summary>
    /// Генерирует строку описания, которая может использовать параметры конкретного баффа.
    /// Можно переопределить в наследниках.
    /// </summary>
    public virtual string GetDescription()
    {
        return description;
    }

    public virtual void OnApply(BuffInstance instance) { }
    public virtual void OnTick(BuffInstance instance, float dt) { }
    public virtual void OnExpire(BuffInstance instance) { }
}
