using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/Repair Drone")]
public class RepairDroneAbilitySO : AbilitySO
{
    [Header("Aura Buff")]
    public AreaBuffSO healAura;

    [Header("Buffs")]
    public BuffSO droneActiveBuff;

    [Header("Drone Settings")]
    public float lifetime = 10f;
    public float followSpeed = 6f;

    [Header("FX / Drone Prefab")]
    public GameObject dronePrefab;

    public override void Execute(AbilityContext context)
    {
        var owner = context.Owner;
        if (!owner) return;

        var buffs = owner.GetComponent<BuffSystem>();
        if (buffs && droneActiveBuff)
            buffs.AddBuff(droneActiveBuff);

        if (!dronePrefab)
        {
            Debug.LogError("RepairDroneAbilitySO: dronePrefab not assigned!");
            return;
        }

        GameObject droneObj = Instantiate(dronePrefab, owner.transform.position, Quaternion.identity);

        if (healAura != null)
        {
            var emitter = droneObj.AddComponent<AreaBuffEmitter>();
            emitter.area = healAura;
            GameObject.Destroy(emitter, lifetime);
        }

        if (droneObj.TryGetComponent<RepairDroneBehaviour>(out var drone))
        {
            drone.Init(owner, lifetime, followSpeed);
        }

        Destroy(droneObj, lifetime + 0.3f);
    }
}
