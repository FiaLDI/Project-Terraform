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

        "move.walkSpeed",
        "move.sprintSpeed",
        "move.crouchSpeed",
        "move.rotation",
        "move.gravity",
        "move.jump.height",

        "turret.fireRate",

        "mining.power"
    };
}
