using UnityEngine;

public class DrillToolFX : MonoBehaviour
{
    [Header("VFX References")]
    public ParticleSystem sparks;
    public ParticleSystem dust;
    public ParticleSystem shards;

    [Header("Heat / Overheat FX")]
    public ParticleSystem heatSmoke;

    [Header("Audio")]
    public AudioSource loopAudio;
    public AudioSource impactAudio;

    private bool isPlaying;

    public void Play(Vector3 position, Vector3 normal)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(-normal);

        if (!isPlaying)
        {
            sparks?.Play();
            dust?.Play();
            shards?.Play();
            loopAudio?.Play();
        }

        impactAudio?.Play();
        isPlaying = true;
    }

    public void Stop()
    {
        if (!isPlaying) return;

        sparks?.Stop();
        dust?.Stop();
        shards?.Stop();
        loopAudio?.Stop();
        isPlaying = false;
    }

    public void SetOverheat(bool overheating)
    {
        if (overheating)
        {
            if (!heatSmoke.isPlaying)
                heatSmoke?.Play();
        }
        else
        {
            heatSmoke?.Stop();
        }
    }
}
