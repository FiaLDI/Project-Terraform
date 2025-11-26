using UnityEngine;

public interface IMineable
{
    /// <summary> наносим урон добычи (виртуальный), возвращает true если объект разрушен </summary>
    bool Mine(float amount, Tool tool);

    /// <summary> показывает общий прогресс (0..1), опционально </summary>
    float GetProgress();
}
