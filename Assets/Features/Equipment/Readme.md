# Equipment System

## Overview
`Features/Equipment` handles:
- equipping the currently selected active inventory slot;
- spawning local FPS view model for owner;
- applying/removing equipped item buffs;
- forwarding item usage input (`Primary`/`Secondary`/`Reload`) to network usage adapter.

## Active Slots Model
Inventory now has only active slots:
- `ActiveSlot0`
- `ActiveSlot1`
- `ActiveSlot2`

And one selected index:
- `ActiveSlotIndex` (0..2)

There is no left/right hand inventory section in runtime logic.

## Main Components
- `UnityIntegration/EquipmentManager.cs`
  Equips item from selected active slot and updates weapon pose + muzzles.
- `UnityIntegration/PlayerUsageController.cs`
  Reads gameplay input and forwards usage + scroll slot switch requests.
- `UnityIntegration/EquipmentRuntime.cs`
  Runtime cache for `ItemRuntimeContext` by `(ItemInstance, ItemActionType)`.
- `../Multiplayer/.../PlayerUsageNetAdapter.cs`
  Server-authoritative usage execution, aim sync and observers FX replication.

## Equip Flow
1. `EquipmentManager.Init(IInventoryContext)` subscribes to inventory changes.
2. On change, `EquipFromInventory()` reads `Model.ActiveSlotIndex` and gets slot item.
3. Equipped prefab is spawned on configured socket.
4. Server applies equipped buffs from `itemDefinition.equippedBuffs`.
5. Owner gets `viewModelPrefab` in FPS weapon socket (if configured).
6. Muzzles are passed to `PlayerUsageNetAdapter`.
7. Weapon pose is synchronized via `PlayerEquipmentNetwork`.

## Use Flow
1. `PlayerUsageController` binds `Use`, `SecondaryUse`, `Reload`, `Scroll`.
2. `Scroll` sends `InventoryCommand.SetActiveSlot` to server.
3. Server updates inventory `ActiveSlotIndex` and replicates active slots state.
4. `Use`/`SecondaryUse`/`Reload` trigger `PlayerUsageNetAdapter.ActionStart/Stop`.
5. Server runs `ItemRuntimeContext`, executes effects and replicates world visuals.

## Required ItemDefinition Data
- `equippedPrefab`
- optional `viewModelPrefab`
- optional `equippedBuffs`
- `actions[]` with effect definitions (for example projectile spawn)
