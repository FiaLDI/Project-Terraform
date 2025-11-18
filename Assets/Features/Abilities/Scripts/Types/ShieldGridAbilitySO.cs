using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/ShieldGrid")]
public class ShieldGridAbilitySO : AbilitySO
{
    public float radius = 8f;
    public float duration = 15f;
    public float damageReductionPercent = 50f;

    public override void Execute(AbilityContext context)
    {
        if (payloadPrefab == null) return;

        var gridObj = GameObject.Instantiate(
            payloadPrefab,
            context.Owner.transform.position,
            Quaternion.identity
        );

        var grid = gridObj.GetComponent<ShieldGridBehaviour>();
        if (grid != null)
        {
            grid.Init(radius, duration, damageReductionPercent, context.Owner);
        }
    }
}
