# Features Quests

This subsystem manages quest definitions, runtime quest state, event-driven progress, persistence, rewards, and quest UI.

## Main pieces

- `QuestAsset` and `QuestChainAsset` define quest and chain content.
- `QuestService`, `QuestChainService`, and `WorldQuestService` manage runtime state on the server.
- `QuestEventBus` is the event-driven bridge from gameplay systems into quest progress.
- `PlayerQuestComponent` is the main Unity/server entry point for a player.
- `QuestUIRuntime` and debug UI classes render quest state on the client.

## Runtime flow

1. Quest assets are loaded into runtime definitions.
2. `PlayerQuestComponent` starts or restores the player quest services.
3. Gameplay systems publish events such as item add/remove and other quest triggers.
4. Quest services update progress, completion, failure, rewards, and chain advancement.
5. Network state replicates quest status to the client UI.

## Integration points

- `Inventory` publishes item events that quests can consume.
- `Progress` receives XP-related quest rewards.
- `Multiplayer` stores persistent quest state in `PlayerSession`, allowing restore after respawn or reconnect.

## Scope

The subsystem already models both player-specific and world-facing quest progression, rather than only a local single-player journal.
