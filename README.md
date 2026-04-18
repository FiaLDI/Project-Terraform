
Download latest build:
https://github.com/FiaLDI/Project-Terraform/releases

# Project: Terraform

Co-op sci-fi survival/exploration game built with Unity.

## Overview

Project: Terraform is a cooperative sci-fi action game where players control autonomous robots exploring procedural zones, gathering resources, and building infrastructure to prepare a hostile planet for colonization.


## Features
1–4 player co-op gameplay
Procedural mission zones
Robot classes with distinct roles (combat, mining, engineering, support)
Resource gathering and crafting systems
Base building and infrastructure
Enemy AI (ECS-based)
Multiplayer support (in progress)
## Architecture
Feature-based structure
Separation of concerns:
Domain / Application / Infrastructure
Gradual migration from legacy systems
Partial use of Unity ECS (Jobs + Burst)
## Project Structure

```
Assets/
  CoreGameplay/   # Core gameplay logic and shared systems
  Features/       # Feature modules (AI, quests, multiplayer, etc.)
  Graphical/      # Rendering, visuals, effects
  Infrastructure/ # Unity integration (MonoBehaviours, bootstrap, services)
  Player/         # Player-related logic
  Resources/      # Unity resources (Addressables/Resources)
  UI/             # User interface
  World/          # World, levels, environment

ProjectSettings/  # Unity project settings
Packages/         # Dependencies
```

## Tech Stack
Unity (2022+ / 2023 LTS)
C#
Unity ECS (partial)
Unity Netcode (WIP)

### Status

Work in progress.
Core systems are being refactored and migrated to a new architecture.
