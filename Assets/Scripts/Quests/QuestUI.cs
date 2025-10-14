using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Quests
{
    public class QuestUI : MonoBehaviour
    {
        [Header("Основные UI элементы")]
        public Transform questPanel;
        public GameObject questTemplate;
        public CanvasGroup mainCanvasGroup;

        [Header("Боковой список активных квестов")]
        public Transform activeQuestsPanel;
        public GameObject activeQuestTemplate;

        [Header("Панель всех квестов (Input Action)")]
        public GameObject allQuestsPanel;
        public Transform completedQuestsParent;
        public Transform activeQuestsParent;
        public GameObject allQuestTemplate;

        [Header("Уведомления")]
        public GameObject notificationPanel;
        public TMP_Text notificationText;
        public float notificationDuration = 3f;

        private Dictionary<Quest, GameObject> questEntries = new();
        private Dictionary<Quest, GameObject> activeQuestEntries = new();
        private Dictionary<Quest, GameObject> allQuestEntries = new();

        private bool isAllQuestsPanelOpen = false;
        private Coroutine notificationCoroutine;

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = 0f;
                mainCanvasGroup.interactable = false;
                mainCanvasGroup.blocksRaycasts = false;
            }

            if (allQuestsPanel != null)
                allQuestsPanel.SetActive(false);

            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

        public void ToggleAllQuestsPanel()
        {
            if (allQuestsPanel == null) return;

            isAllQuestsPanelOpen = !isAllQuestsPanelOpen;
            allQuestsPanel.SetActive(isAllQuestsPanelOpen);

            if (isAllQuestsPanelOpen)
            {
                RefreshAllQuestsPanel();
            }

            if (QuestManager.Instance != null)
            {
                Debug.Log($"Панель квестов: {(isAllQuestsPanelOpen ? "открыта" : "закрыта")}");
            }
        }

        private void RefreshAllQuestsPanel()
        {
            foreach (var entry in allQuestEntries.Values)
            {
                if (entry != null)
                    Destroy(entry);
            }
            allQuestEntries.Clear();

            if (QuestManager.Instance == null)
                return;

            foreach (var quest in QuestManager.Instance.completedQuests)
            {
                if (quest != null)
                    CreateAllQuestEntry(quest, completedQuestsParent);
            }

            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest != null)
                    CreateAllQuestEntry(quest, activeQuestsParent);
            }

            Debug.Log($"?? QuestUI: обновлена панель всех квестов. Активных: {QuestManager.Instance.activeQuests.Count}, завершённых: {QuestManager.Instance.completedQuests.Count}");
        }



        public void AddQuest(Quest quest)
        {
            if (quest == null || questEntries.ContainsKey(quest)) return;

            GameObject entry = CreateQuestEntry(quest, questPanel);
            questEntries.Add(quest, entry);

            GameObject activeEntry = CreateActiveQuestEntry(quest, activeQuestsPanel);
            activeQuestEntries.Add(quest, activeEntry);

            ShowQuestNotification($"Новый квест: {quest.questName}");

            if (isAllQuestsPanelOpen)
            {
                CreateAllQuestEntry(quest, activeQuestsParent);
            }
        }

        public void UpdateQuest(Quest quest)
        {
            if (quest == null) return;

            if (questEntries.ContainsKey(quest))
            {
                UpdateQuestEntry(questEntries[quest], quest);
            }

            if (activeQuestEntries.ContainsKey(quest))
            {
                UpdateQuestEntry(activeQuestEntries[quest], quest);
            }

            if (allQuestEntries.ContainsKey(quest))
            {
                UpdateQuestEntry(allQuestEntries[quest], quest);
            }

            if (quest.isCompleted)
            {
                ShowQuestNotification($"Задание выполнено: {quest.questName}");

                if (activeQuestEntries.ContainsKey(quest))
                {
                    Destroy(activeQuestEntries[quest]);
                    activeQuestEntries.Remove(quest);
                }

                if (isAllQuestsPanelOpen)
                {
                    RefreshAllQuestsPanel();
                }
            }
        }

        public void RemoveQuest(Quest quest)
        {
            if (quest == null) return;

            if (questEntries.ContainsKey(quest))
            {
                Destroy(questEntries[quest]);
                questEntries.Remove(quest);
            }

            if (activeQuestEntries.ContainsKey(quest))
            {
                Destroy(activeQuestEntries[quest]);
                activeQuestEntries.Remove(quest);
            }

            if (allQuestEntries.ContainsKey(quest))
            {
                Destroy(allQuestEntries[quest]);
                allQuestEntries.Remove(quest);
            }
        }

        private GameObject CreateQuestEntry(Quest quest, Transform parent)
        {
            if (questTemplate == null || parent == null) return null;

            GameObject entry = Instantiate(questTemplate, parent);
            entry.SetActive(true);
            UpdateQuestEntry(entry, quest);
            return entry;
        }

        private GameObject CreateActiveQuestEntry(Quest quest, Transform parent)
        {
            if (activeQuestTemplate == null || parent == null) return null;

            GameObject entry = Instantiate(activeQuestTemplate, parent);
            entry.SetActive(true);
            UpdateQuestEntry(entry, quest);
            return entry;
        }

        private GameObject CreateAllQuestEntry(Quest quest, Transform parent)
        {
            if (allQuestTemplate == null || parent == null) return null;

            GameObject entry = Instantiate(allQuestTemplate, parent);
            entry.SetActive(true);
            UpdateQuestEntry(entry, quest);
            allQuestEntries.Add(quest, entry);
            return entry;
        }

        private void UpdateQuestEntry(GameObject entry, Quest quest)
        {
            if (entry == null || quest == null) return;

            TMP_Text questText = entry.GetComponentInChildren<TMP_Text>();
            Slider questSlider = entry.GetComponentInChildren<Slider>();

            if (questText != null)
            {
                string status = quest.isCompleted ? "? " : "? ";
                questText.text = $"{status}{quest.questName} ({quest.currentProgress}/{quest.targetProgress})";

                questText.color = quest.isCompleted ? Color.green : Color.white;
            }

            if (questSlider != null)
            {
                questSlider.value = quest.targetProgress > 0 ?
                    (float)quest.currentProgress / quest.targetProgress : 0f;

                questSlider.gameObject.SetActive(!quest.isCompleted);
            }
        }

        public void ShowQuestNotification(string message)
        {
            if (notificationPanel == null || notificationText == null) return;

            notificationText.text = message;
            notificationPanel.SetActive(true);

            if (notificationCoroutine != null)
                StopCoroutine(notificationCoroutine);

            notificationCoroutine = StartCoroutine(HideNotificationAfterDelay());
        }

        private IEnumerator HideNotificationAfterDelay()
        {
            yield return new WaitForSeconds(notificationDuration);

            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

        public void CloseAllQuestsPanel()
        {
            if (allQuestsPanel != null)
            {
                allQuestsPanel.SetActive(false);
                isAllQuestsPanelOpen = false;
            }
        }
    }
}