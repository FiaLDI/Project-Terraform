using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Quests
{
    public class QuestUI : MonoBehaviour
    {
        [Header("HUD (всегда видимый лог)")]
        public Transform questPanel;
        public GameObject questTemplate;
        public int maxHudQuests = 5; // ⚡ максимум квестов на HUD

        [Header("Журнал активных квестов (AllQuestsPanel)")]
        public GameObject allQuestsPanel;
        public Transform activeQuestsParent;
        public Transform completedQuestsParent;
        public GameObject allQuestTemplate;

        [Header("Уведомления")]
        public GameObject notificationPanel;
        public TMP_Text notificationText;
        public float notificationDuration = 3f;

        [Header("Sliding HUD Quest Panel")]
        public RectTransform slidingHudPanel;
        public CanvasGroup slidingHudCanvasGroup;
        public float slideHudDuration = 0.25f;

        private bool isHudSliding = false;
        private Vector2 hudHiddenPos;
        private Vector2 hudShownPos;
        public bool hudIsOpen = true;

        // HUD квесты
        private Dictionary<QuestAsset, GameObject> hudQuestEntries = new();

        // Журнал (AllQuestsPanel)
        private Dictionary<QuestAsset, GameObject> allQuestEntries = new();

        private bool isAllQuestsPanelOpen = false;
        private Coroutine notificationCoroutine;

        private void Awake()
        {
            if (slidingHudPanel != null)
            {
                hudShownPos = slidingHudPanel.anchoredPosition;
                hudHiddenPos = new Vector2(hudShownPos.x - slidingHudPanel.rect.width, hudShownPos.y);
            }
        }

        private void Start()
        {
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
                RefreshAllQuestsPanel();
        }

        private void RefreshAllQuestsPanel()
        {
            // Очистка UI перед перезаполнением
            foreach (Transform child in activeQuestsParent)
                Destroy(child.gameObject);
            foreach (Transform child in completedQuestsParent)
                Destroy(child.gameObject);

            allQuestEntries.Clear();

            if (QuestManager.Instance == null) return;

            foreach (var quest in QuestManager.Instance.completedQuests)
                if (quest != null)
                    CreateAllQuestEntry(quest, completedQuestsParent);

            foreach (var quest in QuestManager.Instance.activeQuests)
                if (quest != null)
                    CreateAllQuestEntry(quest, activeQuestsParent);
        }

        public void AddQuest(QuestAsset quest)
        {
            if (quest == null) return;

            Debug.Log($"🟪 UI: AddQuest → {quest.questName}");

            // --- HUD (только первые maxHudQuests) ---
            if (hudQuestEntries.Count < maxHudQuests && !hudQuestEntries.ContainsKey(quest))
            {
                GameObject entry = Instantiate(questTemplate, questPanel);
                entry.SetActive(true);
                UpdateQuestEntry(entry, quest);
                hudQuestEntries.Add(quest, entry);
            }

            // --- Журнал (всегда добавляется) ---
            if (!allQuestEntries.ContainsKey(quest))
            {
                GameObject entry = CreateAllQuestEntry(quest, activeQuestsParent);
                allQuestEntries.Add(quest, entry);
            }

            quest.OnQuestUpdated += UpdateQuest;

            ShowQuestNotification($"Добавлен квест: {quest.questName}");
            UpdateQuest(quest);
        }

        public void UpdateQuest(QuestAsset quest)
        {
            if (quest == null) return;
            Debug.Log($"🟪 UI: UpdateQuest → {quest.questName}: {quest.currentProgress}/{quest.targetProgress}");


            // HUD
            if (hudQuestEntries.ContainsKey(quest))
                UpdateQuestEntry(hudQuestEntries[quest], quest);

            // Журнал
            if (allQuestEntries.ContainsKey(quest))
                UpdateQuestEntry(allQuestEntries[quest], quest);

            // Завершение
            if (quest.IsCompleted)
            {
                ShowQuestNotification($"Завершён квест: {quest.questName}");

                if (hudQuestEntries.ContainsKey(quest))
                {
                    Destroy(hudQuestEntries[quest]);
                    hudQuestEntries.Remove(quest);
                }

                if (isAllQuestsPanelOpen)
                    RefreshAllQuestsPanel();
            }
        }

        public void RemoveQuest(QuestAsset quest)
        {
            if (quest == null) return;

            Debug.Log($"🟪 UI: RemoveQuest → {quest.questName}");

            quest.OnQuestUpdated -= UpdateQuest;

            if (hudQuestEntries.ContainsKey(quest))
            {
                Destroy(hudQuestEntries[quest]);
                hudQuestEntries.Remove(quest);
            }

            if (allQuestEntries.ContainsKey(quest))
            {
                Destroy(allQuestEntries[quest]);
                allQuestEntries.Remove(quest);
            }
        }

        private GameObject CreateAllQuestEntry(QuestAsset quest, Transform parent)
        {
            if (allQuestTemplate == null || parent == null) return null;
            GameObject entry = Instantiate(allQuestTemplate, parent);
            entry.SetActive(true);
            UpdateQuestEntry(entry, quest);
            return entry;
        }

        private void UpdateQuestEntry(GameObject entry, QuestAsset quest)
        {
            if (entry == null || quest == null) return;

            TMP_Text questText = entry.GetComponentInChildren<TMP_Text>();
            Slider questSlider = entry.GetComponentInChildren<Slider>();

            int current = quest.currentProgress;
            int target = quest.targetProgress;
            bool completed = quest.IsCompleted;

            if (questText != null)
            {
                string status = completed ? "✔ " : "";
                questText.text = $"{status}{quest.questName} ({current}/{target})";
                questText.color = completed ? Color.green : Color.white;
            }

            if (questSlider != null)
            {
                questSlider.value = target > 0 ? (float)current / target : 0f;
                questSlider.gameObject.SetActive(!completed);
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

        public void ToggleHudQuestPanel()
        {
            if (slidingHudPanel == null || isHudSliding) return;

            StartCoroutine(SlideHudPanel(!hudIsOpen));
            hudIsOpen = !hudIsOpen;
        }

        private IEnumerator SlideHudPanel(bool opening)
        {
            isHudSliding = true;

            Vector2 startPos = slidingHudPanel.anchoredPosition;
            Vector2 targetPos = opening ? hudShownPos : hudHiddenPos;

            float startAlpha = slidingHudCanvasGroup.alpha;
            float targetAlpha = opening ? 1f : 0f;

            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / slideHudDuration;

                slidingHudPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                slidingHudCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                yield return null;
            }

            isHudSliding = false;
        }
    }
}
