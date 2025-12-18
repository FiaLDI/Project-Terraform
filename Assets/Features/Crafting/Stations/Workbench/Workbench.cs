using Features.Inventory;
using Features.Player;
using UnityEngine;

public class Workbench : MonoBehaviour, IInteractable
{
    [SerializeField] private CraftingProcessor processor;
    [SerializeField] private RecipeDatabase recipeDB;
    [SerializeField] private WorkbenchUIController ui;

    public string InteractionPrompt => "Открыть верстак";

    private bool initialized;

    public RecipeSO[] GetRecipes()
        => recipeDB.GetForWorkbench();

    public bool Interact()
    {
        if (!initialized)
            InitForLocalPlayer();

        ui.Toggle();
        return true;
    }

    private void InitForLocalPlayer()
    {
        IInventoryContext inventory = LocalPlayerContext.Inventory;
        if (inventory == null)
        {
            Debug.LogError("[Workbench] Local inventory not available");
            return;
        }

        processor.Init(inventory);
        ui.Init(this, processor, inventory);

        initialized = true;
    }
}
