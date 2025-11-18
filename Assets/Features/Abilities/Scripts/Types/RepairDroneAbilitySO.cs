using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/RepairDrone")]
public class RepairDroneAbilitySO : AbilitySO
{
    public float lifetime = 10f;
    public float healPerSecond = 12f;
    public float healRadius = 6f;
    public float followSpeed = 6f;

    public override void Execute(AbilityContext context)
    {
        // === БУСТ В HUD — Repair Drone Active ===
        var buffs = context.Owner.GetComponent<BuffSystem>();
        if (buffs != null && buffIcon != null)
        {
            buffs.AddBuff(
                BuffType.RepairDroneActive,
                value: 0f,            // нам не нужны бонусы
                duration: lifetime,
                icon: buffIcon
            );
        }

        // === Создание дрона ===
        if (payloadPrefab == null)
        {
            Debug.LogError("RepairDroneAbilitySO: payloadPrefab not assigned!");
            return;
        }

        GameObject droneObj = Instantiate(
            payloadPrefab,
            context.Owner.transform.position,
            Quaternion.identity
        );

        var drone = droneObj.GetComponent<RepairDroneBehaviour>();
        if (drone != null)
        {
            drone.Init(
                context.Owner,
                lifetime,
                healPerSecond,
                healRadius,
                followSpeed
            );
        }

        Destroy(droneObj, lifetime + 0.3f);
    }
}
