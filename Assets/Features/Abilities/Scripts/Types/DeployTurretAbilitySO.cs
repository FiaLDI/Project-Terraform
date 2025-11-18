using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/DeployTurret")]
public class DeployTurretAbilitySO : AbilitySO
{
    [Header("Специфика турели")]
    public GameObject turretPrefab;
    public float duration = 25f;
    public float damagePerSecond = 4f;
    public float range = 15f;
    public int hp = 150;

    public override void Execute(AbilityContext context)
    {
        if (turretPrefab == null) return;

        var spawnPos = context.TargetPoint;
        var turretObj = GameObject.Instantiate(
            turretPrefab,
            spawnPos,
            Quaternion.identity
        );

        var turret = turretObj.GetComponent<TurretBehaviour>();
        if (turret != null)
        {
            turret.Init(
                owner: context.Owner,
                hp: hp,
                dps: damagePerSecond,
                range: range,
                lifetime: duration
            );
        }
    }
}
