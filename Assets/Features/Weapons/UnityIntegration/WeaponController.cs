using UnityEngine;
using Features.Items.Domain;
using Features.Weapons.Domain;
using Features.Weapons.Data;
using Features.Weapons.Application;
using Features.Combat.Application;
using Features.Combat.Domain;
using Features.Inventory;
using Features.Equipment.Domain;

namespace Features.Weapons.UnityIntegration
{
    public class WeaponController : MonoBehaviour, IUsable
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

        // fire control
        private float nextShotTime;
        private bool triggerHeld;
        private int burstRemaining;

        // ======================================================
        // SETUP (from EquipmentManager)
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

            ammoService = new AmmoService(inventory.Service);
            reloadService = new ReloadService(ammoService);
            hitscan = new HitScanService();
            projectileService = new ProjectileService(combat);
            weaponService = new WeaponService();
            recoilService = new RecoilService();
            aimService = new AimService();
            meleeService = new MeleeService();

            ammoState = new WeaponAmmoState(config.startingAmmo);

            aimService.Initialize(config);
            recoilService.Initialize(config);
        }

        private void Update()
        {
            HandleAutomaticFire();
        }

        // ======================================================
        // IUsable INPUT
        // ======================================================

        public void OnUsePrimary_Start()
        {
            triggerHeld = true;

            if (config.fireMode == FireMode.Semi)
                TryFireOnce();

            if (config.fireMode == FireMode.Burst)
                burstRemaining = config.burstCount;
        }

        public void OnUsePrimary_Hold() { }

        public void OnUsePrimary_Stop()
        {
            triggerHeld = false;
        }

        public void OnUseSecondary_Start() { }
        public void OnUseSecondary_Hold() { }
        public void OnUseSecondary_Stop() { }

        public void OnReloadPressed()
        {
            if (reloadService.CanReload(instance, config, ammoState))
            {
                var result = reloadService.StartReload(instance, config, ammoState);
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
            if (!triggerHeld) return;

            if (config.fireMode == FireMode.Auto)
                TryFireOnce();

            if (config.fireMode == FireMode.Burst && burstRemaining > 0)
            {
                TryFireOnce();
                burstRemaining--;
            }
        }

        private void TryFireOnce()
        {
            if (Time.time < nextShotTime) return;
            if (ammoState.isReloading) return;

            if (ammoState.ammoInMagazine <= 0)
            {
                animator.Play("DryFire");
                return;
            }

            nextShotTime = Time.time + 1f / config.fireRate;
            FireWeapon();
        }

        // ======================================================
        // FIRE ACTION
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
            Vector3 direction =
                aimService.GetSpreadDirection(playerCamera.transform);

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
                config.maxDistance,
                config.damage,
                config.damageType
            );

            if (scan.hit && scan.target != null)
            {
                HitInfo hit = new HitInfo
                {
                    damage = config.damage,
                    type = config.damageType,
                    point = scan.hitPoint,
                    direction = direction
                };

                combat.ApplyDamage(
                    scan.target,
                    hit,
                    DamageModifiers.Default
                );
            }
        }

        private void PlayMuzzleFx()
        {
            muzzleFlash?.Play();
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
                    damage = config.meleeDamage,
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

            animator.Play("Melee");
        }

        private void ApplyRecoil()
        {
            Vector2 recoil = recoilService.GetRecoil();
            playerCamera.transform.localRotation *=
                Quaternion.Euler(-recoil.y, recoil.x, 0f);
        }
    }
}
