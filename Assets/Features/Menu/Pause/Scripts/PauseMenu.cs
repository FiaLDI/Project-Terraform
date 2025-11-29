using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu I;

    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Input")]
    public InputActionReference pauseAction;

    private bool isOpen = false;

    private void Awake()
    {
        I = this;
        gameObject.SetActive(false);

        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(ExitToMenu);
    }

    private void OnEnable()
    {
        if (pauseAction?.action != null)
        {
            pauseAction.action.performed += OnPausePressed;
            pauseAction.action.Enable();
        }
        else
        {
            Debug.LogWarning("[PauseMenu] pauseAction не назначен — ESC работать не будет!");
        }
    }

    private void OnDisable()
    {
        if (pauseAction?.action != null)
        {
            pauseAction.action.performed -= OnPausePressed;
            pauseAction.action.Disable();
        }
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        Toggle();
    }

    public void Toggle()
    {
        if (isOpen) Resume();
        else Open();
    }

    public void Open()
    {
        isOpen = true;
        gameObject.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isOpen = false;
        gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OpenSettings()
    {
        SettingsMenuManager.I.OpenSettings(SettingsCaller.PauseMenu);
    }

    private void ExitToMenu()
    {
        string mainMenuScene = "MainMenu";

        if (Application.CanStreamedLevelBeLoaded(mainMenuScene))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Scene '{mainMenuScene}' не в Build Settings");
#else
            Application.Quit();
#endif
        }
    }
}
