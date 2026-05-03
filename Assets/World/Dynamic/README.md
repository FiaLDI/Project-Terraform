# World Dynamic

This branch is currently the lightest part of `Assets/World`.

## Current role

Use this folder for world-side runtime content that is dynamic in nature but does not clearly belong to:

- procedural biome generation in `Biomes`
- resource nodes in `Resources`
- fixed authored spaces in `Static`
- scene entry points in `Scenes`

## Practical expectation

If future systems need temporary world events, dynamic encounters, runtime-only environmental controllers, or other non-static world logic, this is the natural place to grow them.
