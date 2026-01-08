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
        [ContextMenu("Find All Buffs")]
        public void FindAllBuffs()
        {
            allBuffs = UnityEditor.AssetDatabase.FindAssets("t:BuffSO")
                .Select(guid => UnityEditor.AssetDatabase.LoadAssetAtPath<BuffSO>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)))
                .Where(b => b != null)
                .ToList();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
