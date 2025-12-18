using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Features.Player;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu I;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private bool isOpen;
    private PlayerInputContext input;
    private bool subscribed;

    // ======================================================
    // LIFECYCLE
    // ======================================================

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        BindButtons();

        // Стартуем выключенными
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (input == null)
            input = LocalPlayerContext.Get<PlayerInputContext>();

        if (input == null)
        {
            Debug.LogError("[PauseMenu] PlayerInputContext not found");
            return;
        }

        input.Actions.UI.Cancel1.performed += OnCancelPressed;
        subscribed = true;
    }

    private void OnDisable()
    {
        if (!subscribed || input == null)
            return;

        input.Actions.UI.Cancel1.performed -= OnCancelPressed;
        subscribed = false;
    }

    private void BindButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        else
            Debug.LogError("[PauseMenu] resumeButton is NULL", this);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        else
            Debug.LogError("[PauseMenu] settingsButton is NULL", this);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
        else
            Debug.LogError("[PauseMenu] exitButton is NULL", this);
    }

    // ======================================================
    // INPUT
    // ======================================================

    private void OnCancelPressed(InputAction.CallbackContext _)
    {
        Toggle();
    }

    // ======================================================
    // PUBLIC API
    // ======================================================

    public void Toggle()
    {
        if (isOpen) Resume();
        else Open();
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;
        gameObject.SetActive(true);

        Time.timeScale = 0f;

        input?.Actions.Player.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (!isOpen)
            return;

        isOpen = false;
        gameObject.SetActive(false);

        Time.timeScale = 1f;

        input?.Actions.Player.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ======================================================
    // BUTTON HANDLERS
    // ======================================================

    private void OnSettingsClicked()
    {
        Debug.Log("[PauseMenu] Settings");

        if (SettingsMenuManager.I != null)
        {
            SettingsMenuManager.I.OpenSettings(SettingsCaller.PauseMenu);

            // PauseMenu прячем, но состояние времени не трогаем
            gameObject.SetActive(false);
            isOpen = false;
        }
    }

    private void OnExitClicked()
    {
        Debug.Log("[PauseMenu] Exit");

        Time.timeScale = 1f;

        const string mainMenuScene = "MainMenu";
        if (Application.CanStreamedLevelBeLoaded(mainMenuScene))
            SceneManager.LoadScene(mainMenuScene);
        else
            Application.Quit();
    }
}
