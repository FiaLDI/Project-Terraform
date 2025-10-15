using UnityEngine;
using Quests;

public class QuestTestStarter : MonoBehaviour
{
    [Tooltip("Какой квест запустить при старте игры")]
    public Quest questToStart;

    void Start()
    {
        if (questToStart != null)
        {
            Debug.Log($"QuestTestStarter запускает квест: {questToStart.questName}");
            QuestManager.Instance.StartQuest(questToStart);
        }
        else
        {
            Debug.LogWarning("QuestTestStarter: нет квеста для запуска!");
        }
    }
}

