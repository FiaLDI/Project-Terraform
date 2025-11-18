using UnityEngine;

public struct AbilityContext
{
    /// <summary>
    /// Кто кастует способность (обычно объект игрока)
    /// </summary>
    public GameObject Owner;

    /// <summary>
    /// Камера, от которой идёт прицеливание
    /// </summary>
    public Camera AimCamera;

    /// <summary>
    /// Цель как точка в мире (point abilities)
    /// </summary>
    public Vector3 TargetPoint;

    /// <summary>
    /// Куда направлена способность (направление луча, линий, эффектов)
    /// </summary>
    public Vector3 Direction;

    /// <summary>
    /// Индекс слота (1–5)
    /// </summary>
    public int SlotIndex;

    public AbilityContext(
        GameObject owner,
        Camera aimCamera,
        Vector3 targetPoint,
        Vector3 direction,
        int slotIndex
    )
    {
        Owner = owner;
        AimCamera = aimCamera;
        TargetPoint = targetPoint;
        Direction = direction;
        SlotIndex = slotIndex;
    }
}
