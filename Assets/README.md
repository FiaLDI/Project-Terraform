# Assets

This folder is the main game workspace. It combines low-level gameplay systems, player runtime, feature modules, world generation, art assets, and project infrastructure.

## Top-level structure

- `CoreGameplay` contains the reusable gameplay foundation: stats, buffs, passives, effects, and classes.
- `Features` contains player-facing systems built on top of `CoreGameplay`, such as items, inventory, equipment, quests, crafting, multiplayer, and progress.
- `Player` contains the player runtime stack: camera, input, interaction, movement, prefab assembly, and visuals.
- `World` contains world simulation and world content, including procedural biomes, resources, scenes, and static world areas.
- `Infrastructure` contains bootstrap, registration, game phases, shared UI roots, and global runtime glue.
- `Graphical` contains shared visual assets such as fonts, sprites, shaders, settings, and graphics-side resources.
- `Resources` contains shared Unity `Resources` content, primarily databases and lookup assets loaded at runtime.
- `UI` contains reusable UI widgets and screens. This branch exists in the project, but detailed README work was intentionally skipped for this pass.
- `Editor` is reserved for editor-only tooling and support scripts.

## Architectural layers

### 1. Core systems

`CoreGameplay` is the gameplay kernel:

- `Stats` defines typed stat modules and the shared `StatsFacade`.
- `Buffs` manages runtime buff instances, ticking, stacking, and sync.
- `Passives` turns passive assets into long-lived gameplay changes.
- `Effects` executes server-authoritative gameplay payloads such as damage, healing, scans, projectiles, and prefab spawns.
- `Classes` binds a class id to visuals, preset stats, abilities, passives, and progression.

These systems are intentionally generic enough to be reused by multiple features.

### 2. Feature layer

`Features` combines core systems into concrete game features:

- `Abilities` executes active abilities on the server.
- `Items`, `Inventory`, and `Equipment` form the main item loop.
- `Crafting` and `Progress` manage medium- and long-term player progression.
- `Quests` reacts to gameplay events and persistent player state.
- `Enemy` and `Features/World` extend the game world with combatants and interactables.
- `Multiplayer` provides the session and network shell around those systems.

### 3. Player runtime

`Player` is the runtime assembly around the local or remote player:

- `Input` captures player intent.
- `Movement` simulates and synchronizes locomotion.
- `Camera` provides aim and camera runtime behavior.
- `Interaction` decides what the player can use or target.
- `Visual` and `Equipment` present the player state.

### 4. World runtime

`World` contains the environment around the player:

- `Biomes` handles procedural world generation and streaming.
- `Resources` contains mineable and droppable world resources.
- `Scenes` contains entry scenes and bootstrap scenes.
- `Static` contains authored world zones such as hub and boss spaces.
- `Dynamic` is the placeholder branch for more dynamic world-side runtime content.

## Typical runtime flow

1. Infrastructure boots shared services and registers the local player.
2. Progress selects or restores the active character profile.
3. Multiplayer restores session-level data such as class, inventory, passives, quests, and progression.
4. Player systems enable camera, input, movement, interaction, and visuals.
5. Inventory and equipment restore the active item state.
6. Abilities and item actions execute through `CoreGameplay/Effects`.
7. Buffs and passives continuously feed back into stats.
8. Quests and progression react to gameplay events.
9. World and enemy systems provide the surrounding environment and targets.

## Important cross-folder bridges

- `StatsBuffTarget` connects stats, buffs, passives, and effects on an entity.
- `PlayerStateNetAdapter` connects multiplayer restore flow with class presets and passives.
- `InventoryManager` persists inventory into player progress and publishes quest-relevant item events.
- `LocalPlayerContext` is the common runtime access point used by many UI and station-facing systems.
- Scene-bound multiplayer controllers connect player interaction to persistent world objects.

## Content conventions visible in this repo

- Authoring data usually lives in `Config`, `Configs`, `Data`, or `Resources/Databases`.
- Runtime logic is usually split into `Application`, `Domain`, `UnityIntegration`, and sometimes `Net` or `UI`.
- Prefab-heavy branches keep runtime assembly assets in nearby `Prefabs`.
- Many gameplay systems are server-authoritative and replicate only the state that clients need for presentation.

## What to read first

- Start with [CoreGameplay](</C:/Unity Projects/New-Game/Assets/CoreGameplay/README.md>) for the gameplay kernel.
- Then read [Features](</C:/Unity Projects/New-Game/Assets/Features/README.md>) for the concrete systems.
- Then read [Player](</C:/Unity Projects/New-Game/Assets/Player/README.md>) and [World](</C:/Unity Projects/New-Game/Assets/World/README.md>) to understand how those systems meet at runtime.
- Use [Infrastructure](</C:/Unity Projects/New-Game/Assets/Infrastructure/README.md>) when you need the bootstrap and shared-service picture.
