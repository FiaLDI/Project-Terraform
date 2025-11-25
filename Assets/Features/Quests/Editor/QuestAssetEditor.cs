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

    SerializedProperty descriptionProp;
    SerializedProperty behaviourProp;
    SerializedProperty rewardsProp;

    private void OnEnable()
    {
        questAsset = (QuestAsset)target;

        descriptionProp = serializedObject.FindProperty("description");
        behaviourProp = serializedObject.FindProperty("behaviour");
        rewardsProp = serializedObject.FindProperty("rewards");

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

        EditorGUILayout.LabelField("⚔️ Quest Asset", EditorStyles.boldLabel);

        questAsset.questName = EditorGUILayout.TextField("Name", questAsset.questName);
        EditorGUILayout.PropertyField(descriptionProp);

        EditorGUILayout.Space();

        // === BEHAVIOUR SELECTOR ===
        EditorGUILayout.LabelField("Quest Behaviour", EditorStyles.boldLabel);
        int newIndex = EditorGUILayout.Popup("Type", selectedIndex, behaviourTypes);
        if (newIndex != selectedIndex || questAsset.behaviour == null)
        {
            selectedIndex = newIndex;
            questAsset.behaviour = (QuestBehaviour)Activator.CreateInstance(behaviourClasses[selectedIndex]);
            EditorUtility.SetDirty(questAsset);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Progress Settings", EditorStyles.boldLabel);
        questAsset.targetProgress = EditorGUILayout.IntField("Required Targets", questAsset.targetProgress);

        EditorGUILayout.PropertyField(behaviourProp, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rewards", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rewardsProp, true);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Progress", $"{questAsset.currentProgress}/{questAsset.targetProgress}");
    }
}
