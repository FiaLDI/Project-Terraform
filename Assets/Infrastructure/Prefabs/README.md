# Infrastructure Prefabs

This folder stores shared infrastructure prefabs rather than gameplay content prefabs.

## Current prefabs

- `BootstrapPrefab` is the persistent scene/bootstrap root used to bring core runtime services online.
- `PlayerUIRoot` is the shared root for player-bound UI composition.

## Why it exists

Keeping these prefabs under `Infrastructure` makes it clear that they support project startup and common runtime plumbing, not a specific gameplay subsystem.
