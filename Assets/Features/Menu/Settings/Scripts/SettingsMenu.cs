using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public GameObject panel;
    public Button backButton;
    public Button applyButton;
    public Button resetButton;

    public bool IsOpen { get; private set; }

    private SettingsMenuController controller;

    private void Awake()
    {
        HideInstant();

        controller = GetComponent<SettingsMenuController>();

        if (backButton != null)
            backButton.onClick.AddListener(Back);

        if (applyButton != null)
            applyButton.onClick.AddListener(Apply);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetSettings);
    }

    public void Show()
    {
        IsOpen = true;
        panel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        IsOpen = false;
        panel.SetActive(false);

        SettingsMenuManager.I.CloseSettings();
    }

    public void HideInstant()
    {
        IsOpen = false;
        panel.SetActive(false);
    }

    public void Back()
    {
        Hide();
    }

    private void Apply()
    {
        if (controller != null)
            controller.ApplySettings();

        SettingsStorage.Save();
        Hide();
    }

    private void ResetSettings()
    {
        Debug.Log("[SettingsMenu] Resetting to defaults!");

        SettingsStorage.ResetToDefaults();

        if (controller != null)
            controller.LoadSettingsUI();

    }
}
