using Features.Quests.Data;
using Features.Quests.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDebugItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text progress;

    [SerializeField] private Button advanceBtn;
    [SerializeField] private Button completeBtn;
    [SerializeField] private Button failBtn;

    [SerializeField] private QuestDatabaseAsset questDatabase;

    private string questId;
    private PlayerQuestNetwork net;

    public void Bind(string id, QuestNetState state, PlayerQuestNetwork controller)
    {
        questId = id;
        net = controller;

        var def = questDatabase.GetDefinition(id);

        // =========================
        // TITLE
        // =========================
        title.text = def != null ? def.Name : id;

        // =========================
        // PROGRESS (MULTI-CONDITION)
        // =========================
        progress.text = BuildProgressText(state, def);

        if (state.completed)
            progress.text += " ✅";

        // =========================
        // BUTTONS
        // =========================
        advanceBtn.onClick.RemoveAllListeners();
        completeBtn.onClick.RemoveAllListeners();
        failBtn.onClick.RemoveAllListeners();

        advanceBtn.onClick.AddListener(() =>
            net.DebugAdvanceQuestServerRpc(questId));

        completeBtn.onClick.AddListener(() =>
            net.DebugCompleteQuestServerRpc(questId));

        failBtn.onClick.AddListener(() =>
            net.DebugFailQuestServerRpc(questId));
    }

    // =========================================================
    // BUILD TEXT
    // =========================================================

    private string BuildProgressText(QuestNetState state, QuestDefinition def)
    {
        if (state.conditions == null || state.conditions.Length == 0)
            return "No conditions";

        var lines = new System.Text.StringBuilder();

        for (int i = 0; i < state.conditions.Length; i++)
        {
            var c = state.conditions[i];

            string desc = $"Condition {i + 1}";

            // если есть дефиниция — берем описание
            if (def != null && i < def.Conditions.Count)
            {
                desc = def.Conditions[i].GetDescription();
            }

            lines.AppendLine($"{desc}: {c.progress}/{c.target}");
        }

        return lines.ToString();
    }
}
