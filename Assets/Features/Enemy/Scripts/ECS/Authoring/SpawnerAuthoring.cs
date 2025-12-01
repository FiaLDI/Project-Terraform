using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject EnemyPrefab; // Обычный префаб Unity
    public int Count = 100;
    public float Radius = 20f;

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new EnemySpawner
            {
                // Важно! GetEntity преобразует GameObject префаб в Entity префаб
                PrefabToSpawn = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic),
                NumberToSpawn = authoring.Count,
                SpawnRadius = authoring.Radius,
                Initialized = false
            });
        }
    }
}