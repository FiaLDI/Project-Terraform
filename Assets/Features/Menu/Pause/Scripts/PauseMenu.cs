using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu I;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    private bool isOpen = false;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        // Меню по умолчанию скрыто
        gameObject.SetActive(false);

        // ===== ПРОВЕРЯЕМ КНОПКИ И ВЕШАЕМ ЛИСТЕНЕРЫ =====
        if (resumeButton == null)
            Debug.LogError("[PauseMenu] resumeButton is NULL — назначь кнопку в инспекторе!", this);
        else
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (settingsButton == null)
            Debug.LogError("[PauseMenu] settingsButton is NULL — назначь кнопку в инспекторе!", this);
        else
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (exitButton == null)
            Debug.LogError("[PauseMenu] exitButton is NULL — назначь кнопку в инспекторе!", this);
        else
            exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnEnable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed += OnPausePressed;
            pauseAction.action.Enable();
        }
        else
        {
            Debug.LogWarning("[PauseMenu] pauseAction не присвоен или action == null");
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed -= OnPausePressed;
            pauseAction.action.Disable();
        }
    }

    // ================= INPUT =================

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
        Debug.Log("[PauseMenu] Open()");
        isOpen = true;
        gameObject.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        Debug.Log("[PauseMenu] Resume()");
        isOpen = false;
        gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ================= BUTTON HANDLERS =================

    private void OnResumeClicked()
    {
        Debug.Log("[PauseMenu] Resume button CLICK");
        Resume();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[PauseMenu] Settings button CLICK");

        if (SettingsMenuManager.I != null)
        {
            // открываем настройки как вызванные из паузы
            SettingsMenuManager.I.OpenSettings(SettingsCaller.PauseMenu);
            // прячем само pause-меню, но не меняем Time.timeScale — этим займётся SettingsMenu
            gameObject.SetActive(false);
            isOpen = false;
        }
        else
        {
            Debug.LogWarning("[PauseMenu] SettingsMenuManager.I == null");
        }
    }

    private void OnExitClicked()
    {
        Debug.Log("[PauseMenu] Exit button CLICK");

        Time.timeScale = 1f;

        string mainMenuScene = "MainMenu";
        if (Application.CanStreamedLevelBeLoaded(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[PauseMenu] Scene '{mainMenuScene}' не в Build Settings. В редакторе просто ничего не делаю.");
#else
            Application.Quit();
#endif
        }
    }
}
