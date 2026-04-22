# Equipment System

## Overview
`Features/Equipment` now works with a single selected active slot (0..2), not left/right hands.

System responsibilities:
- equip world item model from selected active slot;
- spawn owner-only FPS view model;
- control FPS hands visibility based on camera mode and view model presence;
- apply/remove equipped buffs on server;
- forward use input to network adapter.

## Inventory Model (Current)
Active slots:
- `ActiveSlot0`
- `ActiveSlot1`
- `ActiveSlot2`

Selected slot:
- `ActiveSlotIndex` (0..2)

Legacy left/right runtime sections are not used.

## Core Components
- `UnityIntegration/EquipmentManager.cs`
  Main equip pipeline, view model spawn, pose update, muzzle wiring.
- `UnityIntegration/PlayerUsageController.cs`
  Gameplay input (`Use`, `SecondaryUse`, `Reload`, `Scroll`) and active-slot switching.
- `UnityIntegration/EquipmentRuntime.cs`
  Caches and runs `ItemRuntimeContext`.
- `Features/Multiplayer/Scripts/UnityIntegration/Net/PlayerUsageNetAdapter.cs`
  Server-authoritative item use, aim sync, FX replication.
- `Player/Camera/UnityIntegration/CameraRegistry.cs`
  Owns FPS hands prefab(s), weapon socket, and one-hand pose support.

## Equip Flow
1. `InventoryManager` changes -> `EquipmentManager.EquipFromInventory()`.
2. Selected active slot item is resolved from `Model.ActiveSlotIndex`.
3. World equipped prefab is spawned in `activeItemSocket` (3rd person representation).
4. Owner-only FPS view model is spawned into camera weapon socket if `viewModelPrefab` exists.
5. Muzzles are passed to `PlayerUsageNetAdapter` (`worldMuzzle` + `viewMuzzle`).
6. Weapon pose is applied to owner animation/camera and synchronized via `PlayerEquipmentNetwork`.
7. Equipped buffs are applied/removed on server.

## FPS/TPS Hands Rules
- Hands visibility is controlled by equipment state, not by camera toggle alone.
- Hands are visible only when:
  - local player is in FPS mode; and
  - current item has spawned `currentViewWeapon`.
- If active slot is empty, switching `FPS <-> TPS` must not show hands.
- In TPS, FPS hands stay hidden.

## Weapon Pose
- Pose source: `Item.GetWeaponPose()`.
- `isTwoHanded == true` forces pose `2`.
- One-hand visual support in `CameraRegistry`:
  - `fpsArmsPrefab` (default);
  - `fpsArmsOneHandPrefab` (used when pose is one-hand);
  - optional animator parameter `arms_onehand_pose`.

## Multiplayer Sync
- Active slot changes go through `InventoryCommand.SetActiveSlot`.
- Server updates inventory state and replicates active slots to observers.
- Other players see equipped world weapon and usage effects via network replication.
- Local FPS view model remains owner-only.

## Inventory Persistence
- Inventory is persisted on every change via `InventoryManager.NotifyInventoryChanged()`.
- Local owner save path:
  - `BuildSaveData()` -> `activeCharacter.characterInventoryData` -> `PlayerProgressService.Save()`.
- Manual dirty mark exists for direct mutations:
  - `InventoryManager.MarkDirty()` (used, for example, after upgrade level increment).

## Required Item Data
- `equippedPrefab` (world equipped model)
- optional `viewModelPrefab` (owner FPS model)
- optional `equippedBuffs`
- `actions[]`
- pose data:
  - `isTwoHanded`
  - `weaponPose` (0 none, 1 one-hand, 2 two-hand)
