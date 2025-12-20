using UnityEngine;
using UnityEngine.InputSystem;
using Features.Player;

namespace Features.Quests.UnityIntegration
{
    public sealed class QuestJournalInputHandler :
        MonoBehaviour,
        IInputContextConsumer
    {
        private QuestUIRuntime questUI;

        private InputAction togglePlayer;
        private InputAction toggleUI;

        private bool subscribed;

        public void BindInput(PlayerInputContext ctx)
        {
            if (subscribed)
                return;

            questUI = Object.FindAnyObjectByType<QuestUIRuntime>(
                FindObjectsInactive.Include);

            if (questUI == null)
            {
                Debug.LogError("[QuestJournalInputHandler] QuestUIRuntime not found");
                return;
            }

            togglePlayer = ctx.Actions.Player.FindAction("ToggleQuests", true);
            toggleUI     = ctx.Actions.UI.FindAction("ToggleQuests", true);

            togglePlayer.performed += OnToggle;
            toggleUI.performed     += OnToggle;

            subscribed = true;

            Debug.Log("[QuestJournalInputHandler] Bound");
        }

        private void OnDisable()
        {
            if (!subscribed)
                return;

            togglePlayer.performed -= OnToggle;
            toggleUI.performed     -= OnToggle;

            subscribed = false;
        }

        private void OnToggle(InputAction.CallbackContext _)
        {
            if (UIStackManager.I == null)
                return;

            // toggle логика как в Inventory
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
