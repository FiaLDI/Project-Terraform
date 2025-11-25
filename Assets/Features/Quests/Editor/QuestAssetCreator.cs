using UnityEngine;
using UnityEditor;
using System.IO;
using Quests;

public static class QuestAssetCreator
{
    [MenuItem("Assets/Create/Quests/New Quest Asset", priority = 0)]
    public static void CreateQuestAsset()
    {
        QuestAsset asset = ScriptableObject.CreateInstance<QuestAsset>();

        asset.questName = "New Quest";
        asset.description = "Quest description...";

        asset.rewards = new RewardItem[]
        {
            new RewardItem()
        };

        string path = "Assets/Quests";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/NewQuest.asset");

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log($"✅ QuestAsset создан: {assetPath}");
    }
}
