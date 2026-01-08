using UnityEngine;
using System.Collections.Generic;
using Features.Passives.Domain;
using System.Linq;

namespace Features.Passives.Data
{
    [CreateAssetMenu(menuName = "Game/Registries/Passive Registry")]
    public class PassiveRegistrySO : ScriptableObject
    {
        [Header("All Passives")]
        [SerializeField] private List<PassiveSO> allPassives = new();

        // Кэши
        private Dictionary<string, PassiveSO> _idMap;
        private Dictionary<PassiveSO, string> _passiveMap;

        private static PassiveRegistrySO _instance;

        /// <summary>
        /// Глобальный доступ к реестру.
        /// Загружает файл из Resources/Databases/PassiveRegistry
        /// </summary>
        public static PassiveRegistrySO Instance
        {
            get
            {
                if (_instance == null)
                {
                    // ВАЖНО: Файл должен называться "PassiveRegistry" и лежать в "Assets/Resources/Databases/"
                    _instance = UnityEngine.Resources.Load<PassiveRegistrySO>("Databases/PassiveRegistry");
                    
                    if (_instance == null)
                    {
                        Debug.LogError("[PassiveRegistry] Critical: Could not load registry from Resources/Databases/PassiveRegistry! Check file location.");
                    }
                    else
                    {
                        _instance.BuildCache();
                    }
                }
                return _instance;
            }
        }

        private void OnEnable()
        {
            // В редакторе перестраиваем кэш при перезагрузке
            BuildCache();
        }

        public void BuildCache()
        {
            if (allPassives == null) return;

            _idMap = new Dictionary<string, PassiveSO>();
            _passiveMap = new Dictionary<PassiveSO, string>();

            foreach (var p in allPassives)
            {
                if (p == null) continue;

                // Используем поле id, если заполнено, иначе имя ассета
                // Убедись, что в инспекторе у SO заполнено поле 'id', или просто используй p.name для простоты
                string key = string.IsNullOrEmpty(p.id) ? p.name : p.id;

                if (!_idMap.ContainsKey(key))
                {
                    _idMap[key] = p;
                    _passiveMap[p] = key;
                }
                else
                {
                    Debug.LogWarning($"[PassiveRegistry] Duplicate ID detected: '{key}'. Passive '{p.name}' will be ignored.");
                }
            }
            
            // Debug.Log($"[PassiveRegistry] Initialized with {_idMap.Count} entries.");
        }

        public PassiveSO GetById(string id)
        {
            if (_idMap == null) BuildCache();
            if (string.IsNullOrEmpty(id)) return null;

            return _idMap.TryGetValue(id, out var p) ? p : null;
        }

        public string GetId(PassiveSO p)
        {
            if (_passiveMap == null) BuildCache();
            if (p == null) return null;

            return _passiveMap.TryGetValue(p, out var id) ? id : null;
        }

#if UNITY_EDITOR
        [ContextMenu("Find All Passives")]
        public void FindAllPassives()
        {
            allPassives = UnityEditor.AssetDatabase.FindAssets("t:PassiveSO")
                .Select(guid => UnityEditor.AssetDatabase.LoadAssetAtPath<PassiveSO>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)))
                .Where(p => p != null)
                .ToList();
            
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[PassiveRegistry] Found {allPassives.Count} passives.");
        }
#endif
    }
}
