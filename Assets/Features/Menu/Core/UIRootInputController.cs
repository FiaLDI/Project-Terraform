using UnityEngine;
using UnityEngine.InputSystem;
using Features.Player;
using Features.Quests.UnityIntegration;

public class UIRootInputController : MonoBehaviour
{
    private PlayerInputContext input;
    private QuestUIRuntime questUI;

    private void Awake()
    {
        input = LocalPlayerContext.Get<PlayerInputContext>();
        questUI = FindFirstObjectByType<QuestUIRuntime>();
    }

    private void OnEnable()
    {
        if (input == null) return;

        var ui = input.Actions.UI;

        ui.FindAction("Cancel").performed += OnCancel;
        ui.FindAction("ToggleQuests").performed += OnToggleQuests;
    }

    private void OnDisable()
    {
        if (input == null) return;

        var ui = input.Actions.UI;

        ui.FindAction("Cancel").performed -= OnCancel;
        ui.FindAction("ToggleQuests").performed -= OnToggleQuests;
    }

    private void OnCancel(InputAction.CallbackContext _)
    {
        if (UIStackManager.I.HasScreens)
            UIStackManager.I.Pop();
    }

    private void OnToggleQuests(InputAction.CallbackContext _)
    {
        if (UIStackManager.I.IsTop<QuestJournalScreen>())
            UIStackManager.I.Pop();
        else
            questUI.OpenJournal();
    }

}
