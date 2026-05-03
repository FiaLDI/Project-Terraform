# READMEAI

## Purpose

This file is a high-context project map for AI agents and new contributors working inside `Assets/`.

It is intentionally broader than the regular `README.md`:

- it explains the architecture, not just folder names
- it highlights runtime flows and subsystem boundaries
- it lists important data assets and integration bridges
- it calls out current assumptions and known risks

## Scope

This document describes the project from the point of view of `Assets/`.

Included:

- runtime code under `Assets`
- content structure under `Assets`
- subsystem relationships
- major data assets loaded at runtime

Not covered in depth:

- `Packages/`
- `ProjectSettings/`
- CI/build tooling outside `Assets`

## One-paragraph summary

This is a Unity multiplayer game project with a strongly server-authoritative gameplay core. The central loop is built from `CoreGameplay` systems such as stats, buffs, passives, effects, and classes. Concrete game features live in `Features` and include abilities, items, inventory, equipment, crafting, progress, quests, multiplayer session state, and world interactables. The actual player runtime is assembled from `Player` subsystems such as camera, input, interaction, movement, and visuals. The world side is split between procedural environment generation in `Assets/World` and reusable networked world interactables in `Assets/Features/World`. `Infrastructure` provides bootstrap, local-player discovery, registries, game phases, and shared UI/service glue. Much of the project is data-driven through ScriptableObject registries and assets loaded from `Assets/Resources/Databases`.

## Top-level folders

### `CoreGameplay`

Shared gameplay foundation.

Main subsystems:

- `Buffs`
- `Classes`
- `Effects`
- `Passives`
- `Stats`

This is the gameplay kernel that other branches build on.

### `Features`

Concrete player-facing systems built on top of `CoreGameplay`.

Current major branches:

- `Abilities`
- `Crafting`
- `Enemy`
- `Equipment`
- `Inventory`
- `Items`
- `Multiplayer`
- `Progress`
- `Quests`
- `World`

### `Player`

Player runtime assembly.

Current branches:

- `Camera`
- `Input`
- `Interaction`
- `Movement`
- `Prefabs`
- `Visual`

### `World`

Environment-side runtime and environment content.

Current branches:

- `Biomes`
- `Dynamic`
- `Resources`
- `Scenes`
- `Static`

### `Infrastructure`

Bootstrap and global runtime glue.

Current branches:

- `Prefabs`
- `Scripts`

### `Graphical`

Shared graphical assets and shader-side resources.

Observed content:

- fonts
- sprites
- shader source
- shader graphs
- graphics settings assets
- resource-side graphical assets

### `Resources`

Unity `Resources` assets used by runtime lookup.

Important subfolder:

- `Databases`

### `UI`

Reusable UI layer. This branch was not separately documented in detail in the current README pass, but it clearly contains:

- widgets and button systems
- modal screens
- settings
- stats UI
- tooltip UI
- world map UI
- main menu flows
- world generator UI
- upgrade/progression UI

### `Editor`

Reserved for editor-only tooling. Currently looks sparse at the top level.

## Core architectural model

### 1. Server-authoritative gameplay

Most meaningful gameplay execution happens on the server.

Examples:

- abilities are validated and executed on the server
- buffs and passives live on server-owned runtime components
- inventory commands are routed through server-side flows
- world resource nodes are networked and mined through server logic
- many world interactables use FishNet network controllers with `SyncVar`, `ServerRpc`, and `ObserversRpc`

Clients mostly receive:

- presentation state
- cooldowns
- channel state
- HP / energy snapshots
- movement reconciliation data
- quest state
- interactable state

### 2. Data-driven content

Many systems are driven by ScriptableObjects.

Examples:

- classes
- abilities
- buffs
- passives
- item definitions
- recipes
- quest assets
- quest chains
- world configs
- biome configs
- enemy configs
- progression trees
- visual presets

This gives the project a strong authoring-data layer separate from runtime logic.

### 3. Layered folder conventions

Many branches follow variants of:

- `Application`
- `Domain`
- `Data`
- `UnityIntegration`
- `Net`
- `UI`
- `Configs` or `Config`
- `Prefabs`

The intended meaning is usually:

- `Domain`: core models, value types, contracts
- `Application`: orchestration logic and service-level behavior
- `Data`: ScriptableObjects, registries, persistence DTOs
- `UnityIntegration`: MonoBehaviours, scene hookups, concrete runtime bindings
- `Net`: networking payloads or net-specific components
- `UI`: presentation layer

### 4. Entity bridge pattern

One important recurring pattern is the runtime bridge component that lets systems attach to the same entity without tightly coupling.

The most important example is `StatsBuffTarget`, which links:

- stats
- buffs
- passives
- effect targeting
- ownership logic

## Global runtime flow

High-level startup and play flow:

1. Infrastructure boots shared services and scene-level roots.
2. Multiplayer/session systems restore or initialize player session state.
3. Progress restores the local active character.
4. Player systems come online: input, camera, movement, interaction, visuals.
5. Inventory restores bag and active slots.
6. Equipment resolves the selected active item.
7. Class application applies stat preset, passives, and ability loadout.
8. Buffs and passives affect stats.
9. Items and abilities execute effects on the server.
10. Quests and progression react to emitted gameplay events.
11. The world runtime streams environment, resources, enemies, and interactables around the player.

## Important runtime databases in `Assets/Resources/Databases`

Observed assets:

- `AbilityLibrary.asset`
- `BuffRegistry.asset`
- `ImpactRegistry.asset`
- `ItemRegistry.asset`
- `PassiveRegistry.asset`
- `PlayerClassLibrary.asset`
- `RecipeDatabase.asset`
- `SoundRegistry.asset`

Practical meaning:

- gameplay content is often looked up by id instead of by hard reference from scene objects
- runtime systems can rebuild state from ids, saves, or network payloads
- UI and loaders can resolve authoring assets late through registries

## `CoreGameplay` deep context

### `Stats`

Role:

- typed stat model
- not a generic key-value bag internally
- exposes a shared facade for modifiers

Known typed areas:

- health
- energy
- combat
- movement
- mining
- protect

Important components:

- `StatsFacade`
- `StatsOwnerBase`
- `PlayerStats`
- `EnemyStats`
- `TurretStats`
- `StatsNetSync`
- `MovementStatsSync`
- `UnifiedStatsUpdateSystem`

Why it matters:

- buffs and passives modify stats through stat keys
- abilities and effects consume live stat values
- players, enemies, turrets, and resource-like actors can expose stats in a consistent way

### `Buffs`

Role:

- manage runtime buff instances
- apply and expire effects
- handle stacking and lifetime
- publish aggregated buff state to UI

Important behavior:

- stacking is source-based
- `Duration` buffs expire automatically
- `WhileSourceAlive` buffs stay until removed by source cleanup

Important components:

- `BuffSO`
- `BuffInstance`
- `BuffService`
- `BuffSystem`
- `BuffExecutor`
- `BuffTickSystem`
- `StatsBuffTarget`

### `Passives`

Role:

- activate passive loadouts for an entity
- transform passive assets into persistent gameplay changes
- cache ability modifiers

Important components:

- `PassiveSO`
- `PassiveSystem`
- `PassiveService`
- `PassiveExecutor`
- `AbilityModifierSO`

Very important behavior:

- passive cleanup is source-based through the buff system
- a common passive pattern is applying `BuffSO` with `WhileSourceAlive`

### `Effects`

Role:

- execute generic gameplay payloads
- shared action layer for abilities, items, projectiles, and spawned objects

Observed effect coverage:

- damage
- hitscan damage
- healing
- apply/remove buff
- spawn prefab
- scan
- scan resources
- spawn projectile
- impact FX
- sound
- chain damage
- continuous effects
- mining

Important components:

- `EffectDefinition`
- `EffectFactory`
- `EffectExecutor`
- `TargetResolver`
- projectile and spawn registries

### `Classes`

Role:

- tie character class identity to stats, visuals, passives, abilities, and progression

Important components:

- `PlayerClassConfigSO`
- `PlayerClassLibrarySO`
- `PlayerClassService`
- `PlayerClassController`
- `PlayerStateNetAdapter`

Important flow:

- class selection
- apply stats preset
- assemble passives
- push passives into passive system
- push abilities into runtime

## `Features` deep context

### `Abilities`

Role:

- active abilities for the player or class
- server validation and execution

Important components:

- `AbilitySO`
- `AbilityService`
- `AbilityCaster`
- `AbilityCasterNetAdapter`
- `AbilityExecutor`
- `ClientAbilityView`

Important integration:

- class system provides base ability list
- passive modifiers can mutate runtime ability effect lists
- effects system executes the actual payload
- stats provide energy and related values

Known caveats from earlier analysis:

- channel abilities deserve careful review for possible double execution
- stat key `ability.cooldown` exists conceptually but is not fully wired into runtime cooldown logic

### `Items`

Role:

- define item assets and runtime item actions

Important components:

- `Item`
- `ItemActionDefinition`
- `ItemRuntimeContext`
- `ItemRegistrySO`

Runtime model:

- actions can have windup
- actions can burst
- actions can fire on release
- actions can tick continuously
- actions execute through `EffectExecutor`

### `Inventory`

Role:

- bag model
- active slot model
- add/remove/move/consume logic
- persistence
- quest event emission

Important components:

- `InventoryModel`
- `InventoryService`
- `InventoryManager`
- command pipeline and network wrappers

Important behavior:

- active slots are separate from bag
- active slot selection drives equipment
- changes persist into player progress for local owner
- item add/remove events are published to quest systems

### `Equipment`

Role:

- equip selected active-slot item
- spawn world weapon and owner-only FPS view model
- apply equipped buffs
- hand item usage over to item runtime

Important components:

- `EquipmentManager`
- `EquipmentRuntime`
- `EquipmentItemBuffApplier`
- `PlayerUsageController`
- `PlayerEquipmentNetwork`

Important behavior:

- active slots, not left/right runtime hands, are the current equipment model

### `Crafting`

Role:

- recipes
- crafting stations
- processors
- station UI

Important components:

- `RecipeSO` family
- `RecipeDatabase`
- `CraftingProcessor`
- `MaterialProcessor`
- `UpgradeProcessor`

Important behavior:

- local progress UI/process is separate from the final authoritative inventory mutation
- actual result application goes through inventory/network command paths

### `Progress`

Role:

- local long-term character/profile state

Important components:

- `PlayerProgressService`
- `PlayerProgressData`
- `PlayerProfile`
- `PlayerCharacterState`
- `PlayerProgressionRules`
- `ClassProgressionSO`
- `ProgressionNodeSO`

Important behavior:

- save file is `player_progress.json`
- active character stores class id, level, experience, and inventory snapshot
- other systems read from and write back into this state

### `Quests`

Role:

- quest assets and runtime state
- quest chains
- event-driven progress
- persistence
- rewards
- quest UI

Important components:

- `QuestAsset`
- `QuestChainAsset`
- `QuestService`
- `QuestChainService`
- `WorldQuestService`
- `PlayerQuestComponent`
- `QuestEventBus`
- quest UI classes

Important behavior:

- quests can survive respawn and reconnect through session-level persistence
- inventory events and other gameplay events feed quest progression
- XP rewards flow into progress systems

### `Multiplayer`

Role:

- session shell around the whole game
- reconnect-safe player data
- world readiness/bootstrap
- scene-bound network state

Important components:

- `ServerGameFlow`
- `SessionManager`
- `PlayerSession`
- scene-binding infrastructure
- world-ready runtime components

Player session currently stores:

- persistent id
- client binding
- player object
- inventory save data
- quest persistence state
- character identity
- nickname
- class id
- level
- experience
- passive ids
- pending world bootstrap payload

This branch is one of the most important cross-system glue layers in the project.

### `Enemy`

Role:

- enemy content, AI, combat data, presentation, LOD, ECS runtime

Important components:

- `EnemyConfigSO`
- `EnemyAIConfigSO`
- `EnemyCombatConfigSO`
- `EnemyRenderConfigSO`
- `EnemyDatabaseSO`
- ECS systems such as `EnemyAISystem`, `EnemyTargetingSystem`, `EnemyAggroSystem`, `EnemyDespawnSystem`

Important behavior:

- enemy logic is not just MonoBehaviour-based
- ECS and presentation/network layers are bridged together
- enemy population also plugs into world and biome runtime

### `Features/World`

Role:

- reusable networked world interactables

Observed groups:

- containers
- doors
- elevators
- levers
- terminals

Common pattern:

- network controller
- scene-bound identity
- view component
- `SyncVar`-driven state
- player interaction routed through scene-binding commands

## `Player` deep context

### `Input`

Role:

- input action ownership and handler fan-out

Important components:

- `PlayerInputContext`
- generated `GameInput`
- movement / camera / ability / inventory / stats / quest journal / pause handlers

### `Movement`

Role:

- deterministic movement
- prediction
- reconciliation
- remote interpolation

Important components:

- `MoveCommand`
- `PlayerState`
- `DeterministicMovement`
- `PlayerNetworkController`
- `RemoteInterpolation`
- `PlayerView`

This is a serious custom movement netcode area, not just default transform sync.

### `Camera`

Role:

- camera runtime service
- FOV
- shake
- follow/head binding
- aim helpers
- crosshair UI

Important components:

- `CameraRuntimeService`
- `CameraControlService`
- `CameraRegistry`
- `PlayerCameraController`
- `PlayerCameraNetAdapter`
- `AimRay`

### `Interaction`

Role:

- decide what the player can interact with
- route ray hits to interactables
- expose prompts
- connect to scene-bound world usage

Important components:

- `InteractionRayService`
- `InteractionService`
- `PlayerInteractionController`
- `SceneBoundInteractable`
- `InteractionPromptUI`

### `Visual`

Role:

- player presentation
- visual preset application
- sockets
- animation hookup
- death burst

Important components:

- `PlayerVisualController`
- `PlayerAnimationController`
- `CharacterSockets`
- `RobotVisualLibrarySO`
- `RobotVisualPresetSO`

Important behavior:

- model prefab can be swapped by preset
- sockets feed camera and equipment
- local/remote layer handling is done here

## `World` deep context

### `Biomes`

Role:

- procedural terrain and chunk streaming
- world presets and seed-driven world generation
- biome-based environment, spawn, fog, and atmosphere

Important components mentioned in existing project docs:

- `WorldConfig`
- `BiomeConfig`
- `BiomeRuntimeDatabase`
- `Chunk`
- `ChunkManager`
- `MeshDataGenerator`
- `TerrainMeshGenerator`
- `RuntimeSpawnerSystem`
- `ChunkedInstanceLODSystem`
- `BiomeFog`
- `BiomeAtmosphereController`
- `AdvancedWaterPlane`
- `BiomeEnemySpawner`

Important behavior:

- the world is synchronized by seed and world config selection, not by replicating a full terrain map
- server and client both rebuild compatible procedural runtime from shared rules
- server streaming has already been adapted for multiplayer, not just host-local streaming

This is one of the most complex subsystems in the project.

### `Resources`

Role:

- mineable world resource nodes and resource drops

Important components:

- `ResourceSO`
- `MiningService`
- `ResourceDropService`
- `ResourceNodeModel`
- `ResourceNodeNetwork`
- `ResourceNodePresenter`
- `ResourceNodeSpawner`

### `Scenes`

Role:

- world-related scene entry points

Observed scene folders:

- `BootstrapScene`
- `NetHubScene`

### `Static`

Role:

- fixed authored spaces

Observed groups:

- `Hub`
- `Boss`

### `Dynamic`

Role:

- currently the least developed top-level world branch
- likely intended for world-side runtime content that is dynamic but not part of procedural terrain or fixed authored spaces

## `Infrastructure` deep context

Role:

- bootstrap
- local-player lookup
- player registries
- game phases
- UI roots
- scene/service spawners

Important components:

- `BootstrapRoot`
- `LocalPlayerContext`
- `GamePhase`
- `ServerGamePhase`
- `PhaseAssert`
- `PlayerRegistry`
- `PlayerEcsBinder`
- `PlayerUIRoot`
- `UIRegistry`
- `ServerServicesSpawner`
- `WorldServicesSpawner`
- `SceneTransitionService`

Why it matters:

- many systems assume local-player resolution is available here
- many systems gate initialization through phases defined here

## `Graphical`, `Resources`, `UI`, `Editor`

### `Graphical`

Observed asset mix suggests this folder is the shared graphics resource branch:

- `.shader`
- `.shadergraph`
- `.cginc`
- fonts
- settings assets
- sprite-like assets

It looks like a place for rendering-side shared content rather than gameplay logic.

### `Resources`

Primary purpose:

- runtime-loadable databases under `Resources/Databases`

This folder is critical because several systems explicitly call `Resources.Load(...)`.

### `UI`

The UI branch is large and active even though we intentionally skipped dedicated README work there.

Observed signals:

- many MonoBehaviours
- multiple screen types implementing `IUIScreen`
- world generator UI
- pause UI
- settings UI
- main menu and character selection
- upgrade/progression modal UI
- tooltip and stat UI

This means UI is not a thin cosmetic layer. It actively participates in:

- world selection
- progression submission
- network requests
- pause/return flows
- quest journal and debug tooling

### `Editor`

Top-level editor branch looks light right now, but some editor tooling also exists inside feature branches, for example in quests and stats drawers.

## Strong integration bridges

These are the classes and patterns that matter a lot when navigating the codebase:

- `StatsBuffTarget`: stats + buffs + passives + ownership
- `PlayerStateNetAdapter`: class application + stats preset + passives + ability sync
- `InventoryManager`: runtime inventory + persistence + quest item events
- `LocalPlayerContext`: static access point for local-player dependent UI and interaction
- `PlayerSession`: reconnect-safe player state
- `EffectExecutor`: universal gameplay payload execution
- scene-bound network controllers in `Features/World`: persistent world interactions
- `WorldProvider` / procedural world bootstrap path in `World/Biomes`

## Conventions and assumptions

### Naming and organization

- runtime code is generally split by responsibility rather than by giant god-modules
- folder names usually describe either a domain or a runtime layer
- most assets are intended to be id-driven and registry-backed

### Authority model

- client input is usually intent only
- server decides gameplay truth
- clients mostly present replicated state

### Persistence model

- local long-term profile state is in `PlayerProgressService`
- session/reconnect-safe state is in multiplayer `PlayerSession`
- some world bootstrap state is transient and session-mediated

### Content authoring model

- use ScriptableObject assets for gameplay definitions
- use `Resources/Databases` for key global registries
- use prefabs for runtime assembly and spawned gameplay objects

## Known risks or incomplete areas

These are not necessarily bugs, but they are important context:

- `UI` is functionally important but still under-documented compared to core gameplay branches
- `World/Dynamic` is structurally present but currently light in concrete content
- some older deeper README files in the repo have encoding damage and should not be treated as clean source text
- some branches have richer data models than the currently applied runtime logic
- ability channel/cooldown edge cases have already been identified as areas worth extra caution
- world bootstrap and session payload handling remain a sensitive integration zone
- enemy population, biome balance, and streaming performance are likely to require play-mode balancing rather than only code reading

## Recommended reading order for an AI agent

If starting from zero:

1. `Assets/README.md`
2. `Assets/CoreGameplay/README.md`
3. `Assets/Features/README.md`
4. `Assets/Player/README.md`
5. `Assets/World/README.md`
6. `Assets/Infrastructure/README.md`

Then narrow by task:

- combat / buffs / passives / damage: `CoreGameplay`
- item use / equipment / inventory: `Features/Items`, `Inventory`, `Equipment`
- class loadout / abilities / progression: `Classes`, `Abilities`, `Progress`
- quests and persistence: `Quests`, `Progress`, `Multiplayer`
- movement / camera / interaction: `Player`
- procedural world and resources: `World/Biomes`, `World/Resources`
- world interactables: `Features/World`

## Short mental model

Use this simplified picture:

- `Infrastructure` boots the runtime
- `Multiplayer` and `Progress` restore who the player is
- `Player` makes that player controllable
- `CoreGameplay` defines how gameplay rules work
- `Features` turns those rules into actual game systems
- `World` provides the environment they operate in
- `UI` exposes all of that to the player

That is the project in one sentence:

This codebase is a server-authoritative, data-driven Unity game where `CoreGameplay` defines reusable gameplay rules, `Features` assembles them into systems, `Player` and `World` host the runtime, and `Infrastructure` plus `Multiplayer` keep the whole thing coherent.
