# Player Input

This subsystem wraps Unity Input System actions and exposes player-facing handlers for gameplay and UI.

## Main pieces

- `PlayerInputContext` owns `PlayerInput` and the generated `GameInput` wrapper.
- `InputSystem_Actions` is the generated input-actions bridge.
- Handler components such as `MovementInputHandler`, `CameraInputHandler`, `AbilityInputHandler`, `InventoryInputHandler`, `QuestJournalInputHandler`, `StatsInputHandler`, and pause/UI handlers translate input into subsystem calls.
- `PlayerInputState` is the runtime state payload consumed by movement and other systems.

## Runtime role

This folder centralizes how local input is enabled, disabled, and mapped into gameplay systems, instead of letting each subsystem talk directly to Unity input assets.
