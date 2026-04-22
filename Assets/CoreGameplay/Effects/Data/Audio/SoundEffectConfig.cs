using UnityEngine;

[CreateAssetMenu(menuName = "FX/Audio/Sound Effect Config")]
public sealed class SoundEffectConfig : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable id used to resolve this sound on clients over the network.")]
    public string id;

    [Header("Source")]
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Min(0f)]
    [Tooltip("Minimum interval between repeated plays of the same sound effect.")]
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
