using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;
using Features.Player.UnityIntegration;

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

    public void BuffAllPlayerDevices(GameObject player, BuffSO buff, float duration = -1f)
    {
        if (buff == null)
        {
            Debug.LogWarning("[DeviceBuffService] BuffSO is null");
            return;
        }

        BuffAllTurrets(player, buff, duration);

        // будущие устройства (дроны, щиты и т.д.)
    }

    private void BuffAllTurrets(GameObject player, BuffSO buff, float duration)
    {
        var devices = PlayerDeviceRegistry.Instance?.GetDevices(player);
        if (devices == null)
            return;

        foreach (var turret in devices)
        {
            if (!turret) continue;

            if (turret.TryGetComponent<BuffSystem>(out var bs))
            {
                var service = bs.GetServiceSafe();
                if (service == null) continue;

                var inst = service.AddBuff(buff, bs.Target);

                if (duration >= 0 && inst != null)
                    inst.SetDuration(duration);
            }
        }
    }


}
