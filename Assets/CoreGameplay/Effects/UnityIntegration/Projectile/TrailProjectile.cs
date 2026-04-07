using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailProjectile : MonoBehaviour
{
    private TrailRenderer trail;
    private PooledProjectile pooled;

    private float lifetime;
    private float timer;

    private Vector3 start;
    private Vector3 end;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        pooled = GetComponent<PooledProjectile>();
    }

    public void Init(Vector3 start, Vector3 end, float duration)
    {
        this.start = start;
        this.end = end;

        lifetime = duration;
        timer = 0f;

        transform.position = start;

        trail.Clear();
        trail.emitting = true;

        // мгновенно "пролетаем"
        transform.position = end;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifetime)
        {
            Release();
        }
    }

    private void Release()
    {
        if (pooled != null)
            pooled.Release();
        else
            Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (trail != null)
            trail.Clear();
    }
}
