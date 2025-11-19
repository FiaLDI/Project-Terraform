using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/Repair Drone")]
public class RepairDroneAbilitySO : AbilitySO
{
    [Header("Aura Buff")]
    public AreaBuffSO healAura; // <-- АУРА ХИЛА здесь

    [Header("Buffs")]
    public BuffSO droneActiveBuff; // <-- баф владельцу (если нужен)

    [Header("Drone Settings")]
    public float lifetime = 10f;
    public float followSpeed = 6f;

    [Header("FX / Drone Prefab")]
    public GameObject dronePrefab;

    public override void Execute(AbilityContext context)
    {
        var owner = context.Owner;
        if (!owner) return;

        // Баф владельцу ("дрон активен") — по желанию
        var buffs = owner.GetComponent<BuffSystem>();
        if (buffs && droneActiveBuff)
            buffs.AddBuff(droneActiveBuff);

        if (!dronePrefab)
        {
            Debug.LogError("RepairDroneAbilitySO: dronePrefab not assigned!");
            return;
        }

        // ---------- SPAWN DRONE ----------
        GameObject droneObj = Instantiate(dronePrefab, owner.transform.position, Quaternion.identity);

        // ---------- ADD AURA EMITTER TO DRONE ----------
        if (healAura != null)
        {
            var emitter = droneObj.AddComponent<AreaBuffEmitter>();
            emitter.area = healAura;        // <-- aura делает лечение
            GameObject.Destroy(emitter, lifetime);
        }

        // ---------- DRONE MOVEMENT ----------
        if (droneObj.TryGetComponent<RepairDroneBehaviour>(out var drone))
        {
            drone.Init(owner, lifetime, followSpeed);
        }

        Destroy(droneObj, lifetime + 0.3f);
    }
}
