using UnityEngine;
using UnityEngine.UI;
using Features.Camera.Application;
using Features.Camera.Domain;
using Features.Camera.UnityIntegration;

public abstract class BaseStationUI : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected Button closeButton;

    [Header("Recipe UI")]
    [SerializeField] protected Transform recipeListContainer;
    [SerializeField] protected RecipeButtonUI recipeButtonPrefab;
    [SerializeField] protected RecipePanelUI recipePanel;

    protected bool isOpen = false;
    public bool IsOpen => isOpen;

    private ICameraControlService cameraControl;

    protected virtual void Awake()
    {
        cameraControl = CameraServiceProvider.Control;

        if (closeButton != null)
            closeButton.onClick.AddListener(() => Toggle());

        canvas.gameObject.SetActive(false);
    }

    public void Init(RecipeSO[] recipes)
    {
        Populate(recipes);
    }

    protected void Populate(RecipeSO[] recipes)
    {
        foreach (Transform t in recipeListContainer)
            Destroy(t.gameObject);

        foreach (var recipe in recipes)
        {
            var btn = Instantiate(recipeButtonPrefab, recipeListContainer);
            btn.Init(recipe, this);
        }
    }

    public void Toggle()
    {
        if (!isOpen)
            Open();
        else
            Close();
    }

    private void Open()
    {
        if (isOpen) return;
        isOpen = true;

        Debug.Log("Station UI OPEN");

        canvas.gameObject.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        cameraControl?.SetInputBlocked(true);

        UIStationManager.I.OpenStation(this);
    }

    private void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        Debug.Log("Station UI CLOSE");

        canvas.gameObject.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        cameraControl?.SetInputBlocked(false);

        UIStationManager.I.CloseStation(this);
    }

    public virtual void ShowRecipe(RecipeSO recipe)
    {
        recipePanel.ShowRecipe(recipe);
    }
}
