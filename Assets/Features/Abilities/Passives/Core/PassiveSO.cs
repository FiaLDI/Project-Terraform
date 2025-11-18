using UnityEngine;

public abstract class PassiveSO : ScriptableObject
{
    [Header("UI")]
    public Sprite icon;   // ← ДОБАВЛЕНО: чтобы баф мог взять иконку пассивки

    /// <summary>
    /// Применить пассивку к владельцу.
    /// </summary>
    public abstract void Apply(GameObject owner);

    /// <summary>
    /// Удалить эффект при смене класса.
    /// </summary>
    public virtual void Remove(GameObject owner) { }
}
