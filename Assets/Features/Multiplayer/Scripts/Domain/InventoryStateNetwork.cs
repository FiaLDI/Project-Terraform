using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;
using System.Collections;
using Features.Equipment.UnityIntegration;

public sealed class InventoryStateNetwork : NetworkBehaviour
{
    private InventoryManager inventory;
    private bool syncing;
    private int applyCount;

    // ======================================================
    // LIFECYCLE
    // ======================================================

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        inventory = GetComponent<InventoryManager>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory != null)
            inventory.OnInventoryChanged += ServerOnInventoryChanged;

        StartCoroutine(InitialSnapshotRoutine());
    }

    [Server]
    private IEnumerator InitialSnapshotRoutine()
    {
        yield return null;
        ServerOnInventoryChanged();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (inventory != null)
            inventory.OnInventoryChanged -= ServerOnInventoryChanged;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        // remote owner → запрос полного состояния
        if (IsOwner)
            RequestFullState_Server();
    }

    // ======================================================
    // COMMANDS (CLIENT → SERVER)
    // ======================================================

    [ServerRpc(RequireOwnership = true)]
    public void RequestInventoryCommand(InventoryCommandData cmd)
    {
        Debug.Log(
            $"[InventoryStateNetwork] RequestInventoryCommand from client={Owner.ClientId} | " +
            $"Command={cmd.Command}, WorldItemNetId={cmd.WorldItemNetId}, ItemId={cmd.ItemId}, Qty={cmd.PickupQuantity}",
            this
        );

        if (inventory == null || inventory.Service == null)
        {
            Debug.LogError("[InventoryStateNetwork] RequestInventoryCommand: inventory or Service is null", this);
            return;
        }

        switch (cmd.Command)
        {
            case InventoryCommand.PickupWorldItem:
                HandlePickup(cmd);
                break;

            case InventoryCommand.MoveItem:
                HandleMove(cmd);
                break;

            case InventoryCommand.DropFromSlot:
                HandleDrop(cmd);
                break;

            case InventoryCommand.UpgradeItem:
                HandleUpgrade(cmd);
                break;

            case InventoryCommand.CraftRecipe:
                HandleCraft(cmd);
                break;

            default:
                Debug.LogWarning($"[InventoryStateNetwork] Unknown command: {cmd.Command}", this);
                break;
        }
    }


    [ServerRpc(RequireOwnership = true)]
    private void RequestFullState_Server()
    {
        ServerOnInventoryChanged();
    }

    // ======================================================
    // SERVER → CLIENT STATE
    // ======================================================

    [Server]
    private void ServerOnInventoryChanged()
    {
        // Guard: если уже синхронизируем, не запускать снова
        if (syncing || inventory == null || inventory.Model == null)
            return;

        syncing = true;
        try
        {
            var m = inventory.Model;

            // ---------- OWNER : FULL SNAPSHOT ----------
            var owner = Owner;
            if (owner != null)
            {
                if (!NetworkObject.Observers.Contains(owner))
                Debug.LogWarning($"[InventoryStateNetwork] Owner {owner.ClientId} is NOT observer of {NetworkObject.ObjectId}");
                var bag = new InventorySlotNet[m.main.Count];
                for (int i = 0; i < m.main.Count; i++)
                    bag[i] = ToNet(m.main[i].item);

                var left  = ToNet(m.leftHand.item);
                var right = ToNet(m.rightHand.item);

                Debug.Log("[InventoryStateNetwork] Sending snapshot to owner", this);
                TargetReceiveInventoryState(owner, bag, left, right);
            }

            // ---------- OBSERVERS : HANDS ONLY ----------
            ObserversReceiveHands(
                ToNet(m.leftHand.item),
                ToNet(m.rightHand.item)
            );
        }
        finally
        {
            syncing = false;
        }
    }

    [TargetRpc]
    private void TargetReceiveInventoryState(
        NetworkConnection _,
        InventorySlotNet[] bag,
        InventorySlotNet left,
        InventorySlotNet right)
    {
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        syncing = true;
        try
        {
            applyCount++;
            Debug.Log($"[InventoryStateNetwork] TargetReceiveInventoryState #{applyCount}", this);
            inventory.ApplyNetState(bag, left, right);
            
            var equipmentMgr = GetComponent<EquipmentManager>();
            if (equipmentMgr != null)
            {
                Debug.Log("[InventoryStateNetwork] Force EquipFromInventory after state sync", this);
                equipmentMgr.EquipFromInventory();
            }
        }
        finally
        {
            syncing = false;
        }
    }

    [ObserversRpc]
    private void ObserversReceiveHands(
        InventorySlotNet left,
        InventorySlotNet right)
    {
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        syncing = true;
        try
        {
            inventory.ApplyHandsNetState(left, right);
            
            var equipmentMgr = GetComponent<EquipmentManager>();
            if (equipmentMgr != null)
            {
                equipmentMgr.EquipFromInventory();
            }
        }
        finally
        {
            syncing = false;
        }
    }

    // ======================================================
    // NET CONVERSION
    // ======================================================

    private static InventorySlotNet ToNet(ItemInstance inst)
    {
        if (inst == null ||
            inst.IsEmpty ||
            inst.itemDefinition == null ||
            inst.quantity <= 0)
        {
            return new InventorySlotNet
            {
                itemId = null,
                quantity = 0,
                level = 0
            };
        }

        return new InventorySlotNet
        {
            itemId = inst.itemDefinition.id,
            quantity = inst.quantity,
            level = inst.level
        };
    }

    // ======================================================
    // HANDLERS (SERVER ONLY)
    // ======================================================

    private void HandleMove(InventoryCommandData cmd)
    {
        if (inventory?.Service == null)
        {
            Debug.LogWarning("[InventoryStateNetwork] HandleMove: inventory or Service is null");
            return;
        }

        try
        {
            inventory.Service.MoveItem(
                cmd.FromIndex,
                cmd.FromSection,
                cmd.ToIndex,
                cmd.ToSection
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InventoryStateNetwork] HandleMove error: {ex}", this);
        }
    }

    private void HandleDrop(InventoryCommandData cmd)
    {
        Debug.Log("[InventoryStateNetwork] HandleDrop START", this);

        // ✅ ЗАЩИТА 1: Все основные null-проверки
        if (inventory == null)
        {
            Debug.LogError("[InventoryStateNetwork] HandleDrop: inventory is NULL", this);
            return;
        }

        if (inventory.Service == null)
        {
            Debug.LogError("[InventoryStateNetwork] HandleDrop: inventory.Service is NULL", this);
            return;
        }

        if (inventory.Model == null)
        {
            Debug.LogError("[InventoryStateNetwork] HandleDrop: inventory.Model is NULL", this);
            return;
        }

        // ✅ ЗАЩИТА 2: Валидация индекса для Bag
        if (cmd.Section == InventorySection.Bag)
        {
            if (cmd.Index < 0 || cmd.Index >= inventory.Model.main.Count)
            {
                Debug.LogWarning($"[InventoryStateNetwork] HandleDrop: Invalid bag index {cmd.Index}, count={inventory.Model.main.Count}", this);
                return;
            }
        }

        // ✅ ЗАЩИТА 3: Получаем слот с обработкой ошибок
        InventorySlot sourceSlot = null;
        try
        {
            sourceSlot = GetSlot(cmd.Section, cmd.Index);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InventoryStateNetwork] HandleDrop: GetSlot exception: {ex.Message}", this);
            return;
        }

        if (sourceSlot == null)
        {
            Debug.LogWarning($"[InventoryStateNetwork] HandleDrop: sourceSlot is NULL", this);
            return;
        }

        // ✅ ЗАЩИТА 4: Проверяем содержимое слота
        if (sourceSlot.item == null)
        {
            Debug.LogWarning("[InventoryStateNetwork] HandleDrop: sourceSlot.item is NULL", this);
            return;
        }

        if (sourceSlot.item.IsEmpty)
        {
            Debug.LogWarning("[InventoryStateNetwork] HandleDrop: sourceSlot.item is EMPTY", this);
            return;
        }

        // ✅ ЗАЩИТА 5: Экстрактим предмет
        ItemInstance extracted = null;
        try
        {
            int amountToDrop = cmd.Amount <= 0 ? int.MaxValue : cmd.Amount;
            extracted = inventory.Service.ExtractFromSlot(cmd.Section, cmd.Index, amountToDrop);
            Debug.Log($"[InventoryStateNetwork] HandleDrop: Extracted {extracted?.itemDefinition?.name ?? "NULL"}", this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InventoryStateNetwork] HandleDrop: ExtractFromSlot exception: {ex.Message}", this);
            return;
        }

        if (extracted == null)
        {
            Debug.LogWarning("[InventoryStateNetwork] HandleDrop: ExtractFromSlot returned NULL", this);
            return;
        }

        if (extracted.IsEmpty)
        {
            Debug.LogWarning("[InventoryStateNetwork] HandleDrop: extracted.IsEmpty == true", this);
            return;
        }

        // ✅ ЗАЩИТА 6: Валидация позиции и направления
        if (float.IsNaN(cmd.WorldPos.x) || float.IsNaN(cmd.WorldPos.y) || float.IsNaN(cmd.WorldPos.z))
        {
            Debug.LogError($"[InventoryStateNetwork] HandleDrop: WorldPos contains NaN: {cmd.WorldPos}", this);
            inventory.Service.AddItem(extracted);
            return;
        }

        if (cmd.WorldPos.magnitude > 10000f) // Разумный лимит
        {
            Debug.LogError($"[InventoryStateNetwork] HandleDrop: WorldPos too far: {cmd.WorldPos}", this);
            inventory.Service.AddItem(extracted);
            return;
        }

        // ✅ ЗАЩИТА 7: Проверяем WorldItemDropService
        WorldItemDropService dropService = WorldItemDropService.I;

        if (dropService == null)
        {
            Debug.LogWarning("[InventoryStateNetwork] HandleDrop: WorldItemDropService.I is NULL, trying to find it on scene", this);
            dropService = FindObjectOfType<WorldItemDropService>();
        }

        if (dropService == null)
        {
            Debug.LogError("[InventoryStateNetwork] HandleDrop: WorldItemDropService not found anywhere!", this);
            inventory.Service.AddItem(extracted);
            return;
        }

        // ✅ Все проверки пройдены - дропаем
        try
        {
            Debug.Log($"[InventoryStateNetwork] Dropping {extracted.itemDefinition.name} x{extracted.quantity} at {cmd.WorldPos}", this);
            dropService.DropServer(extracted, cmd.WorldPos, cmd.WorldForward);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InventoryStateNetwork] HandleDrop: DropServer exception: {ex.Message}\n{ex.StackTrace}", this);
            try
            {
                inventory.Service.AddItem(extracted);
            }
            catch { }
        }
    }

    private void HandleUpgrade(InventoryCommandData cmd)
    {
        if (inventory?.Service == null) return;

        var recipe = RecipeDatabase.Instance?.GetRecipeById(cmd.RecipeId);
        if (recipe == null) return;

        if (!inventory.Service.HasIngredients(recipe.ingredients)) return;

        inventory.Service.ConsumeIngredients(recipe.ingredients);

        var slot = GetSlot(cmd.Section, cmd.Index);
        if (slot != null && !slot.item.IsEmpty)
        {
            slot.item.level++;
            Debug.Log($"[Server] Upgrade success: {slot.item.itemDefinition.id} lvl -> {slot.item.level}");

            ServerOnInventoryChanged();
        }
    }


    private void HandleCraft(InventoryCommandData cmd)
    {
        if (inventory?.Service == null)
        {
            Debug.LogError("[InventoryStateNetwork] HandleCraft: inventory or Service is null", this);
            return;
        }

        try
        {
            Debug.Log($"[CRAFT] Server craft {cmd.RecipeId}");

            var recipeDb = RecipeDatabase.Instance;
            if (recipeDb == null)
            {
                Debug.LogError("[CRAFT] RecipeDatabase.Instance is null");
                return;
            }

            var recipe = recipeDb.GetRecipeById(cmd.RecipeId);
            if (recipe == null)
            {
                Debug.LogError("[CRAFT] Recipe not found");
                return;
            }

            if (!inventory.Service.HasIngredients(recipe.ingredients))
            {
                Debug.LogWarning("[CRAFT] Missing ingredients on SERVER");
                return;
            }

            Debug.Log("[CRAFT] Ingredients OK, crafting...");
            inventory.Service.ConsumeIngredients(recipe.ingredients);

            var output = recipe.outputItem;
            if (output == null)
            {
                Debug.LogError("[CRAFT] Recipe output item is null");
                return;
            }

            bool added;
            if (output.isStackable)
            {
                added = inventory.Service.AddItem(
                    new ItemInstance(output, recipe.outputAmount)
                );
            }
            else
            {
                added = true;
                for (int i = 0; i < recipe.outputAmount; i++)
                    added &= inventory.Service.AddItem(
                        new ItemInstance(output, 1)
                    );
            }

            Debug.Log($"[CRAFT] AddItem result = {added}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InventoryStateNetwork] HandleCraft error: {ex}", this);
        }
    }

    private void HandlePickup(InventoryCommandData cmd)
    {
        Debug.Log(
            $"[InventoryStateNetwork] HandlePickup START | " +
            $"WorldItemNetId={cmd.WorldItemNetId}, ItemId='{cmd.ItemId}', Qty={cmd.PickupQuantity}, Lvl={cmd.PickupLevel}",
            this
        );

        if (inventory?.Service == null)
        {
            Debug.LogWarning("[InventoryStateNetwork] HandlePickup: inventory or Service is null", this);
            return;
        }

        try
        {
            // 0. Проверяем, существует ли объект
            if (cmd.WorldItemNetId > 0 &&
                !NetworkManager.ServerManager.Objects.Spawned.ContainsKey((int)cmd.WorldItemNetId))
            {
                Debug.LogWarning(
                    $"[InventoryStateNetwork] HandlePickup: WorldItem {cmd.WorldItemNetId} " +
                    $"NOT in Spawned (already consumed or invalid)",
                    this
                );
                return;
            }

            // 1. Валидация ItemId / Qty
            if (string.IsNullOrEmpty(cmd.ItemId) || cmd.ItemId == "UNKNOWN" || cmd.ItemId == "0")
            {
                Debug.LogError($"[InventoryStateNetwork] HandlePickup: INVALID ItemId='{cmd.ItemId}'", this);
                return;
            }

            if (cmd.PickupQuantity <= 0)
            {
                Debug.LogError($"[InventoryStateNetwork] HandlePickup: PickupQuantity={cmd.PickupQuantity} <= 0", this);
                return;
            }

            // 2. Ищем дефинишн
            var def = ItemRegistrySO.Instance?.Get(cmd.ItemId);
            if (def == null)
            {
                Debug.LogError(
                    $"[InventoryStateNetwork] HandlePickup: ItemDefinition '{cmd.ItemId}' NOT found in ItemRegistrySO",
                    this
                );
                return;
            }

            Debug.Log($"[InventoryStateNetwork] HandlePickup: ItemDefinition OK ({def.id})", this);

            // 3. Находим WorldItemNetwork и съедаем
            WorldItemNetwork worldItem = null;

            if (cmd.WorldItemNetId > 0 &&
                NetworkManager.ServerManager.Objects.Spawned.TryGetValue((int)cmd.WorldItemNetId, out var netObj))
            {
                worldItem = netObj.GetComponent<WorldItemNetwork>();
                if (worldItem == null)
                {
                    Debug.LogWarning(
                        $"[InventoryStateNetwork] HandlePickup: NetObject {cmd.WorldItemNetId} " +
                        "has NO WorldItemNetwork component",
                        this
                    );
                    return;
                }

                Debug.Log($"[InventoryStateNetwork] HandlePickup: WorldItemNetwork found, calling ServerConsume()", this);

                try
                {
                    worldItem.ServerConsume();
                    Debug.Log(
                        $"[InventoryStateNetwork] HandlePickup: ServerConsume OK (NetId={cmd.WorldItemNetId})",
                        this
                    );
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(
                        $"[InventoryStateNetwork] HandlePickup: ServerConsume EXCEPTION: {ex}",
                        this
                    );
                    return;
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[InventoryStateNetwork] HandlePickup: WorldItem {cmd.WorldItemNetId} " +
                    "not found in Spawned, skip AddItem",
                    this
                );
                return;
            }

            // 4. Добавляем в инвентарь
            var inst = new ItemInstance(def, cmd.PickupQuantity, cmd.PickupLevel);
            Debug.Log(
                $"[InventoryStateNetwork] HandlePickup: Try AddItem {def.id} x{cmd.PickupQuantity} L{cmd.PickupLevel}",
                this
            );

            if (!inventory.Service.AddItem(inst))
            {
                Debug.LogWarning("[InventoryStateNetwork] HandlePickup: AddItem returned FALSE", this);
                return;
            }

            int total = inventory.Service.GetItemCount(def);
            Debug.Log(
                $"[InventoryStateNetwork] HandlePickup: ✅ SUCCESS, now have total={total} of '{def.id}'",
                this
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InventoryStateNetwork] HandlePickup EXCEPTION: {ex}", this);
        }
    }



    private InventorySlot GetSlot(InventorySection section, int index)
    {
        if (inventory == null || inventory.Model == null)
            return null;

        return section switch
        {
            InventorySection.Bag => 
                index >= 0 && index < inventory.Model.main.Count 
                    ? inventory.Model.main[index] 
                    : null,
            InventorySection.LeftHand => inventory.Model.leftHand,
            InventorySection.RightHand => inventory.Model.rightHand,
            _ => null
        };
    }
}
