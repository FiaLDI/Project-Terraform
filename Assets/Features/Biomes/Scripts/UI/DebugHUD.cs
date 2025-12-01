using UnityEngine;
using TMPro;
using System.Linq;
using Features.Biomes.Domain;
using Features.Enemies;
using Features.Biomes.UnityIntegration;

public class DebugHUD : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Vector2 screenOffset = new Vector2(20, 20);

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (label == null)
        {
            GameObject go = new GameObject("DebugHUD_Label");
            go.transform.SetParent(FindAnyObjectByType<Canvas>().transform, false);

            label = go.AddComponent<TextMeshProUGUI>();
            label.fontSize = 22;
            label.color = new Color(1, 1, 1, 0.9f);
        }
    }

    void Update()
    {
        if (label == null) return;

        // FPS
        float fps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

        // Performance Scaling
        float lodScale = EnemyPerformanceManager.Instance != null
            ? EnemyPerformanceManager.Instance.LodScale
            : 1f;

        float countScale = EnemyPerformanceManager.Instance != null
            ? EnemyPerformanceManager.Instance.EnemyCountScale
            : 1f;

        // WORLD enemy count
        int worldEnemies = EnemyWorldManager.Instance != null
            ? EnemyWorldManager.Instance.GetCount()
            : 0;

        // BIOME detection
        BiomeConfig biome = null;
        int biomeEnemies = 0;

        if (RuntimeWorldGenerator.PlayerInstance != null &&
            RuntimeWorldGenerator.World != null)
        {
            var pos = RuntimeWorldGenerator.PlayerInstance.transform.position;
            var blend = RuntimeWorldGenerator.World.GetBiomeBlend(pos);
            float best = 0f;

            foreach (var b in blend)
            {
                if (b.biome != null && b.weight > best)
                {
                    biome = b.biome;
                    best = b.weight;
                }
            }

            if (biome != null)
                biomeEnemies = EnemyBiomeCounter.GetCount(biome);
        }

        // Find nearest enemy definition
        float nearestDist = -1f;
        string nearestName = "-";

        var enemies = FindObjectsByType<EnemyDefinition>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (enemies.Length > 0 && RuntimeWorldGenerator.PlayerInstance != null)
        {
            Vector3 playerPos = RuntimeWorldGenerator.PlayerInstance.transform.position;

            var nearest = enemies
                .OrderBy(e => Vector3.Distance(playerPos, e.transform.position))
                .FirstOrDefault();

            if (nearest != null)
            {
                nearestDist = Vector3.Distance(playerPos, nearest.transform.position);
                nearestName = nearest.displayName != "" ? nearest.displayName : nearest.name;
            }
        }

        // Build HUD text
        label.text =
            $"<b>DEBUG HUD</b>\n" +
            $"FPS: <b>{fps:0}</b>\n" +
            $"LOD Scale: <b>{lodScale:0.00}</b>\n" +
            $"Enemy Count Scale: <b>{countScale:0.00}</b>\n" +
            $"World Enemies: <b>{worldEnemies}</b>\n" +
            $"Biome Enemies: <b>{biomeEnemies}</b>\n" +
            $"Nearest: <b>{nearestName}</b>\n" +
            $"Nearest Dist: <b>{nearestDist:0.0}</b>";

        label.rectTransform.anchoredPosition = screenOffset;
    }
}
