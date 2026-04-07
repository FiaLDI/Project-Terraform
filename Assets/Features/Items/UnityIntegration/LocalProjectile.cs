using UnityEngine;

public class LocalProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private float lifetime;
    private float timer;

    private PooledProjectile pooled;

    private void Awake()
    {
        pooled = GetComponent<PooledProjectile>();
    }

    public void Init(Vector3 dir, float speed)
    {
        velocity = dir * speed;

        lifetime = 2f; // можно вынести в config
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
        if (pooled != null)
        {
            pooled.Release();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        velocity = Vector3.zero;
        timer = 0f;
    }
}
