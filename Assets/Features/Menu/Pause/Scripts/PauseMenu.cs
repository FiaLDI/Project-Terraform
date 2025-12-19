using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Features.Input;

public class PauseMenu : MonoBehaviour, IUIScreen
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    public InputMode Mode => InputMode.Pause;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvas.enabled = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        resumeButton.onClick.AddListener(OnResume);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);
    }

    public void Show()
    {
        canvas.enabled = true;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvas.enabled = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Open()
    {
        UIStackManager.I.Push(this);
    }

    private void OnResume()
    {
        UIStackManager.I.Pop();
    }

    private void OnSettings()
    {
        SettingsMenu.I.Open();
    }

    private void OnExit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
