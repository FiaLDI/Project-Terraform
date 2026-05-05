# Player Prefabs

This folder stores the assembled player prefab used at runtime.

## Current prefab

- `Player.prefab` is the main composed player object that brings together movement, camera, interaction, visuals, equipment, stats, abilities, and other player-facing systems.

## Why it matters

This prefab is the concrete integration point where many otherwise separate `Player`, `Features`, and `Infrastructure` components meet.
