using UnityEngine;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;
using Features.Items.Domain;
using Features.Weapons.Data;
using Features.Weapons.Domain;
using Features.Inventory;
using Features.Equipment.Domain;
using Features.Weapons.Application;

namespace Features.Weapons.UnityIntegration
{
    public class WeaponController : MonoBehaviour, IAmmoProvider, IReloadable
    {
        [Header("References")]
        public UnityEngine.Camera playerCamera;
        public Transform muzzlePoint;
        public ParticleSystem muzzleFlash;
        public Animator animator;

        private ItemInstance instance;
        private WeaponConfig config;
        private WeaponAmmoState ammoState;
        private WeaponRuntimeStats runtimeStats;
        private IInventoryContext inventory;
        private WeaponService weaponService;
        private ReloadService reloadService;
        private AimService aimService;
        private RecoilService recoilService;

        private bool triggerHeld;

        public int CurrentAmmo => ammoState?.ammoInMagazine ?? 0;
        public int MaxAmmo => runtimeStats?.magazineSize ?? 0;

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

        public void Initialize(UnityEngine.Camera camera)
        {
            playerCamera = camera;

            runtimeStats = WeaponStatCalculator.Calculate(instance);

            weaponService = new WeaponService();
            weaponService.Initialize(runtimeStats);

            reloadService = new ReloadService(
                new AmmoService(inventory.Service)
            );

            aimService = new AimService();
            aimService.Initialize(runtimeStats);

            recoilService = new RecoilService();
            recoilService.Initialize(runtimeStats, config.recoilPattern);

            ammoState = new WeaponAmmoState(runtimeStats.magazineSize);
        }

        private void Update()
        {
            if (triggerHeld && config.fireMode == FireMode.Auto)
                TryFireOnce();

            if (ammoState.isReloading)
                reloadService.PerformReloadStep(config, ammoState);
        }

        public void OnUsePrimary_Start()
        {
            triggerHeld = true;

            if (config.fireMode == FireMode.Semi)
                TryFireOnce();
        }

        public void OnUsePrimary_Stop()
        {
            triggerHeld = false;
            recoilService.Reset();
        }

        public void OnReloadPressed()
        {
            if (reloadService.CanReload(instance, config, ammoState))
                reloadService.StartReload(instance, config, ammoState);
        }

        private void TryFireOnce()
        {
            if (!weaponService.CanShoot(Time.time))
                return;

            if (ammoState.isReloading || ammoState.ammoInMagazine <= 0)
                return;

            weaponService.RegisterShot(Time.time);
            ammoState.ammoInMagazine--;

            ApplyRecoil();
            PlayMuzzleFx();

            var ctx = new EffectContext(
                GetComponent<IBuffSource>(),
                null,
                playerCamera.transform.position,
                aimService.GetSpreadDirection(playerCamera.transform)
            );

            foreach (var def in config.fireEffects)
                EffectExecutor.Instance.Execute(def, ctx);
        }

        private void ApplyRecoil()
        {
            if (playerCamera == null) return;

            Vector2 recoil = recoilService.GetRecoil();
            playerCamera.transform.localRotation *=
                Quaternion.Euler(-recoil.y, recoil.x, 0f);
        }

        private void PlayMuzzleFx()
        {
            muzzleFlash?.Play();
            animator?.Play("Fire");
        }

        public void OnUsePrimary_Hold()
        {
            // ничего не делаем
        }

        public void OnUseSecondary_Start()
        {
            aimService?.SetAiming(true);
        }

        public void OnUseSecondary_Hold()
        {
            // ничего
        }

        public void OnUseSecondary_Stop()
        {
            aimService?.SetAiming(false);
        }
        // =========================
        // CLIENT FX (for WeaponFxNetwork)
        // =========================

        public void PlayFireFxClient()
        {
            muzzleFlash?.Play();
            animator?.Play("Fire");
        }

        public void PlayReloadFxClient(bool emptyReload)
        {
            if (animator == null)
                return;

            animator.Play(emptyReload ? "ReloadEmpty" : "Reload");
        }

    }
}
