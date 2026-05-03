# Features Inventory

This subsystem owns the player inventory model, active slots, save/load data, command routing, and inventory-related quest events.

## Main pieces

- `InventoryModel` stores the bag plus `ActiveSlot0`, `ActiveSlot1`, `ActiveSlot2`, and the selected active-slot index.
- `InventoryService` is the main application-layer service for add, remove, move, extract, consume, and ingredient checks.
- `InventoryManager` is the Unity entry point that creates the model, wires services, loads save data, and persists changes.
- `Application`, `Middleware`, and `Net` contain the command pipeline used by multiplayer inventory operations.

## Runtime flow

1. `InventoryManager` creates the model and service on startup.
2. Save data or network state populates bag and active-slot contents.
3. Gameplay calls into `InventoryService` for add, remove, move, consume, and recipe ingredient checks.
4. `InventoryManager.NotifyInventoryChanged()` persists owner inventory and notifies listeners.
5. Item add/remove events are published into the quest event bus.

## Important behavior

- The bag and active slots are separate sections.
- The selected active slot drives `Equipment`.
- Persistence for the local owner is stored through `PlayerProgressService`.
- Network state can overwrite local runtime state through the inventory net adapters.
