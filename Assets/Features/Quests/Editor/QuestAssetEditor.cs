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

            serializedObject.ApplyModifiedProperties();
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
