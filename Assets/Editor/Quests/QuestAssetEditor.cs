using UnityEditor;
using UnityEngine;
using Quests;
using System;
using System.Linq;

[CustomEditor(typeof(QuestAsset))]
public class QuestAssetEditor : Editor
{
    private QuestAsset questAsset;
    private string[] behaviourTypes;
    private Type[] behaviourClasses;
    private int selectedIndex;

    private void OnEnable()
    {
        questAsset = (QuestAsset)target;

        // ищем все классы-наследники QuestBehaviour
        behaviourClasses = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(QuestBehaviour)) && !t.IsAbstract)
            .ToArray();

        behaviourTypes = behaviourClasses.Select(t => t.Name).ToArray();

        if (questAsset.behaviour != null)
        {
            var currentType = questAsset.behaviour.GetType();
            selectedIndex = Array.FindIndex(behaviourClasses, t => t == currentType);
            if (selectedIndex < 0) selectedIndex = 0;
        }
        else
        {
            selectedIndex = 0;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Заголовок
        EditorGUILayout.LabelField("⚔️ Quest Asset", EditorStyles.boldLabel);

        // Название и описание
        questAsset.questName = EditorGUILayout.TextField("Name", questAsset.questName);
        EditorGUILayout.LabelField("Description");
        questAsset.description = EditorGUILayout.TextArea(questAsset.description, GUILayout.Height(40));

        EditorGUILayout.Space();

        // Выбор типа поведения
        EditorGUILayout.LabelField("Quest Behaviour", EditorStyles.boldLabel);
        int newIndex = EditorGUILayout.Popup("Type", selectedIndex, behaviourTypes);
        if (newIndex != selectedIndex || questAsset.behaviour == null)
        {
            selectedIndex = newIndex;
            questAsset.behaviour = (QuestBehaviour)Activator.CreateInstance(behaviourClasses[selectedIndex]);
            EditorUtility.SetDirty(questAsset);
        }

        // Если есть поведение — рисуем его поля (но не дублируем сам объект)
        if (questAsset.behaviour != null)
        {
            SerializedObject so = new SerializedObject(questAsset);
            SerializedProperty behaviourProp = so.FindProperty("behaviour");

            if (behaviourProp != null && behaviourProp.hasVisibleChildren)
            {
                EditorGUILayout.LabelField("Тип поведения квеста", EditorStyles.boldLabel);

                var copy = behaviourProp.Copy();
                var end = copy.GetEndProperty();

                // проходим по всем вложенным свойствам behaviour
                while (copy.NextVisible(true) && !SerializedProperty.EqualContents(copy, end))
                {
                    EditorGUILayout.PropertyField(copy, true);
                }
            }

            so.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Progress", $"{questAsset.currentProgress}/{questAsset.targetProgress}");
    }
}
