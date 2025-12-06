using UnityEngine;
using Features.Quests.Domain;
using Features.Quests.Data;
using Features.Quests.UnityIntegration;

public class PlayerQuestController : MonoBehaviour
{
    public QuestService Service { get; private set; }
    public QuestChainService ChainService { get; private set; }

    [Header("Данные квестов")]
    [SerializeField] private QuestDatabaseAsset databaseAsset;

    [Header("Стартовые цепочки")]
    [SerializeField] private QuestChainAsset[] startChains;

    [Header("Одиночные стартовые квесты (не цепочки)")]
    [SerializeField] private QuestAsset[] startQuests;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        Debug.Log("PlayerQuestController.Init() called");

        // Создаём сервис квестов
        Service = new QuestService();
        ChainService = new QuestChainService(Service);

        // 1) ПРИВЯЗЫВАЕМ сервис к QuestManagerMB СРАЗУ
        QuestManagerMB.Instance.Bind(Service);

        // 2) ТЕПЕРЬ выдаём квесты
        foreach (var quest in startQuests)
        {
            if (quest != null)
            {
                Debug.Log("StartQuest SINGLE: " + quest.questId);
                Service.StartQuest(quest.ToDefinition());
            }
        }

        foreach (var chain in startChains)
        {
            if (chain != null)
            {
                Debug.Log("StartChain: " + chain.chainId);
                ChainService.StartChain(chain.ToDefinition());
            }
        }

        Debug.Log("INIT ACTIVE COUNT AFTER START = " + Service.ActiveQuests.Count);
        Debug.Log("✔ PlayerQuestController: стартовые квесты загружены");
    }
}
