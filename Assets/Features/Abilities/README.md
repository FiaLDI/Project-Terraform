# Features Abilities

This subsystem manages active abilities, their server-authoritative execution, and the client state needed for ability UI.

## Main pieces

- `AbilitySO` defines identity, timing, cost, cast type, and effect payloads.
- `AbilityService` validates casts, energy cost, cooldown, and channel lifecycle.
- `AbilityCaster` owns the runtime ability list for one entity and drives execution.
- `AbilityCasterNetAdapter` is the network entry point from client input to server cast requests.
- `AbilityExecutor` executes the ability effects on the server.
- `ClientAbilityView`, `AbilityHUD`, and `AbilitySlotUI` expose ability state to the client.

## Runtime flow

1. The client requests a cast through the network adapter.
2. The server resolves the ability from the current slot list.
3. `AbilityService` validates cooldown, cost, and channel state.
4. `AbilityCaster` builds runtime effect copies and applies passive ability modifiers.
5. The final effect list is executed on the server through `EffectExecutor`.

## Integration points

- `Classes` supplies the base ability loadout.
- `Passives` can inject `AbilityModifierSO` changes without mutating the source `AbilitySO`.
- `Stats` provides energy and related runtime values used by cast validation.

## Client sync

The client receives presentation-oriented state such as cooldowns and channel status, while actual ability logic remains server-authoritative.
