# Player Interaction

This subsystem handles what the player can target, pick up, or use, and how that state is surfaced in the UI.

## Main pieces

- `InteractionRayService` builds raycast hits used for interaction checks.
- `InteractionService` resolves an `IInteractable` from a hit result.
- `InteractionResolver`, `PlayerInteractionController`, and nearby registrators connect interaction logic to the player runtime.
- `SceneBoundInteractable` bridges player interaction into persistent world objects.
- `InteractionPromptUI` and `InteractableUIObject` present interaction state to the player.

## Integration points

- `Camera` supplies aim/ray providers.
- `World` supplies many of the actual interactable targets.
- `Items` and pickups can also register through this layer.
