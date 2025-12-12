#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Features.Quests.Data;
using Features.Enemy.Data;

namespace Features.Quests.Editor
{
    [CustomEditor(typeof(QuestAsset))]
    public class QuestAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var quest = (QuestAsset)target;

            // --------------------------
            // QUEST ID BLOCK
            // --------------------------
            EditorGUILayout.LabelField("Quest ID", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(quest.questId);
            EditorGUI.EndDisabledGroup();

            if (string.IsNullOrWhiteSpace(quest.questId))
            {
                if (GUILayout.Button("Generate Quest ID"))
                {
                    Undo.RecordObject(quest, "Generate Quest ID");
                    quest.questId = GenerateQuestId(quest);
                    EditorUtility.SetDirty(quest);
                }
            }
            else
            {
                if (GUILayout.Button("Regenerate Quest ID"))
                {
                    Undo.RecordObject(quest, "Regenerate Quest ID");
                    quest.questId = GenerateQuestId(quest);
                    EditorUtility.SetDirty(quest);
                }
            }

            EditorGUILayout.Space(15);

            // --------------------------
            // DRAW DEFAULT INSPECTOR
            // --------------------------
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            // --------------------------
            // CUSTOM UI FOR KillEnemies
            // --------------------------
            if (quest.behaviourType == QuestBehaviourType.KillEnemies)
            {
                DrawKillEnemiesSection(quest);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawKillEnemiesSection(QuestAsset quest)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Kill Enemies Parameters", EditorStyles.boldLabel);

            // Database selector
            quest.enemyDatabase = (EnemyDatabaseSO)EditorGUILayout.ObjectField(
                "Enemy Database", quest.enemyDatabase,
                typeof(EnemyDatabaseSO), false
            );

            if (quest.enemyDatabase == null)
            {
                EditorGUILayout.HelpBox("Assign EnemyDatabaseSO to choose enemy types.", MessageType.Info);
                return;
            }

            var ids = quest.enemyDatabase.GetAllIds();
            if (ids == null || ids.Length == 0)
            {
                EditorGUILayout.HelpBox("EnemyDatabase is empty.", MessageType.Warning);
                return;
            }

            // Auto-generate enemyId if empty
            if (string.IsNullOrWhiteSpace(quest.enemyId))
            {
                Undo.RecordObject(quest, "Auto-generate EnemyId");
                quest.enemyId = ids[0];      // первое значение из базы
                EditorUtility.SetDirty(quest);
            }

            int currentIndex = Mathf.Max(0, System.Array.IndexOf(ids, quest.enemyId));
            int newIndex = EditorGUILayout.Popup("Enemy ID", currentIndex, ids);

            if (newIndex != currentIndex && newIndex >= 0 && newIndex < ids.Length)
            {
                Undo.RecordObject(quest, "Select EnemyID");
                quest.enemyId = ids[newIndex];
                EditorUtility.SetDirty(quest);
            }

            quest.requiredKills = EditorGUILayout.IntField("Required Kills", quest.requiredKills);
        }


        // ===============================
        // UTILS
        // ===============================

        private string GenerateQuestId(QuestAsset quest)
        {
            string name = string.IsNullOrWhiteSpace(quest.questName)
                ? "quest"
                : quest.questName;

            string safe = name
                .ToLower()
                .Replace(" ", "_")
                .Replace("-", "_");

            string guid = System.Guid.NewGuid().ToString("N").Substring(0, 6);

            return $"quest_{safe}_{guid}";
        }
    }
}
#endif
