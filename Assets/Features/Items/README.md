# Features Items

This subsystem defines item assets and the runtime logic that executes item actions through the shared effects system.

## Main pieces

- `Item` is the authoring asset for identity, stacking, visuals, equipped buffs, upgrades, and actions.
- `ItemActionDefinition` describes per-action timing and effect lists.
- `ItemRuntimeContext` runs one action runtime, including windup, burst, fire-on-release, tick, and cooldown behavior.
- `ItemRegistrySO` provides runtime lookup for item definitions.
- `HeldItemController`, `ItemObject`, and related Unity integration classes connect items to the player and world.

## Runtime flow

1. Equipment or world interaction resolves an `Item`.
2. `ItemRuntimeContext.StartUse(...)` begins the selected action.
3. The runtime tracks windup, active firing, bursts, release behavior, and cooldown.
4. On execution, the action builds an `EffectContext` or `HitEffectContext`.
5. The item action effects are executed through `EffectExecutor`.

## Integration points

- `Equipment` supplies the currently active equipped item.
- `Inventory` stores item instances and quantities.
- `CoreGameplay/Effects` executes the actual gameplay payload of each action.
- `CoreGameplay/Buffs` supports equipped buffs and upgrade buffs referenced by item assets.
