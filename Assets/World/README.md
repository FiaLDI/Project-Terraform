# World

`World` contains the environment-side runtime of the project: procedural terrain, world resources, authored scenes, and static world spaces.

## Subsystems

- `Biomes` is the procedural world-generation branch. It handles world config selection, seed-driven generation, chunk streaming, terrain meshes, procedural spawn, and biome-driven atmosphere.
- `Resources` contains world resource nodes, mining, drops, and resource spawning/presentation.
- `Scenes` contains scene entry points such as bootstrap and network hub scenes.
- `Static` contains authored world spaces like hub and boss areas.
- `Dynamic` is the lightest branch right now and is a good place for runtime-driven world content that does not belong to procedural terrain or fixed authored spaces.

## Runtime role

This folder is where the playable environment becomes concrete:

1. A world preset or scene is selected.
2. Procedural or authored world content is brought online.
3. Resource nodes, enemies, and world objects are placed or streamed.
4. Player-facing systems interact with this environment through movement, interaction, combat, and quests.

## Relationship to the rest of the project

- `Features/World` contains reusable networked interactables such as doors, terminals, elevators, and levers.
- `Assets/World` contains the broader environment runtime and environment content around those interactables.
- `Features/Enemy` plugs into this branch by populating and updating combatants inside the world.

## Where to go next

- Read [Biomes](</C:/Unity Projects/New-Game/Assets/World/Biomes/Readme.md>) for the procedural world-generation side.
- Read the local README files in `Resources`, `Scenes`, and `Static` for the authored and resource-driven parts of the environment.
