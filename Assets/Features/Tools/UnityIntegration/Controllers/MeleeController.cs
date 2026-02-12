using UnityEngine;
using Features.Items.Domain;
using Features.Equipment.Domain;
using Features.Items.UnityIntegration;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;
using Features.Weapons.Data;

public sealed class MeleeController : MonoBehaviour, IUsable
{
    private Camera cam;
    private ItemInstance instance;
    private WeaponConfig config;
    private IBuffSource source;

    // ======================================================
    // INITIALIZE
    // ======================================================

    public void Initialize(Camera camera)
    {
        cam = camera;

        instance = GetComponent<ItemRuntimeHolder>()?.Instance;
        if (instance == null)
        {
            Debug.LogError("[MeleeController] ItemInstance not found");
            enabled = false;
            return;
        }

        config = instance.itemDefinition.weaponConfig;
        if (config == null)
        {
            Debug.LogError("[MeleeController] WeaponConfig missing");
            enabled = false;
            return;
        }

        source = GetComponentInParent<IBuffSource>();
        if (source == null)
        {
            Debug.LogError("[MeleeController] IBuffSource missing");
            enabled = false;
        }
    }

    // ======================================================
    // IUsable
    // ======================================================

    public void OnUsePrimary_Start() => ExecuteMelee();
    public void OnUsePrimary_Hold()  { }
    public void OnUsePrimary_Stop()  { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold()  { }
    public void OnUseSecondary_Stop()  { }

    // ======================================================
    // EXECUTION
    // ======================================================

    private void ExecuteMelee()
    {
        if (cam == null || config == null || config.meleeEffects == null)
            return;

        var ctx = new EffectContext(
            source: source,
            targets: null,
            origin: cam.transform.position,
            direction: cam.transform.forward
        );

        foreach (var def in config.meleeEffects)
            EffectExecutor.Instance.Execute(def, ctx);
    }
}
