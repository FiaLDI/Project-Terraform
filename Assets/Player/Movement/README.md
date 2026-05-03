# Player Movement

This subsystem owns deterministic player movement, prediction, interpolation, and network reconciliation.

## Main pieces

- `MoveCommand` and `PlayerState` are the core movement payloads.
- `DeterministicMovement` performs the actual local/server simulation.
- `PlayerNetworkController` handles prediction, input buffering, authoritative state replication, and reconciliation.
- `RemoteInterpolation` and `PlayerView` smooth non-owner presentation.

## Runtime flow

1. `Input` produces a `PlayerInputState`.
2. `PlayerNetworkController` converts that into `MoveCommand`.
3. The owner predicts locally while the server simulates authoritatively.
4. The resulting `PlayerState` is replicated and reconciled back into the owner and remote views.

## Scope

This folder is specifically about locomotion and its netcode. Camera, interaction, and animation live in neighboring `Player` folders.
