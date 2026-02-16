using System.Collections.Generic;

public static class StatKeyRegistry
{
    public static readonly List<string> AllIds = new()
    {
        "health.maxHp",
        "health.regen",
        "health.shield",

        "energy.max",
        "energy.regen",
        "energy.costMult",

        "combat.damage",

        "movement.walkSpeed",
        "movement.sprintSpeed",
        "movement.crouchSpeed",
        "movement.rotation",

        "turret.fireRate",

        "mining.power"
    };
}
