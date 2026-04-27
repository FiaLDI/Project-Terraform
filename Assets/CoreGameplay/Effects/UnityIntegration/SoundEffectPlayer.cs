using System.Collections.Generic;
using UnityEngine;

namespace Features.Effects.Application
{
    public sealed class SoundEffectPlayer : MonoBehaviour
    {
        private static SoundEffectPlayer instance;
        private readonly Dictionary<string, float> lastPlayTimeByKey = new();

        public static SoundEffectPlayer Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                var go = new GameObject("[SoundEffectPlayer]");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<SoundEffectPlayer>();
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Play(SoundEffectConfig config, Vector3 position, string throttleKey = null)
        {
            if (config == null || config.clip == null)
                return;

            if (!CanPlay(config, throttleKey))
                return;

            var clip = config.clip;
            var go = new GameObject($"[SFX] {clip.name}");
            go.transform.position = position;

            var audioSource = go.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = config.volume > 0f
                ? Mathf.Clamp01(config.volume)
                : 1f;

            float pitchMin = Mathf.Approximately(config.pitchMin, 0f) && Mathf.Approximately(config.pitchMax, 0f)
                ? 1f
                : Mathf.Min(config.pitchMin, config.pitchMax);

            float pitchMax = Mathf.Approximately(config.pitchMin, 0f) && Mathf.Approximately(config.pitchMax, 0f)
                ? 1f
                : Mathf.Max(config.pitchMin, config.pitchMax);

            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.spatialBlend = Mathf.Clamp01(config.spatialBlend);
            audioSource.minDistance = config.minDistance > 0f ? config.minDistance : 1f;
            audioSource.maxDistance = config.maxDistance > 0f
                ? Mathf.Max(audioSource.minDistance, config.maxDistance)
                : 30f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.Play();

            var lifetime = clip.length;
            if (Mathf.Abs(audioSource.pitch) > 0.001f)
                lifetime /= Mathf.Abs(audioSource.pitch);

            Destroy(go, lifetime + 0.1f);
        }

        private bool CanPlay(SoundEffectConfig config, string throttleKey)
        {
            if (string.IsNullOrWhiteSpace(throttleKey) || config.minInterval <= 0f)
                return true;

            if (lastPlayTimeByKey.TryGetValue(throttleKey, out var lastPlayTime))
            {
                if (Time.unscaledTime - lastPlayTime < config.minInterval)
                    return false;
            }

            lastPlayTimeByKey[throttleKey] = Time.unscaledTime;
            return true;
        }
    }
}
