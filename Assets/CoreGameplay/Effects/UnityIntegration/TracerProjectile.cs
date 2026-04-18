using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TracerProjectile : MonoBehaviour
{
    private LineRenderer line;
    private PooledProjectile pooled;

    private float lifetime;
    private float timer;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        pooled = GetComponent<PooledProjectile>();
    }

    public void Init(Vector3 start, Vector3 end, float duration)
    {
        lifetime = duration;
        timer = 0f;

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        enabled = true;
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
        if (line != null)
            line.positionCount = 0;

        timer = 0f;
    }
}
