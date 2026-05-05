# World Resources

This subsystem contains mineable world resources, resource-node runtime logic, and resource-related drops.

## Main pieces

- `ResourceSO` is the authoring asset for resource content.
- `ResourceNodeModel` stores resource-node state.
- `ResourceNodeNetwork` is the network-facing runtime for a resource node.
- `ResourceNodePresenter` and `ResourceNodeSpawner` connect data to scene presentation and spawning.
- `MiningService` and `ResourceDropService` handle harvesting and resulting drops.

## Integration points

- `Player` and item actions interact with this folder through mining and scanning.
- `CoreGameplay/Effects` can target world resources via dedicated effect paths.
- Procedural world generation can place these resources into the environment.
