using Features.Inventory;
using Features.Player;
using UnityEngine;

public class UpgradeStation : MonoBehaviour, IInteractable
{
    [SerializeField] private UpgradeProcessor processor;
    [SerializeField] private RecipeDatabase recipeDB;
    [SerializeField] private UpgradeStationUIController ui;

    public string InteractionPrompt => "Улучшить предмет";

    private bool initialized;

    public RecipeSO[] GetRecipes()
        => recipeDB.GetForUpgrade();

    public bool Interact()
    {
        if (!initialized)
            InitForLocalPlayer();

        ui.Open();
        return true;
    }

    // ======================================================
    // INIT (lazy, player-scoped)
    // ======================================================

    private void InitForLocalPlayer()
    {
        IInventoryContext inventory = LocalPlayerContext.Inventory;
        if (inventory == null)
        {
            Debug.LogError("[UpgradeStation] Local inventory not available");
            return;
        }

        processor.Init(inventory);
        ui.Init(this, processor, inventory);

        initialized = true;
    }
}
