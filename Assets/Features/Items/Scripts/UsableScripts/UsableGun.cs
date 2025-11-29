using UnityEngine;

public class UsableGun : MonoBehaviour, IUsable, IStatItem
{
    private Camera cam;

    // Runtime stats
    private float damage;
    private float fireRate;
    private float recoil;
    private float spread;
    private float range;

    private float nextShotTime = 0f;

    public void Initialize(Camera playerCamera)
    {
        cam = playerCamera;
    }

    public void ApplyRuntimeStats(ItemRuntimeStats stats)
    {
        damage = stats[ItemStatType.Damage];
        fireRate = stats[ItemStatType.FireRate];
        recoil = stats[ItemStatType.Recoil];
        spread = stats[ItemStatType.Spread];
        range = stats[ItemStatType.Range];
    }

    public void OnUsePrimary_Start() => TryShoot();
    public void OnUsePrimary_Hold() => TryShoot();
    public void OnUsePrimary_Stop() { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold() { }
    public void OnUseSecondary_Stop() { }

    private void TryShoot()
    {
        if (Time.time < nextShotTime) return;
        nextShotTime = Time.time + 1f / fireRate;

        Ray ray = CreateRayWithSpread(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(damage, DamageType.Ballistic);
            }
        }

        ApplyRecoil();
    }

    private void ApplyRecoil()
    {
        // простая отдача — камеру чуть дергаем вверх
        cam.transform.localRotation *= Quaternion.Euler(-recoil, 0, 0);
    }

    private Ray CreateRayWithSpread(Vector3 origin, Vector3 forward)
    {
        Vector3 direction =
            forward +
            new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                Random.Range(-spread, spread)
            );

        return new Ray(origin, direction.normalized);
    }
}
