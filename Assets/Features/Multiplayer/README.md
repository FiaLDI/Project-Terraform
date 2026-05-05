# Features Multiplayer

This subsystem owns the multiplayer shell around gameplay: server lifecycle, player sessions, reconnect-safe state, scene-bound world objects, and network bootstrap flows.

## Main pieces

- `ServerGameFlow` tracks the server lifecycle from startup to running state.
- `SessionManager` maps connected clients to persistent player sessions.
- `PlayerSession` stores reconnect-safe state such as character identity, progression, passive ids, inventory data, quest state, and pending world quest bootstrap.
- `SceneBinding` contains the reusable infrastructure for scene-bound networked objects and views.

## Runtime flow

1. The server starts and advances through loading, world preparation, and running states.
2. A client login resolves or creates a `PlayerSession`.
3. Session data is used to restore player-specific state such as inventory, class, passives, quests, and progression.
4. Scene-bound network controllers drive persistent world objects such as doors, terminals, levers, and elevators.
5. On disconnect, the session stays alive even though the client binding is removed.

## Why it matters

This folder is the glue that lets feature systems survive respawn and reconnect instead of treating every spawn as a fresh isolated player.
