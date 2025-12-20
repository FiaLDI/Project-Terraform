using UnityEngine;
using Features.Quests.Domain;
using Features.Quests.Data;

namespace Features.Quests.UnityIntegration
{
    public class QuestChainManagerMB : MonoBehaviour
    {
        [SerializeField] private QuestManagerMB questManager;
        [SerializeField] private QuestChainAsset[] chains;

        private QuestChainService chainService;

        private void Awake()
        {
            if (questManager == null)
                questManager = Object.FindAnyObjectByType<QuestManagerMB>();
        }

        private void Start()
        {
            chainService = new QuestChainService(questManager.Service);

            questManager.Service.OnQuestUpdated += OnQuestUpdated;
        }

        private void OnQuestUpdated(QuestRuntime quest)
        {
            if (quest.State == QuestState.Completed)
            {
                chainService.Advance(quest.Definition.Id);
            }
        }

        [ContextMenu("Start All Chains")]
        public void StartAllChains()
        {
            foreach (var chainAsset in chains)
            {
                var def = chainAsset.ToDefinition();
                chainService.StartChain(def);
            }
        }
    }
}
