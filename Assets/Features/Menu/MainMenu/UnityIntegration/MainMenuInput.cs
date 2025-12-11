using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class MainMenuInput : MonoBehaviour
{
    private InputAction _cancelAction;

    private void Awake()
    {
        // Ищем UI-модуль, который уже висит на EventSystem
        var uiModule = Object.FindFirstObjectByType<InputSystemUIInputModule>();
        if (uiModule == null)
        {
            Debug.LogError("[MainMenuInput] InputSystemUIInputModule not found in scene");
            enabled = false;
            return;
        }

        var asset = uiModule.actionsAsset;
        if (asset == null)
        {
            Debug.LogError("[MainMenuInput] Actions Asset is not assigned on InputSystemUIInputModule");
            enabled = false;
            return;
        }

        // Можно так: "UI/Cancel1" (map/action)
        _cancelAction = asset.FindAction("UI/Cancel1", true);
        if (_cancelAction == null)
        {
            Debug.LogError("[MainMenuInput] Action 'UI/Cancel1' not found in Actions Asset");
            enabled = false;
            return;
        }

        _cancelAction.performed += OnCancel;
    }

    private void OnEnable()
    {
        _cancelAction?.Enable();
    }

    private void OnDisable()
    {
        _cancelAction?.Disable();
    }

    private void OnDestroy()
    {
        if (_cancelAction != null)
            _cancelAction.performed -= OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        // Сначала — настройки, если открыты
        if (SettingsMenuManager.I != null && SettingsMenuManager.I.SettingsMenuOpen)
        {
            SettingsMenuManager.I.CloseSettings();
            return;
        }

        // Иначе — назад по FSM
        if (MainMenuFSM.Instance != null)
            MainMenuFSM.Instance.Back();
    }
}
