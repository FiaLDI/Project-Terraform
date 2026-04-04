using System.Collections.Generic;

public static class StatKeyRegistry
{
    public static readonly List<string> AllIds = new()
    {
        // =========================
        // COMBAT
        // =========================
        "combat.fireRate",
        "combat.damage.flat",
        "combat.spread",
        "combat.recoil",
        "combat.aimSpread",
        "combat.range",
        "combat.magazine",
        "combat.damage.mult",
        "combat.crit.chance",
        "combat.crit.multiplier",
        "combat.penetration",

        // =========================
        // HEALTH
        // =========================
        "health.max",
        "health.regen",
        "health.shield",

        // =========================
        // ENERGY
        // =========================
        "energy.max",
        "energy.regen",
        "energy.cost.mult",

        // =========================
        // MOVEMENT
        // =========================
        "move.base",
        "move.walk",
        "move.sprint",
        "move.crouch",
        "move.rotation",
        "move.gravity",
        "move.jump.height",

        // =========================
        // MINING
        // =========================
        "mining.power",
        "mining.speed",

        // =========================
        // SCANNER
        // =========================
        "scan.range",
        "scan.speed",
        "ability.cooldown",

        // =========================
        // DEFENSE (RESISTANCES)
        // =========================
        "defense.res.generic",
        "defense.res.explosion",
        "defense.res.energy",
        "defense.res.mining",
        "defense.res.melee",
        "defense.res.fire",
        "defense.res.electric",
        "defense.res.poison",
        "defense.res.frost",
        "defense.res.acid"
    };
}
