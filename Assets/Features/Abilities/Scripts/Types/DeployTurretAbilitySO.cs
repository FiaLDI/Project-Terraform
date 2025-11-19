using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/Deploy Turret")]
public class DeployTurretAbilitySO : AbilitySO
{
    [Header("Turret Stats")]
    public GameObject turretPrefab;
    public float duration = 25f;
    public float damagePerSecond = 4f;
    public float range = 15f;
    public int hp = 150;

    public override void Execute(AbilityContext context)
    {
        if (!turretPrefab) return;

        var obj = Instantiate(turretPrefab, context.TargetPoint, Quaternion.identity);

        if (obj.TryGetComponent<TurretBehaviour>(out var turret))
        {
            turret.Init(
                context.Owner,
                hp,
                damagePerSecond,
                range,
                duration
            );
        }
    }

}
