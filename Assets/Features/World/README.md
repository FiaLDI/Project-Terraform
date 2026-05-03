# Features World

This subsystem contains reusable networked world interactables and scene objects that players can trigger or operate.

## Included feature groups

- `Containers`
- `Doors`
- `Elevators`
- `Levers`
- `Terminals`

## Common model

Most objects in this folder follow the same pattern:

- a network controller stores authoritative state
- a local view applies that state visually
- interaction commands are funneled through the scene-binding multiplayer infrastructure

## Examples

- `DoorNetworkController` tracks open state, activation mode, trigger occupants, and manual interaction.
- `TerminalNetworkController` tracks powered and busy state.
- Other groups in this folder follow the same scene-bound controller/view pattern for their own state.

## Integration point

These world objects rely on the `Features/Multiplayer/SceneBinding` infrastructure so they can stay bound to persistent scene identities instead of being treated like disposable per-client UI.
