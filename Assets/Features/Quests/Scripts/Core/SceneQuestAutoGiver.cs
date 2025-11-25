using UnityEngine;


namespace Quests
{
    public class SceneQuestAutoGiver : MonoBehaviour
    {
        [Header("Квесты, которые выдаются при входе в сцену")]
        public QuestAsset[] sceneStartQuests;

        private void Start()
        {
            GiveStartQuests();
        }

        private void GiveStartQuests()
        {
            if (QuestManager.Instance == null) return;

            foreach (var q in sceneStartQuests)
            {
                if (q != null)
                {
                    QuestManager.Instance.StartQuest(q);
                    Debug.Log($"🎬 SceneQuestAutoGiver: выдан стартовый квест '{q.questName}'");
                }
            }
        }
    }
}