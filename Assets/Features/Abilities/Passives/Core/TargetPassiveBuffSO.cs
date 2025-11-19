using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/Target Buff Passive")]
public class TargetPassiveBuffSO : PassiveSO
{
    public BuffSO buff;  

    private BuffInstance _instance;

    public override void Apply(GameObject owner)
    {
        var system = owner.GetComponent<BuffSystem>();
        if (system == null || buff == null) return;

        _instance = system.AddBuff(buff);
    }

    public override void Remove(GameObject owner)
    {
        if (_instance == null) return;

        var system = owner.GetComponent<BuffSystem>();
        if (system != null)
            system.RemoveBuff(_instance);
    }
}
