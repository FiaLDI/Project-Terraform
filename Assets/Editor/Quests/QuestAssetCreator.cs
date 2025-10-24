using UnityEngine;
using UnityEditor;
using System.IO;
using Quests;

public static class QuestAssetCreator
{
    [MenuItem("Assets/Create/Quests/New Quest Asset", priority = 0)]
    public static void CreateQuestAsset()
    {
        // Создаём ScriptableObject
        QuestAsset asset = ScriptableObject.CreateInstance<QuestAsset>();

        // Автоматически заполняем ID и дефолтные данные
        asset.questID = System.Guid.NewGuid().ToString();
        asset.questName = "New Quest";

        // Папка по умолчанию
        string path = "Assets/Quests";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        // Генерация уникального имени
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/NewQuest.asset");

        // Создание ассета
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();

        // Выделяем ассет в Project
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log($"✅ QuestAsset создан: {assetPath} (ID: {asset.questID})");
    }
}
