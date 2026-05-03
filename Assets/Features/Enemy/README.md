# Features Enemy

This subsystem defines enemy content and runtime behavior, including configs, ECS AI systems, targeting, combat data, prefabs, and enemy UI.

## Main pieces

- `EnemyConfigSO` is the top-level enemy asset and references AI, render, combat, and stats configs.
- `Scripts/Application` contains ECS systems such as `EnemyAISystem`, `EnemyTargetingSystem`, aggro, LOS, despawn, and spatial update systems.
- `Scripts/Data` contains databases and enemy config assets.
- `Prefabs`, `UI`, and `Model` contain enemy presentation assets.

## Runtime model

- AI logic is driven through ECS systems and jobs rather than a single MonoBehaviour brain.
- Targeting and aggro are handled in dedicated systems before AI movement and action selection.
- Combat and survivability are configured through linked combat and stats assets.

## Notes

- There is also an older deeper note at `Enemy/Scripts/README.md` focused on LOD, instancing, and prefab structure.
- This top-level README is the current subsystem overview for the full `Features/Enemy` folder.
