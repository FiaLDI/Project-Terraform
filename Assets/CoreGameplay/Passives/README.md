# CoreGameplay Passives

This subsystem manages passive loadouts for an entity and translates passive assets into active gameplay changes.

## What is here

- `Config/` stores passive assets and modifier assets, including grouped content such as `Miner` and `Progress_tech`.
- `PassiveSO` is the main passive asset. It stores `id`, `effects`, and `abilityModifiers`.
- `PassiveSystem` is the server-side component attached to an entity.
- `PassiveService` owns the currently active passive list for one entity.
- `PassiveExecutor` is the single runtime executor that applies passive effects.
- `AbilityModifierSO` and related domain classes mutate ability effect lists for matching abilities.

## Runtime flow

1. `PassiveSystem.SetPassives(...)` replaces the current passive loadout for the entity.
2. `PassiveService.ClearAll()` removes gameplay changes created by old passives.
3. Each new passive gets its own `PassiveSource`.
4. Every `PassiveEffectSO` is converted into `PassiveEffectData` through `Build()`.
5. `PassiveExecutor.Apply(...)` applies the resulting data, typically by adding a buff to `StatsBuffTarget.BuffSystem`.
6. Ability modifiers from all active passives are cached separately and exposed through `GetCachedModifiers()`.

## Removal model

Passive cleanup is source-based. When a passive is removed, its runtime source is used to call `BuffSystem.RemoveBySource(...)`, which cleanly removes buffs created by that passive without touching other sources.

## Common pattern

The most common passive effect in this folder is `PassiveEffect_ApplyBuffSO`. It creates a `BuffSO` payload with `BuffLifetimeMode.WhileSourceAlive`, so the passive remains active until the passive source is explicitly removed.
