using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour, IProjectileVisual
{
    private LineRenderer line;
    private PooledProjectile pooled;

    private float timer;
    private float lifetime;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        pooled = GetComponent<PooledProjectile>();

        line.useWorldSpace = true;
    }

    public void Init(Vector3 start, Vector3 end, float duration)
    {
        lifetime = Mathf.Min(duration, 0.15f);
        timer = 0f;

        transform.position = Vector3.zero;

        line.enabled = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
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

    private void OnDisable()
    {
        if (line != null)
        {
            line.positionCount = 0;
            line.enabled = false;
        }

        timer = 0f;
    }
}
