using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/Shield Grid")]
public class ShieldGridAbilitySO : AbilitySO
{
    public float radius = 8f;
    public float duration = 15f;
    public float damageReductionPercent = 50f;

    [Header("FX")]
    public GameObject shieldGridPrefab;

    public override void Execute(AbilityContext context)
    {
        if (!shieldGridPrefab) return;

        GameObject gridObj = Instantiate(
            shieldGridPrefab,
            context.Owner.transform.position,
            Quaternion.identity
        );

        if (gridObj.TryGetComponent<ShieldGridBehaviour>(out var grid))
        {
            grid.Init(radius, duration, damageReductionPercent, context.Owner);
        }
    }
}
