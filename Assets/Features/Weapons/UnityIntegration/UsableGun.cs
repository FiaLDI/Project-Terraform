using Features.Equipment.Domain;
using Features.Weapons.UnityIntegration;
using UnityEngine;

[RequireComponent(typeof(WeaponController))]
public class UsableGun : MonoBehaviour, IUsable, IReloadable
{
    private WeaponController weapon;
    public IAmmoProvider AmmoProvider => weapon;

    public void Initialize(Camera camera)
    {
        weapon = GetComponent<WeaponController>();

        if (weapon.playerCamera == null)
            weapon.playerCamera = camera;
    }

    // PRIMARY
    public void OnUsePrimary_Start() => weapon?.OnUsePrimary_Start();
    public void OnUsePrimary_Hold() => weapon?.OnUsePrimary_Hold();
    public void OnUsePrimary_Stop() => weapon?.OnUsePrimary_Stop();

    // SECONDARY
    public void OnUseSecondary_Start() => weapon?.OnUseSecondary_Start();
    public void OnUseSecondary_Hold() => weapon?.OnUseSecondary_Hold();
    public void OnUseSecondary_Stop() => weapon?.OnUseSecondary_Stop();

    public void OnReloadPressed() => weapon?.OnReloadPressed();
}
