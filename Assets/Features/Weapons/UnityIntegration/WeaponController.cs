using UnityEngine;
using Features.Items.Domain;
using Features.Weapons.Data;
using Features.Weapons.Domain;
using Features.Weapons.Application;
using Features.Combat.Application;
using Features.Combat.Domain;
using Features.Inventory;
using Features.Equipment.Domain;

namespace Features.Weapons.UnityIntegration
{
    public class WeaponController : MonoBehaviour, IAmmoProvider, IReloadable
    {
        [Header("References")]
        public UnityEngine.Camera playerCamera;
        public Transform muzzlePoint;
        public ParticleSystem muzzleFlash;
        public Animator animator;

        // Runtime
        private ItemInstance instance;
        private WeaponConfig config;
        private WeaponAmmoState ammoState;

        // Runtime stats
        private WeaponRuntimeStats runtimeStats;

        // Inventory
        private IInventoryContext inventory;

        // Services
        private WeaponService weaponService;
        private AmmoService ammoService;
        private ReloadService reloadService;
        private HitScanService hitscan;
        private ProjectileService projectileService;
        private RecoilService recoilService;
        private AimService aimService;
        private CombatService combat;
        private MeleeService meleeService;

        // Fire control
        private bool triggerHeld;

        public int CurrentAmmo => ammoState?.ammoInMagazine ?? 0;
        public int MaxAmmo => runtimeStats?.magazineSize ?? 0;


        // ======================================================
        // SETUP
        // ======================================================

        public WeaponController Setup(ItemInstance inst)
        {
            instance = inst;
            config = inst.itemDefinition.weaponConfig;
            return this;
        }

        public void Init(IInventoryContext inventory)
        {
            this.inventory = inventory;
        }

        // ======================================================
        // INITIALIZE
        // ======================================================

        public void Initialize(UnityEngine.Camera camera)
        {
            playerCamera = camera;
            combat = CombatServiceProvider.Service;

            // === CALCULATE RUNTIME STATS ===
            runtimeStats = WeaponStatCalculator.Calculate(instance);

            // === SERVICES ===
            weaponService = new WeaponService();
            weaponService.Initialize(runtimeStats);

            ammoService = new AmmoService(inventory.Service);
            reloadService = new ReloadService(ammoService);
            hitscan = new HitScanService();
            projectileService = new ProjectileService(combat);

            recoilService = new RecoilService();
            aimService = new AimService();
            meleeService = new MeleeService();

            ammoState = new WeaponAmmoState(runtimeStats.magazineSize);

            // === REFERENCES ===
            if (muzzlePoint == null)
                muzzlePoint = transform.Find("MuzzlePoint");

            if (muzzleFlash == null && muzzlePoint != null)
                muzzleFlash = muzzlePoint.GetComponentInChildren<ParticleSystem>();

            // === INIT SERVICES ===
            aimService.Initialize(runtimeStats);

            recoilService.Initialize(
                runtimeStats,
                config.recoilPattern
            );
        }

        private void Update()
        {
            HandleAutomaticFire();

            if (ammoState.isReloading)
            {
                Debug.Log("[Reload] PerformReloadStep()");
                reloadService.PerformReloadStep(config, ammoState);
                Debug.Log($"[Reload] ammo now {ammoState.ammoInMagazine}");
            }
        }


        // ======================================================
        // INPUT
        // ======================================================

        public void OnUsePrimary_Start()
        {
            triggerHeld = true;

            if (config.fireMode == FireMode.Semi)
                TryFireOnce();
        }

        public void OnUsePrimary_Hold() { }

        public void OnUsePrimary_Stop()
        {
            triggerHeld = false;
            recoilService.Reset();
        }

        public void OnUseSecondary_Start()
        {
            aimService.SetAiming(true);
        }

        public void OnUseSecondary_Hold() { }

        public void OnUseSecondary_Stop()
        {
            aimService.SetAiming(false);
        }

        public void OnReloadPressed()
        {
            Debug.Log(
                $"[ReloadPressed] mag={ammoState.ammoInMagazine}/{runtimeStats.magazineSize}, " +
                $"isReloading={ammoState.isReloading}, " +
                $"ammoType={config.ammoType?.name}"
            );

            bool can = reloadService.CanReload(instance, config, ammoState);
            Debug.Log($"[ReloadPressed] CanReload = {can}");

            if (can)
            {
                var result = reloadService.StartReload(instance, config, ammoState);
                Debug.Log($"[ReloadPressed] Reload STARTED ({result})");

                if (animator != null)
                    animator.Play(
                        result == ReloadResult.EmptyReload ? "ReloadEmpty" : "Reload"
                    );
            }
        }


        // ======================================================
        // FIRE LOOP
        // ======================================================

        private void HandleAutomaticFire()
        {
            if (!triggerHeld)
                return;

            if (config.fireMode == FireMode.Auto)
                TryFireOnce();
        }

        private void TryFireOnce()
        {
            if (!weaponService.CanShoot(Time.time))
                return;

            if (ammoState.isReloading)
                return;

            if (ammoState.ammoInMagazine <= 0)
            {
                if (animator != null)
                    animator.Play("DryFire");
                return;
            }

            weaponService.RegisterShot(Time.time);
            FireWeapon();
        }

        // ======================================================
        // FIRE
        // ======================================================

        private void FireWeapon()
        {
            ammoState.ammoInMagazine--;

            ApplyRecoil();
            PlayMuzzleFx();

            if (config.isMelee)
            {
                PerformMeleeAttack();
                return;
            }

            Vector3 origin = playerCamera.transform.position;
            Vector3 direction = aimService.GetSpreadDirection(playerCamera.transform);

            // === DEBUG RAY ===
            Debug.DrawRay(
                origin,
                direction * runtimeStats.range,
                Color.red,
                0.15f
            );

            if (config.isProjectile)
            {
                projectileService.SpawnProjectile(
                    config.projectileConfig,
                    muzzlePoint.position,
                    direction
                );
            }
            else
            {
                FireHitscan(origin, direction);
            }
        }

        private void FireHitscan(Vector3 origin, Vector3 direction)
        {
            var scan = hitscan.FireHitscan(
                origin,
                direction,
                runtimeStats.range,
                runtimeStats.damage,
                config.damageType
            );

            if (scan.hit && scan.target != null)
            {
                Debug.DrawRay(
                    scan.hitPoint,
                    Vector3.up * 0.25f,
                    Color.yellow,
                    0.25f
                );

                HitInfo hit = weaponService.CreateHit(
                    scan.hitPoint,
                    direction,
                    config.damageType
                );

                combat.ApplyDamage(
                    scan.target,
                    hit,
                    DamageModifiers.Default
                );
            }
        }

        // ======================================================
        // HELPERS
        // ======================================================

        private Vector3 GetSpreadDirection()
        {
            float spread = aimService.IsAiming
                ? runtimeStats.aimSpread
                : runtimeStats.spread;

            Vector3 dir = playerCamera.transform.forward;

            float yaw = Random.Range(-spread, spread);
            float pitch = Random.Range(-spread, spread);

            return Quaternion.Euler(pitch, yaw, 0f) * dir;
        }

        private void ApplyRecoil()
        {
            float recoil = runtimeStats.recoil;

            Vector2 recoilVec = recoilService.GetRecoil();

            playerCamera.transform.localRotation *=
                Quaternion.Euler(-recoilVec.y, recoilVec.x, 0f);


            playerCamera.transform.localRotation *=
                Quaternion.Euler(-recoilVec.y, recoilVec.x, 0f);
        }

        private void PlayMuzzleFx()
        {
            if (muzzleFlash != null)
                muzzleFlash.Play();

            if (animator != null)
                animator?.Play("Fire");
        }

        private void PerformMeleeAttack()
        {
            var hits = meleeService.PerformMeleeAttack(
                config,
                playerCamera.transform.position,
                playerCamera.transform.forward
            );

            foreach (var h in hits)
            {
                HitInfo hit = new HitInfo
                {
                    damage = runtimeStats.damage,
                    type = DamageType.Melee,
                    point = h.hitPoint,
                    direction = playerCamera.transform.forward
                };

                combat.ApplyDamage(
                    h.target,
                    hit,
                    DamageModifiers.Default
                );
            }

            animator?.Play("Melee");
        }
    }
}
