using System;
using System.Collections;
using UnityEngine;
using Features.Items.Domain;
using Features.Inventory;
using Features.Inventory.Application;
using Features.Player;
using Features.Inventory.Domain;

public class UpgradeProcessor : MonoBehaviour
{
    public event Action<ItemInstance> OnStart;
    public event Action<float> OnProgress;
    public event Action<ItemInstance> OnComplete;

    private IInventoryContext inventory;

    private InventorySlotRef currentSlot;

    private bool isProcessing;
    private Coroutine currentRoutine;

    // ======================================================
    // INIT
    // ======================================================

    public void Init(IInventoryContext inventory)
    {
        this.inventory = inventory;
    }

    // ======================================================
    // PUBLIC API
    // ======================================================

    public void BeginUpgrade(UpgradeRecipeSO recipe, ItemInstance inst, InventorySlotRef slotRef)
    {
        Debug.Log($"[UpgradeProcessor] BeginUpgrade item={inst.itemDefinition.id} lvl={inst.level} slot={slotRef.Section}[{slotRef.Index}]");

        if (isProcessing)
        {
            Debug.Log("[UpgradeProcessor] Already processing, cancel");
            return;
        }

        if (recipe == null || inst == null || inst.itemDefinition == null)
            return;

        if (inventory == null)
            return;

        int maxLevels = inst.itemDefinition.upgrades?.Length ?? 0;
        if (inst.level >= maxLevels)
        {
            Debug.Log("[UpgradeProcessor] Max level reached");
            return;
        }

        if (!inventory.Service.HasIngredients(recipe.ingredients))
        {
            Debug.Log("[UpgradeProcessor] Missing ingredients");
            return;
        }

        currentSlot   = slotRef;
        currentRoutine = StartCoroutine(Process(recipe, inst));
    }

    // ======================================================
    // PROCESS
    // ======================================================

    private IEnumerator Process(UpgradeRecipeSO recipe, ItemInstance inst)
    {
        isProcessing = true;

        OnStart?.Invoke(inst);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, recipe.duration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            OnProgress?.Invoke(elapsed / duration);
            yield return null;
        }

        Finish(recipe, inst);
    }

    // ======================================================
    // COMPLETE
    // ======================================================

    private void Finish(UpgradeRecipeSO recipe, ItemInstance inst)
    {
        isProcessing  = false;
        currentRoutine = null;

        Debug.Log($"[UpgradeProcessor] Finish item={inst.itemDefinition.id} lvl={inst.level} slot={currentSlot.Section}[{currentSlot.Index}]");

        var player = LocalPlayerContext.Player;
        if (player == null)
            return;

        var net = player.GetComponent<InventoryStateNetwork>();
        if (net == null)
            return;

        var cmd = new InventoryCommandData
        {
            Command  = InventoryCommand.UpgradeItem,
            RecipeId = recipe.recipeId,
            Section  = currentSlot.Section,
            Index    = currentSlot.Index
        };

        Debug.Log($"[UpgradeProcessor] Send Upgrade cmd: recipe={cmd.RecipeId}, section={cmd.Section}, index={cmd.Index}");
        net.RequestInventoryCommand(cmd);

        OnComplete?.Invoke(inst);
    }

    // ======================================================
    // OPTIONAL
    // ======================================================

    public bool IsProcessing => isProcessing;

    public void Cancel()
    {
        if (!isProcessing)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = null;
        isProcessing = false;
    }
}
