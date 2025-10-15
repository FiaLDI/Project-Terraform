using UnityEngine;

namespace Quests
{
    [CreateAssetMenu(fileName = "NewStandOnPointQuest", menuName = "Quests/Stand On Point Quest")]
    public class StandOnPointQuest : Quest
    {
        [Header("Настройки стояния на точке")]
        public Vector3 targetPosition;
        public float requiredStandTime = 30f;
        public float standRadius = 3f;

        [Header("Визуальные эффекты")]
        public GameObject zoneVisualPrefab;
        public bool showProgressUI = true;

        private GameObject zoneVisual;
        private StandZone standZone;
        private float currentStandTime = 0f;

        public override void StartQuest()
        {
            base.StartQuest();
            currentStandTime = 0f;

            RemovePreviousQuestMarkers();

            CreateStandZoneInScene();
            UpdateProgressUI();

            Debug.Log($"Квест 'Стоять на точке' запущен. Стоять {requiredStandTime} сек в зоне");
        }

        private void RemovePreviousQuestMarkers()
        {
            GameObject[] previousMarkers = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in previousMarkers)
            {
                if (obj.name.Contains("QuestMarker") ||
                    obj.name.Contains("QuestMarker") ||
                    obj.name.Contains("DebugSphere") ||
                    (obj.GetComponent<QuestPoint>() != null && obj.transform.childCount == 0))
                {
                    Debug.Log($"??? Удаляем маркер предыдущего квеста: {obj.name}");
                    Destroy(obj);
                }
            }

            GameObject[] cylinders = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in cylinders)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    if (meshFilter.sharedMesh.name.Contains("Cylinder") &&
                        obj.transform.parent == null) 
                    {
                        if (obj.name.Contains("Quest") ||
                            obj.name.Contains("Marker") ||
                            Vector3.Distance(obj.transform.position, targetPosition) > 10f) 
                        {
                            Debug.Log($"??? Удаляем цилиндр-маркер: {obj.name}");
                            Destroy(obj);
                        }
                    }
                }
            }
        }

        void CreateStandZoneInScene()
        {
            GameObject zoneObj = new GameObject($"StandZone_{questID}");
            zoneObj.transform.position = targetPosition;

            SphereCollider collider = zoneObj.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = standRadius;

            standZone = zoneObj.AddComponent<StandZone>();
            standZone.Initialize(this);

            if (zoneVisualPrefab != null)
            {
                zoneVisual = Instantiate(zoneVisualPrefab, targetPosition, Quaternion.identity);
                zoneVisual.transform.localScale = Vector3.one * standRadius * 2f;
            }
            else
            {
                zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                zoneVisual.transform.position = targetPosition;
                zoneVisual.transform.localScale = Vector3.one * standRadius * 2f;

                Renderer renderer = zoneVisual.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(1f, 0.5f, 0f, 0.3f);
                }
                zoneVisual.GetComponent<Collider>().enabled = false;
                zoneVisual.name = "StandZone_Visual";
            }
        }

        public void UpdateStandTime(float deltaTime)
        {
            if (isCompleted || !isActive) return;

            currentStandTime += deltaTime;

            UpdateProgressUI();

            currentProgress = Mathf.RoundToInt(Mathf.Clamp01(currentStandTime / requiredStandTime) * targetProgress);

            NotifyQuestUpdated();


            if (currentStandTime >= requiredStandTime)
            {
                currentProgress = targetProgress;

                CompleteQuest();
            }
        }


        void UpdateProgressUI()
        {
            if (showProgressUI)
            {
                int remainingTime = Mathf.CeilToInt(requiredStandTime - currentStandTime);
                Debug.Log($"Осталось стоять: {remainingTime} сек");
            }
        }

        public override void CompleteQuest()
        {
            base.CompleteQuest();

            if (zoneVisual != null)
            {
                Debug.Log($"??? Удаляем визуал зоны стояния: {zoneVisual.name}");
                Destroy(zoneVisual);
            }

            if (standZone != null && standZone.gameObject != null)
            {
                Debug.Log($"??? Удаляем зону стояния: {standZone.gameObject.name}");
                Destroy(standZone.gameObject);
            }

            Debug.Log($"Квест 'Стоять на точке' завершен! Стояли {currentStandTime:F1} сек");
        }

        public override void ResetQuest()
        {
            base.ResetQuest();

            if (zoneVisual != null) Destroy(zoneVisual);
            if (standZone != null && standZone.gameObject != null) Destroy(standZone.gameObject);
        }

        public float GetProgressPercent()
        {
            return Mathf.Clamp01(currentStandTime / requiredStandTime);
        }
    }
}