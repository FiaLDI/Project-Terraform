using UnityEngine;

public class Workbench : MonoBehaviour, IInteractable
{
    [SerializeField] private CraftingProcessor processor;
    [SerializeField] private RecipeDatabase recipeDB;
    [SerializeField] private WorkbenchUIController ui;

    public string InteractionPrompt => "Открыть верстак";

    private void Start()
    {
        ui.Init(this, processor);
    }

    public RecipeSO[] GetRecipes() => recipeDB.GetForWorkbench();

    public bool Interact()
    {
        Debug.Log("Workbench Interact() CALLED!!");

        ui.Toggle();
        return true;
    }
}
