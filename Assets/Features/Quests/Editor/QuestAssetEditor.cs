using UnityEditor;
using UnityEngine;
using Features.Quests.Data; // ✅ QUEST ASSET находится здесь (проверь)
using Features.Quests.Domain;

namespace Features.Quests.Editor
{
    [CustomEditor(typeof(QuestAsset))]
    public class QuestAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var quest = (QuestAsset)target;

            EditorGUILayout.LabelField("Quest ID", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(quest.questId);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);

            if (string.IsNullOrWhiteSpace(quest.questId))
            {
                if (GUILayout.Button("Generate ID"))
                {
                    Undo.RecordObject(quest, "Generate Quest ID");
                    quest.questId = GenerateQuestId(quest);
                    EditorUtility.SetDirty(quest);
                }
            }
            else
            {
                if (GUILayout.Button("Regenerate ID"))
                {
                    Undo.RecordObject(quest, "Regenerate Quest ID");
                    quest.questId = GenerateQuestId(quest);
                    EditorUtility.SetDirty(quest);
                }
            }

            EditorGUILayout.Space(10);

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }

        private string GenerateQuestId(QuestAsset quest)
        {
            // имя может быть null → исправляем
            string name = string.IsNullOrWhiteSpace(quest.questName)
                ? "quest"
                : quest.questName;

            string safeName = name
                .ToLower()
                .Replace(" ", "_")
                .Replace("-", "_");

            string guid = System.Guid.NewGuid().ToString("N").Substring(0, 6);

            return $"quest_{safeName}_{guid}";
        }

    }
}
