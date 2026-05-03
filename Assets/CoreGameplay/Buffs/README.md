# CoreGameplay Buffs

This subsystem owns runtime buffs for an entity, including application, ticking, removal, stacking rules, and client-facing buff state sync.

## Main pieces

- `BuffSO` stores authoring data such as `buffId`, effects, duration, `isStackable`, and UI fields.
- `BuffInstance` is the runtime object for one applied buff on one target from one source.
- `BuffService` stores active instances and handles add, remove, tick, and source-based cleanup.
- `BuffSystem` is the server-side entity component that owns one `BuffService`.
- `BuffExecutor` applies, ticks, and expires the effects declared by the buff asset.
- `BuffTickSystem` updates all registered buff systems on the server.
- `StatsBuffTarget` bridges buffs to the entity `IStatsFacade`.
- `ClientBuffView`, `BuffHUD`, and `BuffIconUI` expose aggregated buff state to the client UI.

## Runtime flow

1. A source calls `BuffSystem.Add(...)`.
2. `BuffSystem` forwards the request into `BuffService.AddBuff(...)`.
3. If the buff is non-stackable and the same source already applied the same `buffId`, the existing instance is reused.
4. Otherwise a new `BuffInstance` is created and `BuffExecutor.Apply(...)` is called.
5. On every server tick, `BuffService.Tick(dt)` updates lifetime and removes expired instances.
6. `BuffExecutor.Tick(...)` runs per-buff tick behavior.
7. When a buff is removed, `BuffExecutor.Expire(...)` rolls back its gameplay effect.

## Lifetime model

- `BuffLifetimeMode.Duration` counts down `Remaining` and expires automatically.
- `BuffLifetimeMode.WhileSourceAlive` stays active until the owning source is explicitly removed.

This source-based removal model is important because passives, equipment, and area emitters all rely on `RemoveBySource(...)` or `RemoveBySourceAndId(...)` for cleanup.

## Stacking rule

The current stacking rule is source-based:

- `isStackable = false` means one source cannot duplicate the same `buffId`.
- Reapplying a non-stackable duration buff from the same source refreshes duration instead of creating another instance.
- Different sources may still contribute separate instances of the same `buffId`.

## Client sync contract

`BuffSystem.SyncActiveBuffs()` aggregates active buffs by `buffId` and publishes them as encoded `buffId|count` states. The client UI receives stack counts, but not full per-instance details such as remaining time or exact source identity.
