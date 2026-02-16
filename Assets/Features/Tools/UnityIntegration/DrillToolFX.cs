using UnityEngine;

public class DrillToolFX : MonoBehaviour
{
    [Header("VFX")]
    public ParticleSystem sparks;
    public ParticleSystem dust;


    private bool isPlaying;

    public void Play(Vector3 position, Vector3 normal)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(normal);

        if (!isPlaying)
        {
            sparks?.Play();
            dust?.Play();
            isPlaying = true;
        }

    }

    public void Stop()
    {
        if (!isPlaying) return;

        sparks?.Stop();
        dust?.Stop();
        isPlaying = false;
    }
}
