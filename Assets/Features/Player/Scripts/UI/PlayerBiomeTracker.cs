using UnityEngine;
using Features.Biomes.Runtime.Visual;
using Features.Biomes.Domain;

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
        Vector3 pos = transform.position;

        Vector2Int chunkPos = new Vector2Int(
            Mathf.FloorToInt(pos.x / world.chunkSize),
            Mathf.FloorToInt(pos.z / world.chunkSize)
        );

        BiomeConfig biome = world.GetBiomeAtChunk(chunkPos);
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
