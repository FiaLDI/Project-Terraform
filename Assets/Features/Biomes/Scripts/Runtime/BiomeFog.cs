using UnityEngine;

public class BiomeFog : MonoBehaviour
{
    public BiomeConfig biome;

    void Start()
    {
        if (biome != null)
        {
            ApplyBiomeFog(biome);
        }
    }

    public void ApplyBiomeFog(BiomeConfig config)
    {
        RenderSettings.fog = config.enableFog;
        RenderSettings.fogMode = config.fogMode;
        RenderSettings.fogColor = config.fogColor;

        if (config.fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = config.fogLinearStart;
            RenderSettings.fogEndDistance = config.fogLinearEnd;
        }
        else
        {
            RenderSettings.fogDensity = config.fogDensity;
        }
    }

    // Дополнительно: переключение между состояниями
    public void StartSandstorm()
    {
        RenderSettings.fogColor = new Color(1f, 0.35f, 0.1f);
        RenderSettings.fogDensity = 0.05f;
    }

    public void EndSandstorm()
    {
        if (biome != null)
            ApplyBiomeFog(biome);
    }
}
