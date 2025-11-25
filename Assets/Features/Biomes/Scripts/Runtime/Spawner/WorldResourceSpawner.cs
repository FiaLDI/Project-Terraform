using UnityEngine;
using System.Collections.Generic;

public class WorldResourceSpawner
{
    private readonly Vector2Int coord;
    private readonly int chunkSize;
    private readonly BiomeConfig biome;
    private readonly Transform parent;

    private readonly List<Vector3> spawnedPositions = new List<Vector3>();
    private readonly List<Blocker> blockers;

    public WorldResourceSpawner(
        Vector2Int coord,
        int chunkSize,
        BiomeConfig biome,
        Transform parent,
        List<Blocker> blockers)
    {
        this.coord = coord;
        this.chunkSize = chunkSize;
        this.biome = biome;
        this.parent = parent;
        this.blockers = blockers;
    }

    public void Spawn()
    {
        if (biome.possibleResources == null || biome.possibleResources.Length == 0)
            return;

        System.Random prng = new System.Random(coord.x * 928371 + coord.y * 527);
        int groups = Mathf.RoundToInt(chunkSize * chunkSize * biome.resourceDensity);

        for (int i = 0; i < groups; i++)
        {
            ResourceEntry entry = SelectWeightedResource(prng);
            if (entry == null) continue;

            if (prng.NextDouble() > entry.spawnChance) continue;
            if (!IsBiomeAllowed(entry)) continue;

            float cx = coord.x * chunkSize + (float)prng.NextDouble() * chunkSize;
            float cz = coord.y * chunkSize + (float)prng.NextDouble() * chunkSize;

            float h = BiomeHeightUtility.GetHeight(biome, cx, cz);
            Vector3 center = new Vector3(cx, h + 50f, cz);

            int count = prng.Next(entry.clusterCountRange.x, entry.clusterCountRange.y + 1);

            switch (entry.clusterType)
            {
                case ResourceClusterType.Single:
                    SpawnSingle(entry, center, prng);
                    break;

                case ResourceClusterType.CrystalVein:
                    SpawnCrystalVein(prng, entry, center, count);
                    break;

                case ResourceClusterType.RoundCluster:
                    SpawnRoundCluster(prng, entry, center, count);
                    break;

                case ResourceClusterType.VerticalStackNoise:
                    SpawnVerticalNoise(entry, center, count, prng);
                    break;
            }
        }
    }

    // ----------------- SPAWN ONE -----------------

    private void SpawnObject(ResourceEntry entry, Vector3 pos, System.Random prng)
    {
        float distribution = GetDistributionValue(entry, pos);
        if (prng.NextDouble() > distribution)
            return;

        if (!GroundSnapUtility.TrySnapWithNormal(
                pos,
                out Vector3 groundPos,
                out Quaternion normalRot,
                out float slope))
            return;

        if (slope < entry.minSlope || slope > entry.maxSlope)
            return;

        if (entry.useHeightLimit)
        {
            float height = groundPos.y;
            if (height < entry.minHeight || height > entry.maxHeight)
                return;
        }

        if (!PassesEdgeFalloff(groundPos, prng))
            return;

        if (entry.useMinDistance && !IsFarEnough(groundPos, entry.minDistance))
            return;

        // избегаем окружения (деревья/камни/квесты) по радиусу entry.environmentRadius
        if (entry.avoidEnvironment && IsNearBlockers(groundPos, entry.environmentRadius))
            return;

        Quaternion finalRot = Quaternion.identity;

        if (entry.alignToNormal)
            finalRot = normalRot;

        if (entry.randomYRotation)
        {
            float yaw = (float)prng.NextDouble() * 360f;
            finalRot *= Quaternion.Euler(0f, yaw, 0f);
        }

        float scale = 1f;
        if (entry.randomScale)
        {
            double t = prng.NextDouble();
            scale = Mathf.Lerp(entry.minScale, entry.maxScale, (float)t);
        }

        GameObject obj = Object.Instantiate(entry.resourcePrefab, groundPos, finalRot, parent);
        obj.transform.localScale *= scale;

        spawnedPositions.Add(groundPos);

        ResourceVisualizer.resourcePositions.Add(
            (groundPos, entry.clusterType)
        );
    }

    // ----------------- CHECKS -----------------

    private bool IsNearBlockers(Vector3 pos, float radius)
    {
        if (blockers == null || blockers.Count == 0)
            return false;

        float r2 = radius * radius;

        foreach (var b in blockers)
        {
            if ((pos - b.position).sqrMagnitude < r2)
                return true;
        }

        return false;
    }

    private float GetDistributionValue(ResourceEntry entry, Vector3 worldPos)
    {
        if (!entry.useDistributionMap)
            return 1f;

        float nx = (worldPos.x + entry.distributionOffset.x) * entry.distributionScale;
        float nz = (worldPos.z + entry.distributionOffset.y) * entry.distributionScale;

        float noise = Mathf.PerlinNoise(nx, nz);
        return Mathf.Pow(noise, entry.distributionStrength);
    }

    private bool PassesEdgeFalloff(Vector3 worldPos, System.Random prng)
    {
        float falloff = biome.resourceEdgeFalloff;
        if (Mathf.Approximately(falloff, 1f))
            return true;

        float localX = worldPos.x - coord.x * chunkSize;
        float localZ = worldPos.z - coord.y * chunkSize;

        float half = chunkSize * 0.5f;
        float dx = Mathf.Abs(localX - half);
        float dz = Mathf.Abs(localZ - half);

        float edgeFactor = Mathf.Max(dx, dz) / half;
        edgeFactor = Mathf.Clamp01(edgeFactor);

        float chance = Mathf.Lerp(1f, falloff, edgeFactor);
        return prng.NextDouble() <= chance;
    }

    private bool IsFarEnough(Vector3 pos, float minDist)
    {
        float minSqr = minDist * minDist;
        foreach (var p in spawnedPositions)
        {
            if ((pos - p).sqrMagnitude < minSqr)
                return false;
        }
        return true;
    }

    // ----------------- CLUSTER TYPES -----------------

     private void SpawnSingle(ResourceEntry entry, Vector3 center, System.Random prng)
    {
        SpawnObject(entry, center, prng);
    }

    private void SpawnCrystalVein(System.Random rng, ResourceEntry entry, Vector3 center, int count)
    {
        bool prevUseMinDist = entry.useMinDistance;
        entry.useMinDistance = false;

        for (int i = 0; i < count; i++)
        {
            float ox = ((float)rng.NextDouble() * 2f - 1f) * entry.clusterRadius;
            float oz = ((float)rng.NextDouble() * 2f - 1f) * entry.clusterRadius;

            Vector3 pos = new Vector3(center.x + ox, center.y, center.z + oz);
            SpawnObject(entry, pos, rng);
        }

        entry.useMinDistance = prevUseMinDist;
    }

    private void SpawnRoundCluster(System.Random rng, ResourceEntry entry, Vector3 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float dist = (float)rng.NextDouble() * entry.clusterRadius;

            Vector3 pos = new Vector3(
                center.x + Mathf.Cos(angle) * dist,
                center.y,
                center.z + Mathf.Sin(angle) * dist
            );

            SpawnObject(entry, pos, rng);
        }
    }

    private void SpawnVerticalNoise(ResourceEntry entry, Vector3 center, int count, System.Random rng)
    {
        bool prevUseMinDist = entry.useMinDistance;
        entry.useMinDistance = false;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = center + Vector3.up * i * entry.verticalStep;
            SpawnObject(entry, pos, rng);
        }

        entry.useMinDistance = prevUseMinDist;
    }

    // ----------------- HELPERS -----------------

    private bool IsBiomeAllowed(ResourceEntry entry)
    {
        if (entry.allowedBiomes == null || entry.allowedBiomes.Length == 0)
            return true;

        foreach (var t in entry.allowedBiomes)
        {
            if (t == biome.terrainType)
                return true;
        }

        return false;
    }

    private ResourceEntry SelectWeightedResource(System.Random rng)
    {
        float t = 0f;
        foreach (var e in biome.possibleResources)
            t += e.weight;

        if (t <= 0f) return null;

        float r = (float)rng.NextDouble() * t;

        foreach (var e in biome.possibleResources)
        {
            if (r < e.weight) return e;
            r -= e.weight;
        }

        return null;
    }
}
