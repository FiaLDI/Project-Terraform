using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Features.Quests.Domain;
using System.Collections;
using System.Linq;

namespace Features.Quests.UnityIntegration
{
    public class QuestUIRuntime : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private Transform hudContainer;
        [SerializeField] private GameObject hudEntryTemplate;
        [SerializeField] private int maxHudQuests = 5;

        [Header("Journal")]
        [SerializeField] private Transform listParent;
        [SerializeField] private GameObject journalEntryTemplate;
        [SerializeField] private QuestJournalScreen journalScreen;

        [Header("Filter Buttons")]
        [SerializeField] private Button btnAll;
        [SerializeField] private Button btnActive;
        [SerializeField] private Button btnCompleted;

        [Header("Notifications")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TMP_Text notificationText;
        [SerializeField] private float notifyTime = 3f;

        private QuestService service;

        private readonly Dictionary<QuestId, GameObject> hudEntries = new();
        private readonly Dictionary<QuestId, GameObject> journalEntries = new();

        private enum QuestFilter { All, Active, Completed }
        private QuestFilter currentFilter = QuestFilter.All;

        // ============================================================
        // LIFECYCLE
        // ============================================================

        private void Awake()
        {
            notificationPanel?.SetActive(false);

            btnAll.onClick.AddListener(() => SetFilter(QuestFilter.All));
            btnActive.onClick.AddListener(() => SetFilter(QuestFilter.Active));
            btnCompleted.onClick.AddListener(() => SetFilter(QuestFilter.Completed));
        }

        private void Start()
        {
            StartCoroutine(ConnectWhenReady());
        }

        // ============================================================
        // QUEST SERVICE
        // ============================================================

        private IEnumerator ConnectWhenReady()
        {
            while (QuestManagerMB.Instance == null)
                yield return null;

            while (QuestManagerMB.Instance.Service == null)
                yield return null;

            service = QuestManagerMB.Instance.Service;

            service.OnQuestAdded += OnQuestAdded;
            service.OnQuestUpdated += OnQuestUpdated;
            service.OnQuestRemoved += OnQuestRemoved;

            foreach (var quest in service.ActiveQuests)
                RestoreExistingQuest(quest);

            foreach (var quest in service.CompletedQuests)
                RestoreExistingQuest(quest);

            RefreshFilter();
        }

        // ============================================================
        // FILTERING
        // ============================================================

        private void SetFilter(QuestFilter filter)
        {
            currentFilter = filter;
            RefreshFilter();
        }

        private void RefreshFilter()
        {
            if (service == null)
                return;

            foreach (var entry in journalEntries)
            {
                var quest =
                    service.ActiveQuests.FirstOrDefault(q => q.Definition.Id.Equals(entry.Key)) ??
                    service.CompletedQuests.FirstOrDefault(q => q.Definition.Id.Equals(entry.Key));

                if (quest == null)
                    continue;

                entry.Value.SetActive(FilterMatch(quest));
            }
        }

        private bool FilterMatch(QuestRuntime q)
        {
            return currentFilter switch
            {
                QuestFilter.All => true,
                QuestFilter.Active => q.State != QuestState.Completed,
                QuestFilter.Completed => q.State == QuestState.Completed,
                _ => true
            };
        }

        // ============================================================
        // RESTORE / EVENTS
        // ============================================================

        private void RestoreExistingQuest(QuestRuntime quest)
        {
            if (quest.State != QuestState.Completed && hudEntries.Count < maxHudQuests)
            {
                var go = Instantiate(hudEntryTemplate, hudContainer);
                hudEntries[quest.Definition.Id] = go;
                UpdateEntry(go, quest);
            }

            var entry = Instantiate(journalEntryTemplate, listParent);
            journalEntries[quest.Definition.Id] = entry;
            UpdateEntry(entry, quest);
        }

        private void OnQuestAdded(QuestRuntime quest)
        {
            if (hudEntries.Count < maxHudQuests)
            {
                var go = Instantiate(hudEntryTemplate, hudContainer);
                hudEntries[quest.Definition.Id] = go;
                UpdateEntry(go, quest);
            }

            var j = Instantiate(journalEntryTemplate, listParent);
            journalEntries[quest.Definition.Id] = j;
            UpdateEntry(j, quest);

            RefreshFilter();
            ShowNotification($"Добавлен квест: {quest.Definition.Name}");
        }

        private void OnQuestUpdated(QuestRuntime quest)
        {
            if (hudEntries.TryGetValue(quest.Definition.Id, out var hud))
                UpdateEntry(hud, quest);

            if (journalEntries.TryGetValue(quest.Definition.Id, out var j))
                UpdateEntry(j, quest);

            RefreshFilter();
        }

        private void OnQuestRemoved(QuestRuntime quest)
        {
            if (hudEntries.TryGetValue(quest.Definition.Id, out var hud))
                Destroy(hud);

            if (journalEntries.TryGetValue(quest.Definition.Id, out var j))
                Destroy(j);

            RefreshFilter();
        }

        // ============================================================
        // UI ENTRY
        // ============================================================

        private void UpdateEntry(GameObject entry, QuestRuntime quest)
        {
            var text = entry.GetComponentInChildren<TMP_Text>();
            var slider = entry.GetComponentInChildren<Slider>();

            text.text = $"{quest.Definition.Name} ({quest.CurrentProgress}/{quest.TargetProgress})";
            text.color = quest.State == QuestState.Completed ? Color.green : Color.white;

            if (slider != null)
            {
                slider.value = quest.TargetProgress > 0
                    ? (float)quest.CurrentProgress / quest.TargetProgress
                    : 0f;
            }
        }

        // ============================================================
        // JOURNAL OPEN
        // ============================================================

        public void OpenJournal()
        {
            Debug.Log("QuestUIRuntime.OpenJournal CALLED");
            journalScreen.Open();
        }

        // ============================================================
        // NOTIFICATIONS
        // ============================================================

        private void ShowNotification(string message)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);

            CancelInvoke(nameof(HideNotification));
            Invoke(nameof(HideNotification), notifyTime);
        }

        private void HideNotification()
        {
            notificationPanel.SetActive(false);
        }
    }
}
