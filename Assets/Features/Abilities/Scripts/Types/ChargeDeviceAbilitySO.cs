using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/Charge Device")]
public class ChargeDeviceAbilitySO : AbilitySO
{
    [Header("Aura Buff")]
    public AreaBuffSO areaBuff;     // <- вот это главное!

    [Header("FX")]
    public GameObject chargeFxPrefab;

    public override void Execute(AbilityContext context)
    {
        var owner = context.Owner;
        if (!owner) return;

        float duration = areaBuff != null && areaBuff.buff != null
            ? areaBuff.buff.duration
            : 0f;

        // ---------- AURA APPLY ----------
        if (areaBuff != null)
        {
            var emitter = owner.AddComponent<AreaBuffEmitter>();
            emitter.area = areaBuff;

            // уничтожаем эмиттер после окончания бафа
            GameObject.Destroy(emitter, duration);
        }

        // ---------- FX ----------
        if (chargeFxPrefab)
        {
            GameObject fx = Instantiate(chargeFxPrefab, owner.transform.position, Quaternion.identity);

            if (fx.TryGetComponent<ChargeDeviceBehaviour>(out var beh))
                beh.Init(owner.transform, duration);

            Destroy(fx, duration + 0.2f);
        }
    }
}
