using UnityEngine;


namespace Quests
{ 
    public class BiomeQuestGiver : MonoBehaviour
    {
        [Header("Квесты этого биома")]
        public QuestAsset[] biomeQuests;

        /// <summary>
        /// Выдаёт игроку все квесты этого биома (по клику в UI)
        /// </summary>
        public void GiveBiomeQuests()
        {
            if (QuestManager.Instance == null) return;

            foreach (var q in biomeQuests)
            {
                if (q != null)
                {
                    QuestManager.Instance.StartQuest(q);
                    Debug.Log($"🌍 BiomeQuestGiver: выдан квест '{q.questName}'");
                }
            }
        }
    }
}