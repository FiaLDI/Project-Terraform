using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Features.Quests.Data;
using FishNet.Object.Synchronizing;
using Features.Player.UI;
using Features.Quests.UnityIntegration;
using Features.Quests.Domain;

public class QuestUIRuntime : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Transform hudContainer;
    [SerializeField] private GameObject hudEntryTemplate;

    [Header("Journal")]
    [SerializeField] private Transform listParent;
    [SerializeField] private GameObject journalEntryTemplate;
    [SerializeField] private QuestJournalScreen journalScreen;

    [Header("Database")]
    [SerializeField] private QuestDatabaseAsset questDatabase;

    private PlayerQuestComponent questComponent;

    private readonly Dictionary<string, GameObject> hudEntries = new();
    private readonly Dictionary<string, GameObject> journalEntries = new();

    // =========================================================
    // LIFECYCLE
    // =========================================================

    private void OnEnable()
    {
        if (PlayerUIRoot.I != null)
            PlayerUIRoot.I.OnPlayerBound += OnPlayerBound;
    }

    private void OnDisable()
    {
        if (PlayerUIRoot.I != null)
            PlayerUIRoot.I.OnPlayerBound -= OnPlayerBound;

        if (questComponent != null)
            questComponent.Quests.OnChange -= OnQuestChanged;
    }

    // =========================================================
    // PLAYER BIND
    // =========================================================

    private void OnPlayerBound(GameObject player)
    {
        if (player == null)
            return;

        Debug.Log("[QuestUIRuntime] Player bound");

        if (questComponent != null)
            questComponent.Quests.OnChange -= OnQuestChanged;

        questComponent = player.GetComponent<PlayerQuestComponent>();

        if (questComponent == null)
        {
            Debug.LogError("[QuestUIRuntime] PlayerQuestComponent missing on player");
            return;
        }

        questComponent.Quests.OnChange += OnQuestChanged;

        RestoreExisting();
    }

    // =========================================================
    // RESTORE EXISTING QUESTS
    // =========================================================

    private void RestoreExisting()
    {
        foreach (var q in questComponent.Quests.Values)
            AddQuestUI(q);
    }

    // =========================================================
    // NETWORK EVENTS
    // =========================================================

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

    private string BuildConditionsText(QuestNetState state, QuestDefinition def)
    {
        var lines = new List<string>();

        for (int i = 0; i < state.conditions.Length; i++)
        {
            var cond = def.Conditions[i];
            var net = state.conditions[i];

            string line = $"{cond.GetDescription()}: {net.progress}/{net.target}";
            lines.Add(line);
        }

        return string.Join("\n", lines);
    }

    // =========================================================
    // UI CREATION
    // =========================================================

    private void AddQuestUI(QuestNetState state)
    {
        var def = questDatabase.GetDefinition(state.questId);
        if (def == null)
            return;

        string textValue;

        if (state.completed)
            textValue = $"{def.Name} ✓";
        else
            textValue = $"{def.Name}\n{BuildConditionsText(state, def)}";

        if (!hudEntries.ContainsKey(state.questId))
        {
            var go = Instantiate(hudEntryTemplate, hudContainer);
            var text = go.GetComponentInChildren<TMP_Text>();
            text.text = textValue;

            hudEntries[state.questId] = go;
        }

        if (!journalEntries.ContainsKey(state.questId))
        {
            var go = Instantiate(journalEntryTemplate, listParent);
            var text = go.GetComponentInChildren<TMP_Text>();
            text.text = textValue;

            journalEntries[state.questId] = go;
        }
    }

    private void UpdateQuestUI(QuestNetState state)
    {
        var def = questDatabase.GetDefinition(state.questId);
        if (def == null)
            return;

        string textValue;

        if (state.completed)
            textValue = $"{def.Name} ✓";
        else
            textValue = $"{def.Name}\n{BuildConditionsText(state, def)}";

        if (hudEntries.TryGetValue(state.questId, out var hud))
        {
            var text = hud.GetComponentInChildren<TMP_Text>();
            text.text = textValue;

            if (state.completed)
                text.color = Color.green;
        }

        if (journalEntries.TryGetValue(state.questId, out var journal))
        {
            var text = journal.GetComponentInChildren<TMP_Text>();
            text.text = textValue;

            if (state.completed)
                text.color = Color.green;
        }
    }

    private void RemoveQuestUI(string id)
    {
        if (hudEntries.TryGetValue(id, out var hud))
        {
            Destroy(hud);
            hudEntries.Remove(id);
        }

        if (journalEntries.TryGetValue(id, out var journal))
        {
            Destroy(journal);
            journalEntries.Remove(id);
        }
    }

    // =========================================================
    // JOURNAL
    // =========================================================

    public void OpenJournal()
    {
        Debug.Log("QuestUIRuntime.OpenJournal CALLED");

        if (journalScreen != null)
            journalScreen.Open();
    }
}
