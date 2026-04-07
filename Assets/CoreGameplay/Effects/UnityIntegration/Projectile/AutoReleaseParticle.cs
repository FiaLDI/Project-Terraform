using UnityEngine;

public class AutoReleaseParticle : MonoBehaviour
{
    private ParticleSystem ps;
    private PooledProjectile pooled;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        pooled = GetComponent<PooledProjectile>();
    }

    private void OnEnable()
    {
        if (ps != null)
            ps.Play();
    }

    private void Update()
    {
        if (ps != null && !ps.IsAlive())
        {
            if (pooled != null)
                pooled.Release();
            else
                Destroy(gameObject);
        }
    }
}
