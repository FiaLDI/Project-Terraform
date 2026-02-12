using Features.Buffs.Domain;
using Features.Effects.Application;
using Features.Effects.Domain;
using Features.Equipment.Domain;
using Features.Inventory;
using Features.Items.Domain;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using Features.Weapons.Application;
using Features.Weapons.Data;
using Features.Weapons.Domain;
using UnityEngine;

namespace Features.Weapons.UnityIntegration
{
    public class WeaponController : MonoBehaviour, IAmmoProvider, IReloadable
    {
        [Header("References")]
        public UnityEngine.Camera playerCamera;
        public Transform muzzlePoint;
        public ParticleSystem muzzleFlash;
        public Animator animator;
        private IStatsFacade stats;

        private ItemInstance instance;
        private WeaponConfig config;
        private WeaponAmmoState ammoState;
        private IInventoryContext inventory;
        private WeaponService weaponService;
        private ReloadService reloadService;
        private AimService aimService;
        private RecoilService recoilService;

        private bool triggerHeld;

        public int CurrentAmmo => ammoState?.ammoInMagazine ?? 0;
        public int MaxAmmo => stats?.Combat.MagazineSize ?? 0;

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

            var statsOwner = GetComponentInParent<PlayerStats>();
            if (statsOwner == null)
            {
                Debug.LogError("[WeaponController] PlayerStats not found");
                return;
            }

            stats = statsOwner.Facade;

            weaponService = new WeaponService();
            weaponService.Initialize(stats.Combat);

            reloadService = new ReloadService(
                new AmmoService(inventory.Service)
            );

            aimService = new AimService();
            aimService.Initialize(stats.Combat);

            recoilService = new RecoilService();
            recoilService.Initialize(stats.Combat, config.recoilPattern);

            ammoState = new WeaponAmmoState(config.magazineSize);
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
