using UnityEngine;
using Biomes.Data;
using Biomes.UnityIntegration;

public class PlayerBiomeTracker : MonoBehaviour
{
    [Header("References")]
    public BiomeUIController ui;
    public WorldConfig world;

    [Header("Settings")]
    [Tooltip("Как часто проверять биом (сек)")]
    public float checkInterval = 0.25f;

    [Tooltip("Минимальное время между срабатыванием смены биома")]
    public float biomeChangeCooldown = 1.5f;

    private float checkTimer;
    private float cooldownTimer;

    private BiomeConfig lastBiome;

    private void Start()
    {
        if (ui == null)
            ui = Object.FindAnyObjectByType<BiomeUIController>();

        checkTimer = checkInterval;
        cooldownTimer = 0f;
    }

    private void Update()
    {
        checkTimer -= Time.deltaTime;
        cooldownTimer -= Time.deltaTime;

        if (checkTimer > 0)
            return;

        checkTimer = checkInterval;

        UpdateBiome();
    }

    private void UpdateBiome()
    {
        var activeWorld = RuntimeWorldGenerator.World != null
            ? RuntimeWorldGenerator.World
            : world;

        if (activeWorld == null)
            return;

        Vector3 pos = transform.position;

        Vector2Int chunkPos = new Vector2Int(
            Mathf.FloorToInt(pos.x / activeWorld.chunkSize),
            Mathf.FloorToInt(pos.z / activeWorld.chunkSize)
        );

        BiomeConfig biome = activeWorld.GetBiomeAtChunk(chunkPos);
        if (biome == null)
            return;

        ui.UpdateFogGradient(biome.fogLightColor, biome.fogHeavyColor, biome.fogGradientScale);

        if (biome == lastBiome || cooldownTimer > 0f)
            return;

        lastBiome = biome;
        cooldownTimer = biomeChangeCooldown;

        ui.SetBiome(biome.name, biome.uiColor);
        ui.ShowPopup(biome.name, biome.uiColor);
    }
}
