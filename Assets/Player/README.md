# Player

`Player` contains the player-centric runtime stack: camera, input, interaction, movement, visuals, and the player prefab itself.

## Subsystems

- `Camera` manages runtime camera state, follow behavior, aim rays, and crosshair-facing UI helpers.
- `Input` converts Unity input actions into gameplay-facing handlers and a shared input context.
- `Interaction` resolves what the player can target or use and drives interaction prompts.
- `Movement` contains deterministic movement state, prediction, and network reconciliation.
- `Prefabs` stores the main player prefab.
- `Visual` owns player model presets, animation hookup, sockets, and death-burst visuals.

## Typical flow

1. The player prefab is spawned.
2. `Infrastructure` registers the local player and shared UI roots.
3. `Input` gathers local actions.
4. `Movement`, `Camera`, and `Interaction` consume those actions.
5. `Visual` and `Equipment` present the current player state to the local player and other clients.
