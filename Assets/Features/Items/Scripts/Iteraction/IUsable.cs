using UnityEngine;

/// <summary>
/// Интерфейс для любого предмета, который можно "использовать"
/// (стрелять, бить, бросать, бурить).
/// Этот скрипт должен висеть на префабе предмета, который держит игрок.
/// </summary>
public interface IUsable
{
    void Initialize(Camera playerCamera);
    /// <summary>
    /// Вызывается, когда игрок НАЧИНАЕТ использовать предмет (ЛКМ нажата).
    /// </summary>
    void OnUsePrimary_Start();

    /// <summary>
    /// Вызывается КАЖДЫЙ КАДР, пока игрок ДЕРЖИТ ЛКМ.
    /// </summary>
    void OnUsePrimary_Hold();

    /// <summary>
    /// Вызывается, когда игрок ПРЕКРАЩАЕТ использовать предмет (ЛКМ отпущена).
    /// </summary>
    void OnUsePrimary_Stop();
}