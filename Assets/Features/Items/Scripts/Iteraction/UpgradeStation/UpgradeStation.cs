using UnityEngine;

public class UpgradeStation : MonoBehaviour, IInteractable
{
    [SerializeField] private UpgradeProcessor processor;
    [SerializeField] private RecipeDatabase recipeDB;
    [SerializeField] private UpgradeStationUIController ui;

    public string InteractionPrompt => "Улучшить предмет";

    private void Start()
    {
        ui.Init(this, processor);
    }

    public RecipeSO[] GetRecipes() => recipeDB.GetForUpgrade();

    public bool Interact()
    {
        ui.Toggle();
        ui.OnOpenUI();
        return true;
    }
}
