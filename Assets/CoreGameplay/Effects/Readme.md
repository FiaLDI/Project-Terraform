# CoreGameplay Effects

This subsystem executes gameplay effects on the server. It is the shared action layer used by item actions, abilities, deployables, and projectile hits.

## Main pieces

- `EffectDefinition` is the authoring payload for one effect entry.
- `EffectFactory` converts `EffectDefinition.type` into a concrete `IEffect`.
- `TargetResolver` builds the target list for self, area, directional, or explicit execution.
- `EffectExecutor` is the runtime entry point that resolves targets and applies effects.
- `ProjectileVisualService`, `ProjectilePool`, `PooledProjectile`, and related classes support projectile-style execution.
- `SpawnPrefabRegistry`, `ImpactFxRegistry`, and `SoundRegistrySO` provide data-driven lookup for spawned visuals and sounds.

## Effect types currently created by `EffectFactory`

- `DealDamage`
- `HitscanDamage`
- `HealInstant`
- `ApplyBuff`
- `RemoveBuffSource`
- `SpawnPrefab`
- `MineNetworkResource`
- `Continuous`
- `StopContinuous`
- `Scan`
- `ScanResourceEffect`
- `SpawnProjectile`
- `SpawnImpact`
- `PlaySound`
- `ChainDamage`

## Runtime flow

1. An item, ability, projectile, or spawned object creates an `EffectDefinition`.
2. `EffectExecutor` resolves targets through `TargetResolver`.
3. `EffectFactory.Create(...)` builds the concrete effect implementation.
4. The effect applies gameplay changes to the resolved targets.
5. Optional visual and audio helpers spawn projectile visuals, impact FX, or sounds.

## Targeting model

- `Self` targets the source entity.
- `Area` finds targets in a radius.
- `Directional` resolves targets in front of the source.
- `Explicit` uses targets already supplied by the caller, such as projectile hit callbacks.

Ownership filtering is handled through `OwnershipFilter` and can limit effects to same-owner or different-owner targets.

## Projectile and deployable support

The subsystem already contains reusable support for:

- server projectiles and hitscan-style firing
- pooled client projectile visuals
- impact FX dispatch
- spawned prefabs such as zones, drones, or turrets
- device behaviors like `TurretBehaviour`, `ShieldGridBehaviour`, `RepairDroneBehaviour`, `OverloadPulseBehaviour`, and `DamageZone`

## Known gap

`EffectType` still contains `DealDamageHitscan` and `MeleeDamage`, but `EffectFactory` does not currently create implementations for those enum values. New content should use the effect types that are actually wired into the factory.
