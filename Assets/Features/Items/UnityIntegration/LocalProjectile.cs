using UnityEngine;

public class LocalProjectile : MonoBehaviour, IProjectileVisual
{
    [SerializeField] private float speed = 50f;

    private Vector3 velocity;
    private float lifetime;
    private float timer;

    private void Awake()
    {
        // pooled больше не кешируем (как и в других)
    }

    // 🔥 ЭТО ТРЕБУЕТ ИНТЕРФЕЙС
    public void Init(Vector3 start, Vector3 end, float duration)
    {
        transform.position = start;

        Vector3 dir = (end - start).normalized;

        velocity = dir * speed;

        lifetime = duration > 0 ? duration : 2f;
        timer = 0f;
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Release();
        }
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
        velocity = Vector3.zero;
        timer = 0f;
    }
}
