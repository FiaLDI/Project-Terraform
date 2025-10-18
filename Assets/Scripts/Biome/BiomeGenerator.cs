using UnityEngine;
using System.Collections;
using Quests;

public class BiomeGenerator : MonoBehaviour
{
    public BiomeConfig biome;
    public int chunkSize = 32; // размер чанка
    public bool autoSpawnQuests = true; // ⚡ режим автоспавна квестов

    private Coroutine fogRoutine;
    private GameObject biomeRoot; // чтобы убирать предыдущую генерацию

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        if (biome == null)
        {
            Debug.LogWarning("❌ BiomeConfig не назначен!");
            return;
        }

        // Удалим предыдущую генерацию
        if (biomeRoot != null)
        {
            DestroyImmediate(biomeRoot);
            biomeRoot = null;
        }

        // ✅ Skybox
        if (biome.skyboxMaterial != null)
        {
            RenderSettings.skybox = biome.skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            Debug.Log($"🌌 Skybox для биома '{biome.biomeName}' применён.");
        }

        // ✅ Fog
        ApplyFogFromBiome();

        biomeRoot = new GameObject(biome.biomeName + "_Generated");

        int width = biome.width;
        int height = biome.height;

        for (int cz = 0; cz < height; cz += chunkSize)
        {
            for (int cx = 0; cx < width; cx += chunkSize)
            {
                int w = Mathf.Min(chunkSize, width - cx);
                int h = Mathf.Min(chunkSize, height - cz);

                GameObject chunk = GenerateChunk(cx, cz, w, h, biome);
                chunk.transform.parent = biomeRoot.transform;
            }
        }

        if (autoSpawnQuests)
        {
            SpawnQuests();
            Debug.Log($"✅ Biome '{biome.biomeName}' сгенерирован и квесты заспавнены!");
        }
        else
        {
            Debug.Log($"✅ Biome '{biome.biomeName}' сгенерирован (квесты ожидаются в сцене вручную).");
        }

        SpawnEnvironment();
    }

    private GameObject GenerateChunk(int startX, int startZ, int width, int height, BiomeConfig biome)
    {
        GameObject chunkObj = new GameObject($"Chunk_{startX}_{startZ}");

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        int[] triangles = new int[width * height * 6];

        // вершины
        for (int z = 0, i = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float baseNoise = Mathf.PerlinNoise(
                    (startX + x) * biome.terrainScale * 0.01f,
                    (startZ + z) * biome.terrainScale * 0.01f
                );

                float y = 0f;
                switch (biome.terrainType)
                {
                    case TerrainType.SmoothHills:
                        y = baseNoise * biome.heightMultiplier;
                        break;
                    case TerrainType.SharpMountains:
                        y = Mathf.Pow(baseNoise, 3f) * biome.heightMultiplier;
                        break;
                    case TerrainType.Plateaus:
                        y = Mathf.Round(baseNoise * 3f) / 3f * biome.heightMultiplier;
                        break;
                    case TerrainType.Craters:
                        y = (1f - Mathf.Abs(baseNoise * 2f - 1f)) * biome.heightMultiplier;
                        break;
                    case TerrainType.Dunes:
                        float dune = Mathf.PerlinNoise((startX + x) * biome.terrainScale * 0.05f, 0f);
                        y = dune * biome.heightMultiplier * 0.5f;
                        break;
                    case TerrainType.Islands:
                        float dist = Vector2.Distance(
                            new Vector2(startX + x, startZ + z),
                            new Vector2(biome.width / 2f, biome.height / 2f));
                        float gradient = Mathf.Clamp01(1f - dist / (biome.width / 2f));
                        y = baseNoise * biome.heightMultiplier * gradient;
                        break;
                    case TerrainType.Canyons:
                        float canyon = Mathf.Abs(Mathf.PerlinNoise((startX + x) * 0.05f, 0f) - 0.5f) * 2f;
                        y = baseNoise * biome.heightMultiplier * canyon;
                        break;
                    case TerrainType.FractalMountains:
                        y = RidgedNoise(startX + x, startZ + z,
                                        biome.terrainScale * 0.01f,
                                        biome.fractalOctaves,
                                        biome.fractalPersistence,
                                        biome.fractalLacunarity
                                        ) * biome.heightMultiplier;
                        break;
                }

                vertices[i] = new Vector3(startX + x, y, startZ + z);
            }
        }

        // треугольники
        for (int z = 0, vert = 0, tris = 0; z < height; z++, vert++)
        {
            for (int x = 0; x < width; x++, vert++, tris += 6)
            {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter mf = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();
        MeshCollider mc = chunkObj.AddComponent<MeshCollider>();

        mf.sharedMesh = mesh;
        if (biome.groundMaterial != null)
            mr.sharedMaterial = biome.groundMaterial;
        mc.sharedMesh = mesh;

        return chunkObj;
    }

    private float RidgedNoise(float x, float z, float scale, int octaves, float persistence, float lacunarity)
    {
        float total = 0f, frequency = 1f, amplitude = 1f, maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float n = Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency);
            n = 1f - Mathf.Abs(n * 2f - 1f);
            total += n * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return (maxValue > 0f) ? total / maxValue : 0f;
    }

    private void ApplyFogFromBiome()
    {
        if (biome.enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = biome.fogMode;
            RenderSettings.fogColor = biome.fogColor;

            if (biome.fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = biome.fogLinearStart;
                RenderSettings.fogEndDistance = biome.fogLinearEnd;
            }
            else
            {
                RenderSettings.fogDensity = biome.fogDensity;
            }

            Debug.Log($"🌫 Fog применён для биома '{biome.biomeName}'");
        }
        else
        {
            RenderSettings.fog = false;
        }
    }

    private IEnumerator LerpFog(Color targetColor, float targetDensity, float duration)
    {
        Color startColor = RenderSettings.fogColor;
        float startDensity = RenderSettings.fogDensity;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, targetDensity, t);

            yield return null;
        }

        RenderSettings.fogColor = targetColor;
        RenderSettings.fogDensity = targetDensity;
    }

    public void StartSandstorm(float duration = 5f)
    {
        if (fogRoutine != null) StopCoroutine(fogRoutine);
        fogRoutine = StartCoroutine(LerpFog(new Color(1f, 0.35f, 0.1f), 0.05f, duration));
    }

    public void EndSandstorm(float duration = 5f)
    {
        if (fogRoutine != null) StopCoroutine(fogRoutine);
        fogRoutine = StartCoroutine(LerpFog(biome.fogColor, biome.fogDensity, duration));
    }

    public void SpawnQuests()
    {
        if (biome.possibleQuests == null || biome.possibleQuests.Length == 0) return;

        foreach (var entry in biome.possibleQuests)
        {
            if (entry.questAsset == null || entry.questPointPrefab == null) continue;
            if (UnityEngine.Random.value > entry.spawnChance) continue;

            // ⚡ сброс прогресса перед генерацией целей
            entry.questAsset.ResetProgress();

            int targetsCount = UnityEngine.Random.Range(entry.minTargets, entry.maxTargets + 1);

            for (int i = 0; i < targetsCount; i++)
            {
                Vector3 pos = new Vector3(
                    UnityEngine.Random.Range(0f, biome.width),
                    1000f,
                    UnityEngine.Random.Range(0f, biome.height)
                );

                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 2000f))
                    pos = hit.point + Vector3.up * 0.5f;
                else
                    pos.y = 0f;

                GameObject pointObj = Instantiate(entry.questPointPrefab, pos, Quaternion.identity, biomeRoot.transform);

                QuestPoint qp = pointObj.GetComponent<QuestPoint>();
                if (qp != null)
                {
                    qp.linkedQuest = entry.questAsset;

                    // передаём трансформ в поведение
                    if (qp.linkedQuest.behaviour is ApproachPointQuestBehaviour approach)
                        approach.targetPoint = qp.transform;
                    else if (qp.linkedQuest.behaviour is StandOnPointQuestBehaviour stand)
                        stand.targetPoint = qp.transform;
                }
            }

            

            Debug.Log($"✅ Сгенерирован квест '{entry.questAsset.questName}' с {targetsCount} целями");
        }
    }

    private IEnumerator SpawnEnvironmentDelayed()
    {
        // ждём один кадр, чтобы коллайдеры чанков успели обновиться
        yield return new WaitForEndOfFrame();
        SpawnEnvironment();
    }

    private void SpawnEnvironment()
    {
        if (biome.environmentPrefabs == null || biome.environmentPrefabs.Length == 0)
        {
            Debug.Log($"⚠️ У биома '{biome.biomeName}' нет environmentPrefabs.");
            return;
        }

        int totalCount = Mathf.RoundToInt(biome.width * biome.height * biome.environmentDensity);
        int spawned = 0;

        for (int i = 0; i < totalCount; i++)
        {
            EnvironmentEntry entry = GetWeightedRandomEntry(biome.environmentPrefabs);
            if (entry == null || entry.prefab == null)
                continue;

            // 🎯 проверяем индивидуальный шанс спавна
            if (Random.value > entry.spawnChance)
                continue;

            Vector3 pos = new Vector3(
                Random.Range(0f, biome.width),
                1000f,
                Random.Range(0f, biome.height)
            );

            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 2000f))
            {
                Vector3 normal = hit.normal;
                float slope = Vector3.Angle(normal, Vector3.up);
                if (slope > 55f) continue;

                pos = hit.point;
                pos.y -= 0.15f;

                Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                float scale = Random.Range(0.9f, 1.1f);

                float heightFactor = Mathf.InverseLerp(0f, biome.heightMultiplier * 0.8f, hit.point.y);
                float probability = Mathf.Lerp(1f, 0.4f, heightFactor);
                if (Random.value > probability)
                    continue;

                GameObject obj = Instantiate(entry.prefab, pos, rot, biomeRoot.transform);
                obj.transform.localScale *= scale;
                obj.transform.up = Vector3.Lerp(obj.transform.up, normal, 0.4f);

                spawned++;
            }
        }

        Debug.Log($"🌿 Окружение '{biome.biomeName}': {spawned}/{totalCount} объектов заспавнено.");
    }

    private EnvironmentEntry GetWeightedRandomEntry(EnvironmentEntry[] entries)
    {
        float totalWeight = 0f;
        foreach (var e in entries)
            totalWeight += Mathf.Max(0.01f, e.weight);

        float r = Random.Range(0f, totalWeight);
        float sum = 0f;
        foreach (var e in entries)
        {
            sum += Mathf.Max(0.01f, e.weight);
            if (r <= sum)
                return e;
        }
        return entries.Length > 0 ? entries[0] : null;
    }


}
