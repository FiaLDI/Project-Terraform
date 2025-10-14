using UnityEngine;
using Quests;

public class QuestAutoStarter : MonoBehaviour
{
    [Header("Тестовые квесты")]
    public Quest approachQuest;
    public Quest clearEnemiesQuest;
    public Quest interactionQuest;

    void Start()
    {
        
        Invoke("StartTestQuest", 2f);
    }

    void StartTestQuest()
    {
        if (approachQuest != null)
        {
            QuestManager.Instance.StartQuest(approachQuest);
            Debug.Log($"Запущен квест: {approachQuest.questName}");
        }
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha1) && clearEnemiesQuest != null)
        {
            QuestManager.Instance.StartQuest(clearEnemiesQuest);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && interactionQuest != null)
        {
            QuestManager.Instance.StartQuest(interactionQuest);
        }
    }
}
