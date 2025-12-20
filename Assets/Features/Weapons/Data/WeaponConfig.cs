using UnityEngine;
using Features.Combat.Domain;
using Features.Weapons.Domain;
using Features.Items.Data;

namespace Features.Weapons.Data
{
    [CreateAssetMenu(menuName = "Items/Configs/WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Identity")]
        public string weaponId;
        public Sprite icon;

        [Header("Weapon Type")]
        public bool isMelee;
        public bool isProjectile;
        public FireMode fireMode = FireMode.Semi;
        public int burstCount = 3;

        [Header("Damage")]
        public float damage = 20f;
        public DamageType damageType = DamageType.Ballistic;

        [Space]
        public float headMultiplier = 2.0f;
        public float limbMultiplier = 0.75f;
        public float armorPenetration = 0.2f;

        [Header("Recoil")]
        public float verticalRecoil = 1.5f;
        public float horizontalRecoil = 0.5f;
        public AnimationCurve recoilPattern;

        [Header("Spread / ADS")]
        public float hipfireSpread = 2f;
        public float adsSpread = 0.5f;
        public float swayIntensity = 0.2f;

        [Header("Timing")]
        public float fireRate = 8f;
        public float reloadTime = 1.4f;

        [Header("Magazine")]
        public int magazineSize = 30;
        public Item ammoType;
        public int ammoPerItem = 30;
        public int startingAmmo = 30;

        [Header("Distances")]
        public float maxDistance = 100f;

        [Header("Projectile (Optional)")]
        public ProjectileConfig projectileConfig;

        [Header("Melee (Optional)")]
        public float meleeRange = 2f;
        public float meleeDamage = 35f;
        public float meleeAngle = 55f;    // Cone hit
        public float meleeImpactForce = 5f;

        [Header("FX")]
        public GameObject muzzleFlashFX;
        public GameObject hitFX;
        public GameObject bloodFX;
    }
}
