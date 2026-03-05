using UnityEngine;
using FishNet.Object;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Weapons.Data;
using Features.Items.Domain;
using Features.Inventory;
using Features.Equipment.Domain;
using Features.Buffs.Domain;
using Features.Weapons.UnityIntegration;

public class WeaponController : MonoBehaviour,
    IUsable,
    IServerUsable,
    IAmmoProvider,
    IReloadable
{
    [Header("FX")]
    public Camera playerCamera;
    public ParticleSystem muzzleFlash;
    public Animator animator;

    private WeaponConfig config;
    private IInventoryContext inventory;

    private int ammoInMagazine;
    private bool serverTriggerHeld;
    private float nextFireTime;
    private System.Random serverRandom;

    private PlayerUsageNetAdapter usageAdapter;

    // ======================================================
    // SETUP (EquipmentManager compatibility)
    // ======================================================

    public WeaponController Setup(ItemInstance inst)
    {
        //config = inst.itemDefinition.weaponConfig;
        return this;
    }

    public void Init(IInventoryContext inv)
    {
        inventory = inv;
    }

    public void Initialize(Camera camera)
    {
        playerCamera = camera;
    }

    private void Awake()
    {
        usageAdapter = GetComponentInParent<PlayerUsageNetAdapter>();
        serverRandom = new System.Random();
    }

    private void EnsureServerInit()
    {
        if (ammoInMagazine == 0 && config != null)
            ammoInMagazine = config.magazineSize;
    }

    // ======================================================
    // AMMO
    // ======================================================

    public int CurrentAmmo => ammoInMagazine;
    public int MaxAmmo => config != null ? config.magazineSize : 0;

    // ======================================================
    // CLIENT SIDE (FX only)
    // ======================================================

    public void OnUsePrimary_Start()
    {
        ApplyRecoil();
    }

    public void OnUsePrimary_Stop() { }
    public void OnUsePrimary_Hold() { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Stop() { }
    public void OnUseSecondary_Hold() { }

    public void PlayFireFxClient()
    {
        muzzleFlash?.Play();
        animator?.Play("Fire");
    }

    public void PlayReloadFxClient(bool emptyReload = false)
    {
        animator?.Play("Reload");
    }

    // ======================================================
    // SERVER AUTHORITY (called from PlayerUsageNetAdapter)
    // ======================================================

    public void ServerPrimaryStart()
    {
        serverTriggerHeld = true;
        EnsureServerInit();
    }

    public void ServerPrimaryStop()
    {
        serverTriggerHeld = false;
    }

    public void ServerPrimaryHold()
    {
        if (!serverTriggerHeld) return;
        if (config == null) return;
        if (ammoInMagazine <= 0) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + (1f / config.fireRate);
        ammoInMagazine--;

        FireAuthoritative();
    }

    public void ServerSecondaryStart() { }
    public void ServerSecondaryStop() { }
    public void ServerSecondaryHold() { }

    public void ServerReload()
    {
        if (config == null) return;

        ammoInMagazine = config.magazineSize;

        var fxNet = GetComponentInParent<WeaponFxNetwork>();
        fxNet?.NotifyReload(false);
    }

    public void OnReloadPressed()
    {
        ServerReload();
    }

    // ======================================================
    // FIRE (SERVER → Effects only)
    // ======================================================

    private void FireAuthoritative()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        // Authoritative aim from PlayerUsageNetAdapter
        if (usageAdapter != null &&
            usageAdapter.TryGetServerAim(out Ray ray))
        {
            origin = ray.origin;
            direction = ray.direction;
        }

        direction = ApplyAuthoritativeSpread(direction);

        // Effects are the ONLY gameplay executor
        if (config.fireEffects != null && config.fireEffects.Length > 0)
        {
            var ctx = new EffectContext(
                GetComponent<IBuffSource>(),
                null,
                origin,
                direction
            );

            foreach (var def in config.fireEffects)
                EffectExecutor.Instance.Execute(def, ctx);
        }

        // FX sync
        var fxNet = GetComponentInParent<WeaponFxNetwork>();
        fxNet?.NotifyFire();
    }

    private Vector3 ApplyAuthoritativeSpread(Vector3 forward)
    {
        float spread = config.hipfireSpread;

        float x = (float)(serverRandom.NextDouble() * 2 - 1);
        float y = (float)(serverRandom.NextDouble() * 2 - 1);

        Vector3 spreadOffset =
            new Vector3(x, y, 0f) * spread * 0.01f;

        return (forward + spreadOffset).normalized;
    }

    // ======================================================
    // LOCAL RECOIL
    // ======================================================

    private void ApplyRecoil()
    {
        if (playerCamera == null || config == null)
            return;

        playerCamera.transform.localRotation *=
            Quaternion.Euler(
                -config.verticalRecoil,
                config.horizontalRecoil,
                0f
            );
    }
}
