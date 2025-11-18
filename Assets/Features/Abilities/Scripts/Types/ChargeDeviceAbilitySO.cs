using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/ChargeDevice")]
public class ChargeDeviceAbilitySO : AbilitySO
{
    public float bonusEnergyRegen = 15f;
    public float duration = 8f;

    public override void Execute(AbilityContext context)
    {
        var buffs = context.Owner.GetComponent<BuffSystem>();
        if (buffs != null)
            buffs.AddBuff(BuffType.EnergyRegen, bonusEnergyRegen, duration, buffIcon);

        if (payloadPrefab != null)
        {
            GameObject fx = Instantiate(
                payloadPrefab,
                context.Owner.transform.position,
                Quaternion.identity
            );

            var beh = fx.GetComponent<ChargeDeviceBehaviour>();
            if (beh != null)
                beh.Init(context.Owner.transform, duration);

            Destroy(fx, duration + 0.2f);
        }
    }
}
