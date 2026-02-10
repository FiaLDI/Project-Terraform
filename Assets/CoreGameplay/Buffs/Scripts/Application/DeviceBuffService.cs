using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;
using Features.Player.UnityIntegration;
using UnityEngine;

public class PlayerDeviceBuffService : MonoBehaviour
{
    public static PlayerDeviceBuffService I { get; private set; }

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    public void BuffAllPlayerDevices(
        GameObject player,
        BuffSO buff,
        IBuffSource source
    )
    {
        if (buff == null || source == null)
            return;

        var devices = PlayerDeviceRegistry.Instance?.GetDevices(player);
        if (devices == null)
            return;

        foreach (var turret in devices)
        {
            if (!turret) continue;

            if (turret.TryGetComponent<BuffSystem>(out var bs))
            {
                bs.Add(
                    buff,
                    source: source,
                    lifetimeMode: BuffLifetimeMode.WhileSourceAlive
                );
            }
        }
    }


    private void BuffAllTurrets(GameObject player, BuffSO buff, float duration)
    {
        var devices = PlayerDeviceRegistry.Instance?.GetDevices(player);
        if (devices == null)
            return;

        foreach (var turret in devices)
        {
            if (!turret)
                continue;

            if (!turret.TryGetComponent<BuffSystem>(out var buffSystem))
                continue;

            IBuffSource source =
                duration < 0
                    ? new RuntimeBuffSource(turret)
                    : new TimedBuffSource(this, duration);

            buffSystem.Add(
                buff,
                source,
                duration < 0
                    ? BuffLifetimeMode.WhileSourceAlive
                    : BuffLifetimeMode.Duration
            );
        }
    }
}
