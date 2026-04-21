using UnityEngine;
using System.Collections.Generic;
using Features.Quests.Data;
using FishNet.Object.Synchronizing;
using Features.Quests.UnityIntegration;
using Features.Quests.Domain;
using Features.UI;

public class QuestUIRuntime : PlayerBoundUIView
{
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
        if (asServer)
            return;

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
        }
    }

    private void AddQuestUI(QuestNetState state)
    {
        var def = questDatabase.GetDefinition(state.questId);
        if (def == null)
            return;

        CreateEntryIfMissing(state, def, hudEntries, hudEntryTemplate, hudContainer);
        CreateEntryIfMissing(state, def, journalEntries, journalEntryTemplate, listParent);
    }

    private void UpdateQuestUI(QuestNetState state)
    {
        var def = questDatabase.GetDefinition(state.questId);
        if (def == null)
            return;

        UpdateEntryIfExists(state, def, hudEntries);
        UpdateEntryIfExists(state, def, journalEntries);
    }

    private void RemoveQuestUI(string id)
    {
        RemoveEntry(id, hudEntries);
        RemoveEntry(id, journalEntries);
    }

    private void CreateEntryIfMissing(
        QuestNetState state,
        QuestDefinition def,
        Dictionary<string, QuestItemUI> entries,
        QuestItemUI template,
        Transform parent)
    {
        if (entries.ContainsKey(state.questId))
            return;

        var entry = Instantiate(template, parent);
        ApplyEntryVisual(entry, state, def);
        entries[state.questId] = entry;
    }

    private void UpdateEntryIfExists(
        QuestNetState state,
        QuestDefinition def,
        Dictionary<string, QuestItemUI> entries)
    {
        if (!entries.TryGetValue(state.questId, out var entry))
            return;

        ApplyEntryVisual(entry, state, def);
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

    private void ApplyEntryVisual(QuestItemUI entry, QuestNetState state, QuestDefinition def)
    {
        entry.Title.text = BuildTitleText(state, def);
        entry.Conditions.text = BuildConditionsText(state, def);

        var color = state.completed ? Color.green : Color.white;
        entry.Title.color = color;
        entry.Conditions.color = color;
    }

    private string BuildTitleText(QuestNetState state, QuestDefinition def)
    {
        return state.completed
            ? $"{def.Name} ✓"
            : def.Name;
    }

    private string BuildConditionsText(QuestNetState state, QuestDefinition def)
    {
        if (state.completed)
            return string.Empty;

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
