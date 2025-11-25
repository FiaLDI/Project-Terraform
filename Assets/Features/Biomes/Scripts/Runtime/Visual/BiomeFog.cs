using UnityEngine;
using System.Collections;

public class BiomeFog : MonoBehaviour
{
    public float transitionDuration = 1.5f; // время плавного перехода

    private BiomeConfig currentBiome;
    private Coroutine transitionRoutine;

    void Update()
    {
        UpdateFogByBiome();
    }

    private void UpdateFogByBiome()
    {
        var player = RuntimeWorldGenerator.PlayerInstance;
        var world = RuntimeWorldGenerator.World;

        if (player == null || world == null)
            return;

        Vector3 pos = player.transform.position;

        // ==============================
        //  ВАЖНО: переводим в координаты CHUNK
        // ==============================
        Vector2Int chunk = new Vector2Int(
            Mathf.FloorToInt(pos.x / world.chunkSize),
            Mathf.FloorToInt(pos.z / world.chunkSize)
        );

        var dom = world.GetDominantBiome(chunk);

        if (dom.biome == null)
            return;

        if (dom.biome != currentBiome)
        {
            SwitchBiomeFog(dom.biome);
        }

        var ui = FindObjectOfType<BiomeUIController>();
        if (ui != null && currentBiome != null)
        {
            ui.UpdateFogGradient(
                currentBiome.fogLightColor,
                currentBiome.fogHeavyColor,
                currentBiome.fogGradientScale
            );
        }
    }

    private void SwitchBiomeFog(BiomeConfig newBiome)
    {
        currentBiome = newBiome;

        var ui = FindObjectOfType<BiomeUIController>();
        if (ui != null)
        {
            ui.SetBiome(newBiome.biomeName, newBiome.uiColor);
            ui.ShowPopup(newBiome.biomeName, newBiome.uiColor);
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(FogTransitionRoutine(newBiome));
    }

    private IEnumerator FogTransitionRoutine(BiomeConfig target)
    {
        if (target == null)
            yield break;

        if (!target.enableFog)
        {
            RenderSettings.fog = false;
            yield break;
        }

        RenderSettings.fog = true;

        // Сохраняем текущие параметры тумана
        Color startColor = RenderSettings.fogColor;
        float startDensity = RenderSettings.fogDensity;
        float startStartDist = RenderSettings.fogStartDistance;
        float startEndDist = RenderSettings.fogEndDistance;

        FogMode targetMode = target.fogMode;

        // Режим fogMode меняем сразу — Unity не умеет плавно
        RenderSettings.fogMode = targetMode;

        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;
            float k = Mathf.SmoothStep(0f, 1f, t);

            // цвет
            RenderSettings.fogColor = Color.Lerp(startColor, target.fogColor, k);

            // экспонента / линейный
            if (targetMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = Mathf.Lerp(startStartDist, target.fogLinearStart, k);
                RenderSettings.fogEndDistance = Mathf.Lerp(startEndDist, target.fogLinearEnd, k);
            }
            else
            {
                RenderSettings.fogDensity = Mathf.Lerp(startDensity, target.fogDensity, k);
            }

            yield return null;
        }

        ApplyBiomeFogInstant(target);
    }

    private void ApplyBiomeFogInstant(BiomeConfig config)
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

    // ЭФФЕКТЫ
    public void StartSandstorm(float density = 0.05f)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(1f, 0.35f, 0.1f);
        RenderSettings.fogDensity = density;
    }

    public void EndSandstorm()
    {
        if (currentBiome != null)
            SwitchBiomeFog(currentBiome);
    }

    [ContextMenu("DEBUG: Force Strong Fog")]
    public void ForceStrongFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = Color.red;
        RenderSettings.fogDensity = 0.5f;

        Debug.Log("✅ Fog Debug: Strong fog applied");
    }

}
