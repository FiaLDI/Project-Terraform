using UnityEngine;

public static class ProjectileVisualService
{
    public static void Spawn(
        GameObject prefab,
        Vector3 pos,
        Vector3 dir,
        float speed)
    {
        if (prefab == null)
            return;

        var go = GameObject.Instantiate(
            prefab,
            pos,
            Quaternion.LookRotation(dir)
        );

        var proj = go.GetComponent<LocalProjectile>();
        if (proj != null)
            proj.Init(pos, dir, 0.2f);
    }
}
