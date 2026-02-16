using UnityEngine;
using UnityEngine.InputSystem;
using Features.Player;

namespace Features.Quests.UnityIntegration
{
    public sealed class QuestJournalInputHandler :
        MonoBehaviour,
        IInputContextConsumer
    {
        private PlayerInputContext input;
        private QuestUIRuntime questUI;

        private InputAction togglePlayer;
        private InputAction toggleUI;

        private bool subscribed;

        // ======================================================
        // INPUT BIND
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            if (input != null)
                UnbindInput(input);
            input = ctx;

            if (input == null)
                return;

            questUI = Object.FindAnyObjectByType<QuestUIRuntime>(
                FindObjectsInactive.Include);

            if (questUI == null)
            {
                Debug.LogError("[QuestJournalInputHandler] QuestUIRuntime not found");
                return;
            }

            togglePlayer = input.Actions.Player.FindAction("ToggleQuests", true);
            toggleUI = input.Actions.UI.FindAction("ToggleQuests", true);

            togglePlayer.performed += OnToggle;
            toggleUI.performed += OnToggle;

            togglePlayer.Enable();
            toggleUI.Enable();

            subscribed = true;
        }

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (!subscribed || input != ctx)
                return;

            if (togglePlayer != null)
            {
                togglePlayer.performed -= OnToggle;
                togglePlayer.Disable();
                togglePlayer = null;
            }

            if (toggleUI != null)
            {
                toggleUI.performed -= OnToggle;
                toggleUI.Disable();
                toggleUI = null;
            }

            input = null;
            subscribed = false;
        }

        // ======================================================
        // ACTION
        // ======================================================

        private void OnToggle(InputAction.CallbackContext _)
        {
            if (UIStackManager.I == null)
                return;

            if (UIStackManager.I.HasScreens)
            {
                var top = UIStackManager.I.Peek();
                if (top is QuestJournalScreen)
                    UIStackManager.I.Pop();

                return;
            }

            questUI.OpenJournal();
        }
    }
}
