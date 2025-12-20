using UnityEngine;
using Features.Quests.Domain;

namespace Features.Quests.UnityIntegration
{
    [DefaultExecutionOrder(-1000)]
    public sealed class QuestManagerMB : MonoBehaviour
    {
        public static QuestManagerMB Instance { get; private set; }

        public QuestService Service { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void Bind(QuestService service)
        {
            Service = service;
            Debug.Log("✔ QuestManagerMB: bound to PlayerQuestController service");
        }

        private void Update()
        {
            Service?.HandleEvent(new TickEvent(Time.deltaTime));
        }
    }
}
