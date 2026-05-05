# CoreGameplay

`CoreGameplay` contains the shared gameplay foundation used by player classes, items, buffs, passives, and world actors.

## Subsystems

- `Stats` holds typed stat models, presets, owner bindings, and network sync.
- `Buffs` owns runtime buff instances, ticking, stacking rules, and client buff state sync.
- `Passives` defines passive loadouts that apply persistent gameplay changes, usually through buffs and ability modifiers.
- `Effects` executes server-side effects such as damage, healing, projectiles, scans, prefab spawns, sounds, and chain hits.
- `Classes` binds a class id to visuals, base stat preset, passives, abilities, and progression data.

## Typical runtime flow

1. `PlayerStateNetAdapter` waits until stats are ready.
2. The selected class preset is applied to `PlayerStats`.
3. Class passives plus runtime progression passives are sent into `PassiveNetAdapter` and `PassiveSystem`.
4. Passive effects usually add long-lived buffs through `PassiveExecutor` and `BuffSystem`.
5. Buff effects modify `StatsFacade` through stat keys.
6. Abilities and item actions execute `Effects`, which consume the current stats and buff state.

## Folder layout

- `Buffs/`, `Classes/`, `Effects/`, `Passives/`, `Stats/` are separate subsystems with their own data and runtime code.
- `Configs` or `Config` folders store ScriptableObject assets.
- `Scripts` folders store application, domain, and Unity integration code.
- `Prefabs` folders store helper runtime prefabs used by the subsystem.

## Cross-subsystem bridge

`StatsBuffTarget` is the main shared bridge between `Stats`, `Buffs`, and `Passives`. It exposes the entity stats facade, the entity buff system, and ownership information used by effects and targeting.
