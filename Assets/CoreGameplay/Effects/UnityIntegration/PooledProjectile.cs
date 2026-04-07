using UnityEngine;

public sealed class PooledProjectile : MonoBehaviour
{
    private ProjectilePool pool;
    private GameObject prefab;

    public void Init(ProjectilePool pool, GameObject prefab)
    {
        this.pool = pool;
        this.prefab = prefab;
    }

    public void Release()
    {
        if (pool != null)
            pool.Release(gameObject, prefab);
    }
}
