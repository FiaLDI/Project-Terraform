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

        title.text = def != null ? def.Name : id;
        progress.text = BuildProgressText(state, def);

        if (state.state == QuestState.Completed)
            progress.text += " (Done)";
        else if (state.state == QuestState.Failed)
            progress.text += " (Failed)";

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

    private string BuildProgressText(QuestNetState state, QuestDefinition def)
    {
        if (state.conditions == null || state.conditions.Length == 0)
            return "No conditions";

        var lines = new System.Text.StringBuilder();

        for (int i = 0; i < state.conditions.Length; i++)
        {
            var c = state.conditions[i];

            string desc = $"Condition {i + 1}";
            if (def != null && i < def.Conditions.Count)
                desc = def.Conditions[i].GetDescription();

            lines.AppendLine($"{desc}: {c.progress}/{c.target}");
        }

        return lines.ToString();
    }
}
