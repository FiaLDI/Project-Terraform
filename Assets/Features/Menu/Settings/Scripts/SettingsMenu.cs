using UnityEngine;
using UnityEngine.UI;
using Features.Input;

public class SettingsMenu : MonoBehaviour, IUIScreen
{
    public static SettingsMenu I;

    [SerializeField] private GameObject panel;
    [SerializeField] private Button backButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;

    public InputMode Mode => InputMode.Pause;

    private SettingsMenuController controller;

    private void Awake()
    {
        I = this;

        panel.SetActive(false);
        controller = GetComponent<SettingsMenuController>();

        backButton.onClick.AddListener(OnBack);
        applyButton.onClick.AddListener(OnApply);
        resetButton.onClick.AddListener(OnReset);
    }

    // =========================
    // IUIScreen
    // =========================

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    // =========================
    // PUBLIC
    // =========================

    public void Open()
    {
        UIStackManager.I.Push(this);
    }

    // =========================
    // BUTTONS
    // =========================

    private void OnBack()
    {
        UIStackManager.I.Pop();
    }

    private void OnApply()
    {
        controller?.ApplySettings();
        SettingsStorage.Save();
        UIStackManager.I.Pop();
    }

    private void OnReset()
    {
        SettingsStorage.ResetToDefaults();
        controller?.LoadSettingsUI();
    }
}
