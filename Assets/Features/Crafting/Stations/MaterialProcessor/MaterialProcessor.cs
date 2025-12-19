using UnityEngine;
using Features.Player;
using Features.Inventory;

public class MaterialProcessor : MonoBehaviour, IInteractable
{
    [SerializeField] private CraftingProcessor processor;
    [SerializeField] private RecipeDatabase recipeDB;
    [SerializeField] private MaterialProcessorUIController ui;

    public string InteractionPrompt => "Переработать материалы";

    private bool initialized;

    public RecipeSO[] GetRecipes()
        => recipeDB.GetForProcessor();

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
            Debug.LogError("[MaterialProcessor] Local inventory not available");
            return;
        }

        processor.Init(inventory);
        ui.Init(this, processor, inventory);

        initialized = true;
    }
}
