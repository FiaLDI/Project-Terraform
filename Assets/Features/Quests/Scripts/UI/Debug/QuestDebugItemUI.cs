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

    private string questId;
    private PlayerNetworkController net;

    public void Bind(string id, QuestNetState state, PlayerNetworkController controller)
    {
        questId = id;
        net = controller;

        title.text = id;

        progress.text = $"{state.progress} / {state.target}" +
                        (state.completed ? " ✅" : "");

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
}