using UnityEngine;
using System.Collections.Generic;

namespace Quests
{
    public class QuestChainManager : MonoBehaviour
    {
        [Header("Список цепочек")]
        public List<QuestChain> questChains = new List<QuestChain>();

        [Header("Настройки запуска")]
        public bool autoStartChainsOnStart = true;
        public int startChainIndex = 0;

        private Dictionary<QuestAsset, QuestChain> questToChainMap = new Dictionary<QuestAsset, QuestChain>();
        private QuestChain activeChain;

        void Start()
        {
            Debug.Log("▶ QuestChainManager: Start()");

            InitializeChains();

            if (autoStartChainsOnStart)
            {
                if (questChains.Count > 0)
                {
                    Debug.Log($"▶ QuestChainManager: автозапуск цепочки {startChainIndex}");
                    StartChain(startChainIndex);
                }
                else
                {
                    Debug.LogError("❌ QuestChainManager: нет цепочек для запуска!");
                }
            }
        }

        void InitializeChains()
        {
            questToChainMap.Clear();
            Debug.Log($"▶ QuestChainManager: инициализация {questChains.Count} цепочек");

            for (int i = 0; i < questChains.Count; i++)
            {
                QuestChain chain = questChains[i];
                if (chain == null)
                {
                    Debug.LogError($"❌ QuestChainManager: цепочка {i} пустая!");
                    continue;
                }

                Debug.Log($"▶ QuestChainManager: цепочка '{chain.chainName}' содержит {chain.questsInOrder.Count} квестов");

                for (int j = 0; j < chain.questsInOrder.Count; j++)
                {
                    QuestAsset quest = chain.questsInOrder[j];
                    if (quest != null)
                    {
                        questToChainMap[quest] = chain;
                        Debug.Log($"▶ Квест '{quest.questName}' привязан к цепочке '{chain.chainName}'");
                    }
                    else
                    {
                        Debug.LogError($"❌ QuestChainManager: квест {j} в цепочке '{chain.chainName}' пустой!");
                    }
                }
            }
        }

        public void StartChain(int chainIndex)
        {
            Debug.Log($"▶ QuestChainManager.StartChain: запуск цепочки {chainIndex}");

            if (chainIndex >= 0 && chainIndex < questChains.Count)
            {
                activeChain = questChains[chainIndex];
                if (activeChain != null)
                {
                    Debug.Log($"▶ QuestChainManager: запущена цепочка '{activeChain.chainName}'");
                    activeChain.StartChain();
                }
                else
                {
                    Debug.LogError($"❌ QuestChainManager: цепочка {chainIndex} пустая!");
                }
            }
            else
            {
                Debug.LogError($"❌ QuestChainManager: индекс {chainIndex} вне диапазона (всего {questChains.Count})");
            }
        }

        public void OnQuestCompleted(QuestAsset completedQuest)
        {
            if (completedQuest == null)
            {
                Debug.LogError("❌ QuestChainManager.OnQuestCompleted: completedQuest is null");
                return;
            }

            Debug.Log($"▶ QuestChainManager.OnQuestCompleted: завершён '{completedQuest.questName}'");

            if (questToChainMap.ContainsKey(completedQuest))
            {
                QuestChain chain = questToChainMap[completedQuest];
                if (chain != null)
                {
                    Debug.Log($"▶ Квест '{completedQuest.questName}' завершил этап в цепочке '{chain.chainName}'");
                    chain.MoveToNextQuest();
                }
                else
                {
                    Debug.LogError($"❌ QuestChainManager: цепочка для '{completedQuest.questName}' пустая!");
                }
            }
            else
            {
                Debug.LogWarning($"⚠ QuestChainManager: квест '{completedQuest.questName}' не принадлежит ни одной цепочке");
            }
        }

        [ContextMenu("Start First Chain")]
        void StartFirstChain()
        {
            StartChain(0);
        }

        [ContextMenu("Reset All Chains")]
        void ResetAllChains()
        {
            foreach (QuestChain chain in questChains)
            {
                if (chain != null) chain.ResetChain();
            }
            Debug.Log("▶ QuestChainManager: все цепочки сброшены");
        }
    }
}
