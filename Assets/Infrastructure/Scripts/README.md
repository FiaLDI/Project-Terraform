# Infrastructure Scripts

This folder contains the shared runtime plumbing used across scenes and subsystems.

## Main pieces

- `BootstrapRoot` is the persistent root that tracks the current local player.
- `LocalPlayerContext` is the static access point used by UI, stations, and input-adjacent code to reach the local player and local inventory.
- `GamePhase`, `ServerGamePhase`, and `PhaseAssert` provide project-wide phase gating.
- `PlayerRegistry`, `PlayerRegistryECS`, and `PlayerEcsBinder` connect player objects to global lookup and ECS-facing helpers.
- `PlayerUIRoot`, `UIRegistry`, and `PlayerBoundUIView` support shared player UI composition.
- `ServerServicesSpawner`, `WorldServicesSpawner`, and `SceneTransitionService` help bring runtime services online.

## Structure

- `Application` contains lightweight service and helper logic.
- `Domain` contains shared enums and contracts such as game phases.
- `UnityIntegration` contains MonoBehaviours and scene-facing entry points.

## Why it matters

Many systems in `Features` and `Player` assume that local-player lookup, phase progression, and shared service spawning are already handled here.
