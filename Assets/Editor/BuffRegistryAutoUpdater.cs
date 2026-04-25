#if UNITY_EDITOR

using System;
using System.Linq;
using Features.Buffs.Data;
using Features.Buffs.Domain;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Features.Buffs.EditorTools
{
    public static class BuffRegistryAutoUpdater
    {
        private static bool scheduled;

        // ======================================================
        // MENU
        // ======================================================

        [MenuItem("Tools/Buffs/Rebuild Buff Registry")]
        public static void RebuildFromMenu()
        {
            RebuildAllRegistries(log: true);
        }

        // ======================================================
        // OPEN ASSET TRIGGER
        // ======================================================

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);

            if (obj is BuffSO || obj is BuffRegistrySO)
                ScheduleRebuild();

            // false = не перехватываем открытие asset
            return false;
        }

        // ======================================================
        // SCHEDULING
        // ======================================================

        public static void ScheduleRebuild()
        {
            if (scheduled)
                return;

            scheduled = true;

            EditorApplication.delayCall += () =>
            {
                scheduled = false;

                if (EditorApplication.isCompiling)
                    return;

                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                RebuildAllRegistries(log: false);
            };
        }

        // ======================================================
        // REBUILD
        // ======================================================

        private static void RebuildAllRegistries(bool log)
        {
            var registries = AssetDatabase.FindAssets("t:BuffRegistrySO")
                .Select(guid =>
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    return AssetDatabase.LoadAssetAtPath<BuffRegistrySO>(path);
                })
                .Where(r => r != null)
                .ToList();

            if (registries.Count == 0)
            {
                if (log)
                    Debug.LogWarning("[BuffRegistryAutoUpdater] No BuffRegistrySO found.");

                return;
            }

            bool anyChanged = false;

            foreach (var registry in registries)
            {
                bool changed = registry.RebuildFromProjectAssets(log);
                anyChanged |= changed;
            }

            if (anyChanged)
                AssetDatabase.SaveAssets();
        }
    }

    public sealed class BuffRegistryAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (EditorApplication.isCompiling)
                return;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (ContainsRelevantAsset(importedAssets) ||
                ContainsRelevantAsset(deletedAssets) ||
                ContainsRelevantAsset(movedAssets) ||
                ContainsRelevantAsset(movedFromAssetPaths))
            {
                BuffRegistryAutoUpdater.ScheduleRebuild();
            }
        }

        private static bool ContainsRelevantAsset(string[] paths)
        {
            if (paths == null)
                return false;

            foreach (string path in paths)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                if (!path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                    continue;

                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

                if (type == typeof(BuffSO))
                    return true;

                if (type == typeof(BuffRegistrySO))
                    return true;

                // Для deleted/movedFrom AssetDatabase уже может не знать type.
                // Поэтому fallback по имени файла.
                if (path.IndexOf("Buff", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}

#endif
