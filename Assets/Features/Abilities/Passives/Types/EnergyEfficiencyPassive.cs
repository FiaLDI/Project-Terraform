using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/EnergyEfficiency")]
public class EnergyEfficiencyPassive : PassiveSO
{
    public AreaBuffSO areaBuff;
    private AreaBuffEmitter emitter;

    public override void Apply(GameObject owner)
    {
        emitter = owner.AddComponent<AreaBuffEmitter>();
        emitter.area = areaBuff;
    }

    public override void Remove(GameObject owner)
    {
        if (emitter != null)
            GameObject.Destroy(emitter);
    }
}
