using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FX/Audio/Sound Effect Config")]
public sealed class SoundEffectConfig : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Стабильный ID для сетевой передачи, если звук нужно будет резолвить на клиенте по ключу.")]
    public string id;

    [Header("Source")]
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Min(0f)]
    [Tooltip("Минимальный интервал между повторными проигрываниями одного и того же эффекта.")]
    public float minInterval = 0.05f;

    [Header("Pitch Random")]
    public float pitchMin = 1f;
    public float pitchMax = 1f;

    [Header("3D Settings")]
    [Range(0f, 1f)]
    public float spatialBlend = 1f;

    [Min(0.01f)]
    public float minDistance = 1f;

    [Min(0.01f)]
    public float maxDistance = 30f;
}

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
