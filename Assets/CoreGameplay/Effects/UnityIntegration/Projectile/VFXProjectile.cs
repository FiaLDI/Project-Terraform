using UnityEngine;
using UnityEngine.VFX;

public class VFXProjectile : MonoBehaviour, IProjectileVisual
{
    private VisualEffect vfx;
    private PooledProjectile pooled;

    private float timer;
    private float lifetime;

    private static readonly int StartID = Shader.PropertyToID("start");
    private static readonly int EndID = Shader.PropertyToID("end");

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();
        pooled = GetComponent<PooledProjectile>();
    }

    public void Init(Vector3 start, Vector3 end, float duration)
    {
        lifetime = Mathf.Min(duration, 0.15f);
        timer = 0f;

        transform.position = start;

        vfx.SetVector3(StartID, start);
        vfx.SetVector3(EndID, end);

        vfx.Play();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifetime)
            Release();
    }

    private void Release()
    {
        var pooled = GetComponent<PooledProjectile>();

        if (pooled != null)
            pooled.Release();
        else
            Destroy(gameObject);
    }
}
