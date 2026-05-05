# Player Visual

This subsystem owns player model presentation, visual preset lookup, sockets, animation hookup, and death-burst visuals.

## Main pieces

- `PlayerVisualController` applies a visual preset, spawns the model, wires sockets, updates camera head binding, and can play the death-burst effect.
- `PlayerAnimationController` binds gameplay state into the spawned animator.
- `CharacterSockets` exposes important attachment points used by camera and equipment.
- `RobotVisualLibrarySO` and `RobotVisualPresetSO` provide data-driven visual preset lookup.

## Assets

- `Models` contains the actual character model assets and animation content.
- `Materials` contains textures and materials used by player-facing UI and visuals.
- `Prefabs/Player.prefab` consumes this folder's scripts and assets as part of the final player assembly.

## Integration points

- `Equipment` uses character sockets for weapon attachment.
- `Camera` uses the head/socket bindings.
- Death and local/remote presentation differences are coordinated here rather than in gameplay systems.
