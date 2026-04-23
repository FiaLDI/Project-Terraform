using System.Collections.Generic;
using Features.Quests.Data;
using Features.Quests.Domain;
using Features.Quests.UnityIntegration;
using Features.UI;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class QuestUIRuntime : PlayerBoundUIView
{
    private static readonly Color CompletedColor = Color.green;
    private static readonly Color FailedColor = Color.red;
    private static readonly Color DefaultColor = Color.white;

    [Header("HUD")]
    [SerializeField] private Transform hudContainer;
    [SerializeField] private QuestItemUI hudEntryTemplate;

    [Header("Journal")]
    [SerializeField] private Transform listParent;
    [SerializeField] private QuestItemUI journalEntryTemplate;
    [SerializeField] private QuestJournalScreen journalScreen;

    [Header("Database")]
    [SerializeField] private QuestDatabaseAsset questDatabase;

    private PlayerQuestComponent questComponent;

    private readonly Dictionary<string, QuestItemUI> hudEntries = new();
    private readonly Dictionary<string, QuestItemUI> journalEntries = new();

    protected override void OnPlayerBound(GameObject player)
    {
        questComponent = player != null ? player.GetComponent<PlayerQuestComponent>() : null;

        if (questComponent == null)
        {
            Debug.LogError("[QuestUIRuntime] PlayerQuestComponent missing on player");
            return;
        }

        questComponent.Quests.OnChange += OnQuestChanged;
        RestoreExisting();
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        if (questComponent != null)
            questComponent.Quests.OnChange -= OnQuestChanged;

        questComponent = null;
        ClearAllEntries();
    }

    private void RestoreExisting()
    {
        if (questComponent == null)
            return;

        ClearAllEntries();

        foreach (var quest in questComponent.Quests.Values)
            AddQuestUI(quest);
    }

    private void OnQuestChanged(
        SyncDictionaryOperation op,
        string key,
        QuestNetState value,
        bool asServer)
    {
        switch (op)
        {
            case SyncDictionaryOperation.Add:
                AddQuestUI(value);
                break;

            case SyncDictionaryOperation.Set:
                UpdateQuestUI(value);
                break;

            case SyncDictionaryOperation.Remove:
                RemoveQuestUI(key);
                break;

            case SyncDictionaryOperation.Clear:
                ClearAllEntries();
                break;
        }
    }

    private void AddQuestUI(QuestNetState state)
    {
        var def = questDatabase.GetDefinition(state.questId);
        if (def == null)
            return;

        CreateHudEntryIfMissing(state, def);
        CreateJournalEntryIfMissing(state, def);
    }

    private void UpdateQuestUI(QuestNetState state)
    {
        var def = questDatabase.GetDefinition(state.questId);
        if (def == null)
            return;

        UpdateHudEntryIfExists(state, def);
        UpdateJournalEntryIfExists(state, def);
    }

    private void RemoveQuestUI(string id)
    {
        RemoveEntry(id, hudEntries);
        RemoveEntry(id, journalEntries);
        journalScreen?.RemoveEntry(id);
    }

    private void CreateHudEntryIfMissing(QuestNetState state, QuestDefinition def)
    {
        if (hudEntries.ContainsKey(state.questId))
            return;

        if (hudEntryTemplate == null || hudContainer == null)
            return;

        var entry = Instantiate(hudEntryTemplate, hudContainer);
        ApplyHudEntryVisual(entry, state, def);
        hudEntries[state.questId] = entry;
    }

    private void CreateJournalEntryIfMissing(QuestNetState state, QuestDefinition def)
    {
        if (journalEntries.ContainsKey(state.questId))
            return;

        if (journalEntryTemplate == null || listParent == null || journalScreen == null)
            return;

        var entry = Instantiate(journalEntryTemplate, listParent);
        journalEntries[state.questId] = entry;
        journalScreen.RegisterOrUpdateEntry(entry, state, def);
    }

    private void UpdateHudEntryIfExists(QuestNetState state, QuestDefinition def)
    {
        if (!hudEntries.TryGetValue(state.questId, out var entry))
            return;

        ApplyHudEntryVisual(entry, state, def);
    }

    private void UpdateJournalEntryIfExists(QuestNetState state, QuestDefinition def)
    {
        if (!journalEntries.TryGetValue(state.questId, out var entry) || journalScreen == null)
            return;

        journalScreen.RegisterOrUpdateEntry(entry, state, def);
    }

    private void RemoveEntry(string id, Dictionary<string, QuestItemUI> entries)
    {
        if (!entries.TryGetValue(id, out var entry))
            return;

        Destroy(entry.gameObject);
        entries.Remove(id);
    }

    private void ClearAllEntries()
    {
        ClearEntries(hudEntries);
        ClearEntries(journalEntries);
        journalScreen?.ClearEntries();
    }

    private void ClearEntries(Dictionary<string, QuestItemUI> entries)
    {
        foreach (var entry in entries.Values)
        {
            if (entry != null)
                Destroy(entry.gameObject);
        }

        entries.Clear();
    }

    private void ApplyHudEntryVisual(QuestItemUI entry, QuestNetState state, QuestDefinition def)
    {
        entry.Title.text = BuildTitleText(state, def);
        entry.Conditions.text = BuildConditionsText(state, def);

        var color = state.state switch
        {
            QuestState.Completed => CompletedColor,
            QuestState.Failed => FailedColor,
            _ => DefaultColor
        };

        entry.Title.color = color;
        entry.Conditions.color = color;
    }

    private string BuildTitleText(QuestNetState state, QuestDefinition def)
    {
        return state.state switch
        {
            QuestState.Completed => $"{def.Name} (Done)",
            QuestState.Failed => $"{def.Name} (Failed)",
            _ => def.Name
        };
    }

    private string BuildConditionsText(QuestNetState state, QuestDefinition def)
    {
        if (state.state == QuestState.Completed)
            return string.Empty;

        if (state.state == QuestState.Failed)
            return "Quest failed";

        if (state.conditions == null || def.Conditions == null)
            return string.Empty;

        int count = Mathf.Min(state.conditions.Length, def.Conditions.Count);
        var lines = new List<string>(count);

        for (int i = 0; i < count; i++)
        {
            var cond = def.Conditions[i];
            var net = state.conditions[i];
            lines.Add($"{cond.GetDescription()}: {net.progress}/{net.target}");
        }

        return string.Join("\n", lines);
    }

    public void OpenJournal()
    {
        Debug.Log("QuestUIRuntime.OpenJournal CALLED");

        if (journalScreen != null)
            journalScreen.Open();
    }
}
