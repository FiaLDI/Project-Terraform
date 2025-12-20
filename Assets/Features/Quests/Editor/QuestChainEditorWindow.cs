using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Features.Quests.Data;
using Features.Quests.Domain;

namespace Features.Quests.Editor
{
    public class QuestEditorWindow : EditorWindow
    {
        private QuestDatabaseAsset database;
        private Vector2 scroll;

        private List<QuestAsset> quests = new();
        private List<QuestChainAsset> chains = new();

        [MenuItem("Game/Quest Editor")]
        public static void Open()
        {
            GetWindow<QuestEditorWindow>("Quest Editor").Show();
        }

        private void OnEnable()
        {
            RefreshAssets();
        }

        private void RefreshAssets()
        {
            quests = AssetDatabase.FindAssets("t:QuestAsset")
                .Select(guid => AssetDatabase.LoadAssetAtPath<QuestAsset>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(q => q != null)
                .ToList();

            chains = AssetDatabase.FindAssets("t:QuestChainAsset")
                .Select(guid => AssetDatabase.LoadAssetAtPath<QuestChainAsset>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(c => c != null)
                .ToList();

            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Quest Database", EditorStyles.boldLabel);
            database = (QuestDatabaseAsset)EditorGUILayout.ObjectField(database, typeof(QuestDatabaseAsset), false);

            if (database == null)
            {
                EditorGUILayout.HelpBox("Привяжи QuestDatabaseAsset.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("⟳ Обновить список квестов"))
            {
                RefreshAssets();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("🔄 Синхронизировать Database"))
            {
                UpdateDatabase();
            }

            EditorGUILayout.Space(20);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawQuests();
            EditorGUILayout.Space(20);
            DrawChains();

            EditorGUILayout.EndScrollView();
        }

        private void DrawQuests()
        {
            EditorGUILayout.LabelField("Все квесты", EditorStyles.boldLabel);

            foreach (var q in quests)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"ID: {q.questId}", GUILayout.Width(200));
                if (GUILayout.Button("Открыть", GUILayout.Width(70)))
                    Selection.activeObject = q;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"Название: {q.questName}");
                EditorGUILayout.LabelField($"Описание: {q.description}");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawChains()
        {
            EditorGUILayout.LabelField("Цепочки квестов", EditorStyles.boldLabel);

            foreach (var chain in chains)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(chain.chainName, GUILayout.Width(200));
                if (GUILayout.Button("Открыть", GUILayout.Width(70)))
                    Selection.activeObject = chain;
                EditorGUILayout.EndHorizontal();

                foreach (var q in chain.quests)
                {
                    if (q != null)
                        EditorGUILayout.LabelField($"→ {q.questName}");
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void UpdateDatabase()
        {
            Undo.RecordObject(database, "Update QuestDatabase");

            var list = new List<QuestAsset>();

            foreach (var q in quests)
            {
                if (q != null)
                    list.Add(q);
            }

            // Это поле нужно добавить в QuestDatabaseAsset:
            // public List<QuestAsset> quests = new();
            SerializedObject so = new SerializedObject(database);
            var questsProp = so.FindProperty("quests");

            questsProp.ClearArray();

            for (int i = 0; i < list.Count; i++)
            {
                questsProp.InsertArrayElementAtIndex(i);
                questsProp.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            }

            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            Debug.Log("✔ QuestDatabase обновлена");
        }
    }
}
