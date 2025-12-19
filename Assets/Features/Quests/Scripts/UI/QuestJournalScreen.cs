using UnityEngine;
using Features.Input;

namespace Features.Quests.UnityIntegration
{
    public class QuestJournalScreen : MonoBehaviour, IUIScreen
    {
        [SerializeField] private GameObject journalPanel;

        public InputMode Mode => InputMode.Inventory;

        private void Awake()
        {
            journalPanel.SetActive(false);
        }

        // =========================
        // IUIScreen
        // =========================

        public void Show()
        {
            journalPanel.SetActive(true);
        }

        public void Hide()
        {
            journalPanel.SetActive(false);
        }

        // =========================
        // PUBLIC
        // =========================

        public void Open()
        {
            UIStackManager.I.Push(this);
        }
    }
}
