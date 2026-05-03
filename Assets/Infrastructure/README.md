# Infrastructure

`Infrastructure` contains the shared bootstrap and runtime glue used across the project. It is the layer that wires player registration, global services, phases, and common UI roots together.

## Included parts

- `Prefabs` contains shared bootstrap and UI root prefabs.
- `Scripts` contains application helpers, phase definitions, registries, and bootstrap MonoBehaviours.

## Typical role

This folder is not a gameplay feature on its own. Instead it provides the common runtime scaffolding that other systems depend on, such as local-player discovery, phase gating, world/service spawning, and shared UI registration.
