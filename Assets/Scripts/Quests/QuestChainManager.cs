using UnityEngine;
using System.Collections.Generic;

namespace Quests
{
    public class QuestChainManager : MonoBehaviour
    {
        [Header("Цепочки квестов")]
        public List<QuestChain> questChains = new List<QuestChain>();

        [Header("Автозапуск")]
        public bool autoStartChainsOnStart = true;
        public int startChainIndex = 0;

        private Dictionary<Quest, QuestChain> questToChainMap = new Dictionary<Quest, QuestChain>();
        private QuestChain activeChain;

        void Start()
        {
            Debug.Log("?? QuestChainManager: Start() вызван");

            InitializeChains();

            if (autoStartChainsOnStart)
            {
                if (questChains.Count > 0)
                {
                    Debug.Log($"?? QuestChainManager: Автозапуск цепочки с индексом {startChainIndex}");
                    StartChain(startChainIndex);
                }
                else
                {
                    Debug.LogError("? QuestChainManager: Нет цепочек для автозапуска!");
                }
            }
        }

        void InitializeChains()
        {
            questToChainMap.Clear();
            Debug.Log($"?? QuestChainManager: Инициализация {questChains.Count} цепочек");

            for (int i = 0; i < questChains.Count; i++)
            {
                QuestChain chain = questChains[i];
                if (chain == null)
                {
                    Debug.LogError($"? QuestChainManager: Цепочка с индексом {i} равна null!");
                    continue;
                }

                Debug.Log($"?? QuestChainManager: Обрабатываем цепочку '{chain.chainName}' с {chain.questsInOrder.Count} квестами");

                for (int j = 0; j < chain.questsInOrder.Count; j++)
                {
                    Quest quest = chain.questsInOrder[j];
                    if (quest != null)
                    {
                        questToChainMap[quest] = chain;
                        Debug.Log($"?? QuestChainManager: Зарегистрирован квест '{quest.questName}' в цепочке '{chain.chainName}'");
                    }
                    else
                    {
                        Debug.LogError($"? QuestChainManager: Квест с индексом {j} в цепочке '{chain.chainName}' равен null!");
                    }
                }
            }

            Debug.Log($"?? QuestChainManager: Зарегистрировано {questToChainMap.Count} квестов в цепочках");
        }

        public void StartChain(int chainIndex)
        {
            Debug.Log($"?? QuestChainManager.StartChain: Запрос на запуск цепочки {chainIndex}");

            if (chainIndex >= 0 && chainIndex < questChains.Count)
            {
                activeChain = questChains[chainIndex];
                if (activeChain != null)
                {
                    Debug.Log($"?? QuestChainManager: Запускаем цепочку '{activeChain.chainName}'");
                    activeChain.StartChain();
                }
                else
                {
                    Debug.LogError($"? QuestChainManager: Цепочка с индексом {chainIndex} равна null!");
                }
            }
            else
            {
                Debug.LogError($"? QuestChainManager: Неверный индекс цепочки {chainIndex}. Всего цепочек: {questChains.Count}");
            }
        }

        public void OnQuestCompleted(Quest completedQuest)
        {
            if (completedQuest == null)
            {
                Debug.LogError("? QuestChainManager.OnQuestCompleted: completedQuest is null");
                return;
            }

            Debug.Log($"?? QuestChainManager.OnQuestCompleted: Обрабатываем завершение '{completedQuest.questName}'");

            if (questToChainMap.ContainsKey(completedQuest))
            {
                QuestChain chain = questToChainMap[completedQuest];
                if (chain != null)
                {
                    Debug.Log($"?? QuestChainManager: Квест '{completedQuest.questName}' принадлежит цепочке '{chain.chainName}'");
                    chain.MoveToNextQuest();
                }
                else
                {
                    Debug.LogError($"? QuestChainManager: Цепочка для квеста '{completedQuest.questName}' равна null!");
                }
            }
            else
            {
                Debug.LogWarning($"?? QuestChainManager: Квест '{completedQuest.questName}' не принадлежит ни одной цепочке");
                Debug.Log($"?? QuestChainManager: Зарегистрированные квесты: {string.Join(", ", questToChainMap.Keys)}");
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
            Debug.Log("?? QuestChainManager: Все цепочки сброшены");
        }
    }
}