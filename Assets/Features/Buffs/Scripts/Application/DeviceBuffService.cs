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
        DontDestroyOnLoad(gameObject);
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
        var registry = PlayerRegistry.Instance;

        if (!registry.PlayerOwnedTurrets.TryGetValue(player, out var list))
            return;

        foreach (var turret in list)
        {
            if (turret == null) continue;

            if (turret.TryGetComponent<BuffSystem>(out var bs))
            {
                var service = bs.GetServiceSafe();
                if (service == null) continue;

                // создаём бафф
                var inst = service.AddBuff(buff, bs.Target);

                // кастомная длительность
                if (duration >= 0 && inst != null)
                    inst.SetDuration(duration);
            }
        }
    }

}
