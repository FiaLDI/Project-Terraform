using System;

[Serializable]
public struct StatsDebugData
{
    // COMBAT
    public float damage;
    public float fireRate;
    public float spread;
    public float aimSpread;
    public float range;
    public float recoil;
    public int magazine;

    public float critChance;
    public float critMultiplier;
    public float penetration;

    // HP
    public float hp;
    public float maxHp;
    public float shield;
    public float maxShield;

    // ENERGY
    public float energy;
    public float maxEnergy;
    public float regen;
    public float costMult;

    // MOVEMENT
    public float walk;
    public float sprint;
    public float crouch;
    public float rotation;
    public float gravity;
    public float jump;

    // RESIST
    public float generic;
    public float explosion;
    public float energyRes;
    public float mining;
    public float melee;
    public float fire;
    public float electric;
    public float poison;
    public float frost;
    public float acid;
}
