# CoreGameplay Stats

This subsystem owns the typed stat model used by players, enemies, turrets, buffs, passives, and effects.

## Stat model

The system is split into typed stat modules instead of a single generic dictionary:

- `Health`
- `Energy`
- `Combat`
- `Movement`
- `Mining`
- `Protect`

`StatsFacade` is the shared entry point. Buffs and other systems talk to the facade through `StatKey` values and `TryAdd(...)` or `TryMultiply(...)`.

## Main layers

- `Domain` contains `StatsFacade`, stat keys, and the concrete stat modules.
- `Data` contains `StatsProfileSO`, `StatsPresetSO`, `EnemyStatsPresetSO`, `TurretPresetSO`, and debug registries.
- `UnityIntegration` contains the entity owners such as `PlayerStats`, `EnemyStats`, `TurretStats`, plus sync and update systems.
- `Adapter` contains view-facing adapters for UI and presentation.
- `Net` contains networking-specific stat payloads and profiles.

## Runtime flow

1. `StatsOwnerBase` initializes the stat modules required by its `StatsProfileSO`.
2. The owner assembles a `StatsFacade`.
3. Owner-specific presets apply the base values.
4. Buffs and passives modify stats through stat keys rather than through direct module knowledge.
5. `UnifiedStatsUpdateSystem` handles recurring server updates such as regen.
6. Sync components publish the required subset of data to clients.

## Integration points

- `AddStatEffectSO` and `MultiplyStatEffectSO` modify stats through `StatsFacade`.
- `StatsBuffTarget` exposes the server stats facade to the buff and passive systems.
- Class application feeds `StatsPresetSO` into `PlayerStats`.
- Combat and effect code consume current stat values through the typed stat interfaces.

## Network contract

- `StatsNetSync` synchronizes core health and energy values.
- `MovementStatsSync` synchronizes movement-related values such as walk, sprint, crouch, rotation, gravity, and jump height.

The project intentionally does not sync every stat through one monolithic channel. Different stat groups have different update paths depending on gameplay needs.
