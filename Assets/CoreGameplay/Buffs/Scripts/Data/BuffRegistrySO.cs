using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using System.Linq;

namespace Features.Buffs.Data
{
    [CreateAssetMenu(menuName = "Game/Registries/Buff Registry")]
    public class BuffRegistrySO : ScriptableObject
    {
        [Header("All Buffs")]
        [SerializeField] private List<BuffSO> allBuffs = new();

        private Dictionary<string, BuffSO> _idMap;
        private Dictionary<BuffSO, string> _buffMap;

        private static BuffRegistrySO _instance;
        public static BuffRegistrySO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UnityEngine.Resources.Load<BuffRegistrySO>("Databases/BuffRegistry");
                    if (_instance == null) Debug.LogError("[BuffRegistry] Not found in Resources/Databases/BuffRegistry!");
                    else _instance.BuildCache();
                }
                return _instance;
            }
        }

        private void OnEnable() => BuildCache();

        public void BuildCache()
        {
            if (allBuffs == null) return;
            _idMap = new Dictionary<string, BuffSO>();
            _buffMap = new Dictionary<BuffSO, string>();

            foreach (var b in allBuffs)
            {
                if (b == null) continue;
                // Используем buffId из твоего класса BuffSO
                string key = b.buffId; 
                if (string.IsNullOrEmpty(key)) key = b.name; // Fallback на имя файла

                if (!_idMap.ContainsKey(key))
                {
                    _idMap[key] = b;
                    _buffMap[b] = key;
                }
            }
        }

        public BuffSO GetById(string id)
        {
            if (_idMap == null) BuildCache();
            return _idMap.TryGetValue(id, out var b) ? b : null;
        }

        public string GetId(BuffSO b)
        {
            if (_buffMap == null) BuildCache();
            return _buffMap.TryGetValue(b, out var id) ? id : null;
        }

#if UNITY_EDITOR
        [ContextMenu("Find All Buffs Unique")]
        public void FindAllBuffs()
        {
            RebuildFromProjectAssets(log: true);
        }

        public bool RebuildFromProjectAssets(bool log = false)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:BuffSO");

            var byId = new Dictionary<string, BuffSO>();
            var duplicates = new List<string>();

            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var buff = UnityEditor.AssetDatabase.LoadAssetAtPath<BuffSO>(path);

                if (buff == null)
                    continue;

                string id = GetEditorKey(buff);

                if (string.IsNullOrWhiteSpace(id))
                {
                    Debug.LogWarning($"[BuffRegistry] Buff without id skipped: {path}", buff);
                    continue;
                }

                if (byId.TryGetValue(id, out var existing))
                {
                    duplicates.Add(
                        $"Duplicate buffId '{id}'\n" +
                        $"Keeping: {UnityEditor.AssetDatabase.GetAssetPath(existing)}\n" +
                        $"Skipped: {path}"
                    );
                    continue;
                }

                byId.Add(id, buff);
            }

            var rebuilt = byId
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToList();

            bool changed =
                allBuffs == null ||
                allBuffs.Count != rebuilt.Count ||
                !allBuffs.SequenceEqual(rebuilt);

            if (!changed)
            {
                if (log)
                    Debug.Log($"[BuffRegistry] Already up to date. Buffs: {rebuilt.Count}", this);

                return false;
            }

            allBuffs = rebuilt;
            BuildCache();

            UnityEditor.EditorUtility.SetDirty(this);

            if (log)
            {
                Debug.Log($"[BuffRegistry] Rebuilt. Unique buffs: {allBuffs.Count}", this);

                foreach (var duplicate in duplicates)
                    Debug.LogWarning($"[BuffRegistry] {duplicate}", this);
            }

            return true;
        }

        private static string GetEditorKey(BuffSO buff)
        {
            if (buff == null)
                return null;

            if (!string.IsNullOrWhiteSpace(buff.buffId))
                return buff.buffId.Trim();

            return buff.name;
        }
#endif
    }
}
