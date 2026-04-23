using System.Collections.Generic;
using System.Text;
using Features.Input;
using Features.Quests.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Quests.UnityIntegration
{
    public class QuestJournalScreen : MonoBehaviour, IUIScreen
    {
        private enum QuestJournalFilter
        {
            All,
            Active,
            Completed
        }

        private sealed class JournalQuestEntry
        {
            public QuestItemUI view;
            public QuestNetState state;
            public QuestDefinition definition;
        }

        private static readonly Color DefaultTextColor = Color.white;
        private static readonly Color ActiveTextColor = Color.white;
        private static readonly Color CompletedTextColor = Color.green;
        private static readonly Color FailedTextColor = new Color(1f, 0.45f, 0.45f, 1f);

        [Header("Root")]
        [SerializeField] private GameObject journalPanel;

        [Header("Filter")]
        [SerializeField] private Button btnAll;
        [SerializeField] private Button btnActive;
        [SerializeField] private Button btnCompleted;

        [Header("Layout")]
        [SerializeField] private Transform listParent;
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private TMP_Text selectedQuestTitle;
        [SerializeField] private TMP_Text selectedQuestDescription;

        public InputMode Mode => InputMode.Inventory;

        private readonly Dictionary<string, JournalQuestEntry> entries = new();
        private QuestJournalFilter currentFilter = QuestJournalFilter.All;
        private string selectedQuestId;
        private bool initialized;

        private void Awake()
        {
            Initialize();

            if (journalPanel != null)
                journalPanel.SetActive(false);
        }

        public void RegisterOrUpdateEntry(QuestItemUI entryView, QuestNetState state, QuestDefinition definition)
        {
            if (!initialized || entryView == null || definition == null)
                return;

            if (!entries.TryGetValue(state.questId, out var entry))
            {
                entry = new JournalQuestEntry
                {
                    view = entryView
                };
                entries[state.questId] = entry;
            }

            entry.view = entryView;
            entry.state = state;
            entry.definition = definition;
            entry.view.SetClickHandler(() => SelectQuest(state.questId));

            ApplyListEntryVisual(entry);
            ApplyFilterToEntry(entry);

            if (string.IsNullOrWhiteSpace(selectedQuestId) || selectedQuestId == state.questId)
            {
                SelectQuest(state.questId);
                return;
            }

            if (!entries.ContainsKey(selectedQuestId))
            {
                SelectFirstVisibleQuest();
                return;
            }

            RefreshSelectionVisuals();
            RefreshDetails();
        }

        public void RemoveEntry(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
                return;

            entries.Remove(questId);

            if (selectedQuestId == questId)
                selectedQuestId = null;

            SelectFirstVisibleQuest();
            RefreshSelectionVisuals();
            RefreshDetails();
        }

        public void ClearEntries()
        {
            entries.Clear();
            selectedQuestId = null;
            RefreshFilterButtons();
            RefreshDetails();
        }

        public void Show()
        {
            Initialize();

            if (journalPanel != null)
                journalPanel.SetActive(true);

            ApplyCurrentFilter();
            RefreshSelectionVisuals();
            RefreshDetails();
        }

        public void Hide()
        {
            if (journalPanel != null)
                journalPanel.SetActive(false);
        }

        public void Open()
        {
            UIStackManager.I.Push(this);
        }

        private void Initialize()
        {
            if (initialized)
                return;

            if (!HasRequiredReferences())
            {
                Debug.LogWarning("[QuestJournalScreen] Required references are missing. Journal runtime setup is skipped.", this);
                return;
            }

            initialized = true;
            WireButtons();
            RefreshFilterButtons();
            RefreshDetails();
        }

        private void WireButtons()
        {
            BindFilterButton(btnAll, QuestJournalFilter.All);
            BindFilterButton(btnActive, QuestJournalFilter.Active);
            BindFilterButton(btnCompleted, QuestJournalFilter.Completed);
        }

        private void BindFilterButton(Button button, QuestJournalFilter filter)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SetFilter(filter));
        }

        private void SetFilter(QuestJournalFilter filter)
        {
            currentFilter = filter;
            ApplyCurrentFilter();
        }

        private void ApplyCurrentFilter()
        {
            foreach (var entry in entries.Values)
                ApplyFilterToEntry(entry);

            if (!IsSelectionVisible())
                SelectFirstVisibleQuest();

            RefreshFilterButtons();
            RefreshSelectionVisuals();
            RefreshDetails();
        }

        private void ApplyFilterToEntry(JournalQuestEntry entry)
        {
            if (entry?.view == null)
                return;

            bool visible = MatchesFilter(entry.state);
            entry.view.gameObject.SetActive(visible);
        }

        private bool MatchesFilter(QuestNetState state)
        {
            return currentFilter switch
            {
                QuestJournalFilter.Active => state.state == QuestState.Active,
                QuestJournalFilter.Completed => state.state == QuestState.Completed,
                _ => true
            };
        }

        private void SelectQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
                return;

            if (!entries.ContainsKey(questId))
                return;

            selectedQuestId = questId;
            RefreshSelectionVisuals();
            RefreshDetails();
        }

        private void SelectFirstVisibleQuest()
        {
            foreach (var pair in entries)
            {
                if (pair.Value?.view != null && pair.Value.view.gameObject.activeSelf)
                {
                    selectedQuestId = pair.Key;
                    return;
                }
            }

            selectedQuestId = null;
        }

        private bool IsSelectionVisible()
        {
            if (string.IsNullOrWhiteSpace(selectedQuestId))
                return false;

            if (!entries.TryGetValue(selectedQuestId, out var entry))
                return false;

            return entry.view != null && entry.view.gameObject.activeSelf;
        }

        private void RefreshSelectionVisuals()
        {
            // Selection visuals are driven by the assigned prefab styling.
        }

        private void RefreshFilterButtons()
        {
            if (btnAll != null)
                btnAll.interactable = currentFilter != QuestJournalFilter.All;

            if (btnActive != null)
                btnActive.interactable = currentFilter != QuestJournalFilter.Active;

            if (btnCompleted != null)
                btnCompleted.interactable = currentFilter != QuestJournalFilter.Completed;
        }

        private void RefreshDetails()
        {
            if (selectedQuestTitle == null || selectedQuestDescription == null)
                return;

            if (!entries.TryGetValue(selectedQuestId ?? string.Empty, out var entry) || entry.definition == null)
            {
                selectedQuestTitle.text = "No quest selected";
                selectedQuestDescription.text = BuildEmptyDetailsText();

                if (detailsPanel != null)
                    detailsPanel.SetActive(true);

                return;
            }

            selectedQuestTitle.text = entry.definition.Name;
            selectedQuestDescription.text = BuildDetailsText(entry.state, entry.definition);

            if (detailsPanel != null)
                detailsPanel.SetActive(true);
        }

        private string BuildEmptyDetailsText()
        {
            return currentFilter switch
            {
                QuestJournalFilter.Active => "No active quests match this filter.",
                QuestJournalFilter.Completed => "No completed quests yet.",
                _ => "Select a quest on the left to inspect its details."
            };
        }

        private string BuildDetailsText(QuestNetState state, QuestDefinition definition)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Status: {GetStateLabel(state.state)}");
            builder.AppendLine($"Type: {(definition.Scope == QuestScope.Shared ? "Shared" : "Personal")}");
            builder.AppendLine($"XP Reward: {definition.ExperienceReward}");
            builder.AppendLine();

            builder.AppendLine(string.IsNullOrWhiteSpace(definition.Description)
                ? "No description."
                : definition.Description.Trim());

            if (definition.Conditions != null && definition.Conditions.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Objectives:");

                int count = state.conditions != null
                    ? Mathf.Min(state.conditions.Length, definition.Conditions.Count)
                    : definition.Conditions.Count;

                for (int i = 0; i < count; i++)
                {
                    var condition = definition.Conditions[i];
                    if (state.conditions != null && i < state.conditions.Length)
                    {
                        var conditionState = state.conditions[i];
                        builder.AppendLine($"- {condition.GetDescription()} [{conditionState.progress}/{conditionState.target}]");
                    }
                    else
                    {
                        builder.AppendLine($"- {condition.GetDescription()}");
                    }
                }
            }

            if (definition.Rewards != null && definition.Rewards.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Rewards:");

                foreach (var reward in definition.Rewards)
                    builder.AppendLine($"- Item {reward.ItemId} x{reward.Amount}");
            }

            return builder.ToString().TrimEnd();
        }

        private string GetStateLabel(QuestState state)
        {
            return state switch
            {
                QuestState.Completed => "Completed",
                QuestState.Failed => "Failed",
                QuestState.Inactive => "Inactive",
                _ => "Active"
            };
        }

        private void ApplyListEntryVisual(JournalQuestEntry entry)
        {
            if (entry?.view == null)
                return;

            entry.view.Title.text = BuildListTitle(entry.definition, entry.state);

            if (entry.view.Conditions != null)
                entry.view.Conditions.text = string.Empty;

            Color textColor = entry.state.state switch
            {
                QuestState.Completed => CompletedTextColor,
                QuestState.Failed => FailedTextColor,
                QuestState.Active => ActiveTextColor,
                _ => DefaultTextColor
            };

            entry.view.Title.color = textColor;
        }

        private string BuildListTitle(QuestDefinition definition, QuestNetState state)
        {
            return state.state switch
            {
                QuestState.Completed => $"{definition.Name} (Done)",
                QuestState.Failed => $"{definition.Name} (Failed)",
                _ => definition.Name
            };
        }

        private bool HasRequiredReferences()
        {
            return journalPanel != null &&
                btnAll != null &&
                btnActive != null &&
                btnCompleted != null &&
                listParent != null &&
                detailsPanel != null &&
                selectedQuestTitle != null &&
                selectedQuestDescription != null;
        }
    }
}
