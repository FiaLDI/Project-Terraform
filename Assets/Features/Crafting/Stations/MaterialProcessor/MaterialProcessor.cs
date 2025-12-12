using UnityEngine;

public class MaterialProcessor : MonoBehaviour, IInteractable
{
    [SerializeField] private CraftingProcessor processor;
    [SerializeField] private RecipeDatabase recipeDB;
    [SerializeField] private MaterialProcessorUIController ui;

    public string InteractionPrompt => "Переработать материалы";

    private void Start()
    {
        ui.Init(this, processor);
    }

    public RecipeSO[] GetRecipes() => recipeDB.GetForProcessor();

    public bool Interact()
    {
        ui.Toggle();
        return true;
    }
}
