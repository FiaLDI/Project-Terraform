using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FX/Audio/Sound Registry")]
public sealed class SoundRegistrySO : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string id;
        public SoundEffectConfig config;
    }

    [SerializeField] private Entry[] entries;

    private Dictionary<string, SoundEffectConfig> byId;
    private static SoundRegistrySO instance;

    public static SoundRegistrySO Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<SoundRegistrySO>("Databases/SoundRegistry");
                if (instance != null)
                    instance.BuildCache();
            }

            return instance;
        }
    }

    private void OnEnable()
    {
        instance = this;
        BuildCache();
    }

    private void BuildCache()
    {
        byId = new Dictionary<string, SoundEffectConfig>();

        if (entries == null)
            return;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (string.IsNullOrWhiteSpace(entry.id) || entry.config == null)
                continue;

            byId[entry.id] = entry.config;
        }
    }

    public static SoundEffectConfig Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var registry = Instance;
        if (registry == null || registry.byId == null)
            return null;

        return registry.byId.TryGetValue(id, out var config)
            ? config
            : null;
    }
}
