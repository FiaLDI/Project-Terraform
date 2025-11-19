using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/Aura Passive Buff")]
public class AuraPassiveBuffSO : PassiveSO
{
    public AreaBuffSO areaBuff;   // теперь SO хранит buff, radius и маску
    private AreaBuffEmitter emitter;

    public override void Apply(GameObject owner)
    {
        emitter = owner.AddComponent<AreaBuffEmitter>();
        emitter.area = areaBuff; // одно поле вместо трёх
    }

    public override void Remove(GameObject owner)
    {
        if (emitter != null)
            GameObject.Destroy(emitter);
    }
}
