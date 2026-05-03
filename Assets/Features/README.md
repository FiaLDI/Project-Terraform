# Features

`Features` contains the gameplay feature layer built on top of `CoreGameplay`. These modules combine stats, buffs, effects, inventory, UI, persistence, and networking into player-facing game systems.

## Subsystems

- `Abilities` manages active class abilities and their server-side execution.
- `Crafting` handles recipe lookup, station flows, processors, and crafting UI.
- `Enemy` contains enemy configs, ECS AI systems, targeting, combat setup, and enemy presentation.
- `Equipment` equips active-slot items, spawns world and FPS models, and applies equipped buffs.
- `Inventory` owns the bag and active-slot model, command handling, save/load, and quest item events.
- `Items` defines item assets and runtime item action execution through the effects system.
- `Multiplayer` owns session state, server lifecycle, spawn/bootstrap flows, and scene-bound networked objects.
- `Progress` stores local player progression, character profiles, XP, level, and related save data.
- `Quests` manages quest assets, runtime services, event-driven progress, persistence, rewards, and quest UI.
- `World` contains reusable networked world interactables such as doors, terminals, elevators, levers, and containers.

## Typical feature flow

1. `Progress` selects the active character.
2. `Multiplayer` restores the player session and binds world state.
3. `Inventory` and `Equipment` restore active items and equipped runtime.
4. `Items`, `Abilities`, and `CoreGameplay/Effects` execute player actions on the server.
5. `Quests` and `Progress` react to gameplay events and update long-term state.
6. `World` and `Enemy` provide the interactable and combat-facing environment around those systems.
