# Features Equipment

This subsystem equips the currently selected active-slot item, spawns the proper world and owner-only view models, and connects equipped items to runtime item usage.

## Main pieces

- `EquipmentManager` is the main equip pipeline and visual/runtime coordinator.
- `EquipmentRuntime` caches and runs `ItemRuntimeContext` for equipped item actions.
- `PlayerUsageController` handles local use input and active-slot switching.
- `EquipmentItemBuffApplier` applies or removes equipped buffs on the server.
- `PlayerEquipmentNetwork` and related multiplayer classes synchronize visible equipment state.

## Current equipment model

- The system uses one selected active slot out of `ActiveSlot0`, `ActiveSlot1`, and `ActiveSlot2`.
- It no longer relies on legacy left/right-hand runtime sections.

## Runtime flow

1. `Inventory` changes or active-slot changes trigger reequip.
2. `EquipmentManager` resolves the selected slot item.
3. A world equipped prefab is attached for shared third-person representation.
4. An owner-only FPS view model is spawned when available.
5. Equipped buffs are applied on the server.
6. Item use is forwarded into runtime item execution.

## Integration points

- `Inventory` provides active-slot contents.
- `Items` supplies the item definition, actions, view model, and equipped buffs.
- `Multiplayer` replicates visible equipment state to other players.
