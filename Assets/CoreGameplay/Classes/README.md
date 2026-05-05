# CoreGameplay Classes

This folder defines playable class data and the server-side flow that applies a class to a player.

## What is here

- `Configs/Class` currently contains class assets such as `miner` and `Tech`.
- `PlayerClassConfigSO` stores class identity, `StatsPresetSO`, visual preset, passive list, ability list, and progression asset.
- `PlayerClassLibrarySO` is the lookup asset used to resolve classes by id.
- `PlayerClassService` is a lightweight in-memory selector over loaded class configs.
- `PlayerClassController` applies the chosen class locally to the player runtime objects.
- `PlayerStateNetAdapter` is the authoritative server entry point for class application.

## Runtime flow

1. `PlayerStateNetAdapter.ApplyClass(classId)` stores the requested class id.
2. Once `GamePhase.StatsReady` is reached, the adapter resets player stats and applies `cfg.preset`.
3. The adapter builds the final passive list from class passives plus runtime progression passives.
4. That passive list is sent through `PassiveNetAdapter.ServerSetPassives(...)`.
5. `PlayerClassController.ApplyClass(...)` selects the config, updates `AbilityCaster`, and raises the class-applied signal after the buff phase is ready.
6. The adapter sends updated movement and ability state to the client.

## Important detail

`PlayerClassController` does not directly activate passives. The actual passive application happens in `PlayerStateNetAdapter` through `PassiveNetAdapter` and `PassiveSystem`. The controller is responsible for class selection, ability assignment, and phase coordination.
