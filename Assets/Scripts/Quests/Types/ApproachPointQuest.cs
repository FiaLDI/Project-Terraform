using UnityEngine;

namespace Quests
{
    [CreateAssetMenu(fileName = "NewApproachPointQuest", menuName = "Quests/Approach Point Quest")]
    public class ApproachPointQuest : Quest
    {
        [Header("Настройки точки подхода")]
        public Vector3 targetPosition;
        public float markerHeightOffset = 2f;
        public GameObject questMarkerPrefab;

        [Header("Визуальные эффекты")]
        public bool showDebugSphere = true;
        public Color debugColor = Color.yellow;

        private GameObject activeMarker;
        private GameObject debugSphere;

        public override void StartQuest()
        {
            base.StartQuest();

            SpawnQuestMarker();

            Debug.Log($"Квест 'Подойти к точке' запущен. Точка: {targetPosition}");

            CheckQuestPointInScene();
        }

        void SpawnQuestMarker()
        {
            if (questMarkerPrefab != null)
            {
                Vector3 spawnPos = targetPosition + Vector3.up * markerHeightOffset;
                activeMarker = Instantiate(questMarkerPrefab, spawnPos, Quaternion.identity);
                activeMarker.name = $"QuestMarker_{questID}";
                Debug.Log($"? Создан маркер квеста в позиции: {spawnPos}");
            }
            else
            {
                Debug.LogWarning("?? questMarkerPrefab не назначен - маркер не создан");
            }
        }

        void CheckQuestPointInScene()
        {
            QuestPoint[] questPoints = GameObject.FindObjectsOfType<QuestPoint>();
            bool foundQuestPoint = false;

            foreach (QuestPoint qp in questPoints)
            {
                if (qp.linkedQuest == this)
                {
                    foundQuestPoint = true;
                    Debug.Log($"? Найден QuestPoint в сцене: {qp.gameObject.name} в позиции {qp.transform.position}");
                    break;
                }
            }

            if (!foundQuestPoint)
            {
                Debug.LogError($"? Не найден QuestPoint в сцене для квеста '{questName}'! Создайте QuestPoint вручную и привяжите этот квест.");
            }
        }

        public override void CompleteQuest()
        {
            if (activeMarker != null)
            {
                Debug.Log($"??? Удаляем маркер: {activeMarker.name}");
                Destroy(activeMarker);
                activeMarker = null;
            }

            if (debugSphere != null)
            {
                Debug.Log($"??? Удаляем debug сферу: {debugSphere.name}");
                Destroy(debugSphere);
                debugSphere = null;
            }

            base.CompleteQuest();

            Debug.Log($"? Квест 'Подойти к точке' полностью завершен!");
        }

        public override void ResetQuest()
        {
            if (activeMarker != null) Destroy(activeMarker);
            if (debugSphere != null) Destroy(debugSphere);

            base.ResetQuest();
        }
    }
}