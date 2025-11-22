using UnityEngine;

public class AdvancedWaterPlane : MonoBehaviour
{
    public float fadeSpeed = 1.5f;

    private Renderer rend;
    private Material currentMaterial;
    private float currentAlpha = 0f;

    private Transform player;

    private void Start()
    {
        rend = GetComponent<Renderer>();

        if (RuntimeWorldGenerator.PlayerInstance != null)
            player = RuntimeWorldGenerator.PlayerInstance.transform;

        UpdateWaterMaterial();
        ApplyAlphaInstant(0f);
    }

    private void Update()
    {
        if (player == null)
        {
            if (RuntimeWorldGenerator.PlayerInstance != null)
                player = RuntimeWorldGenerator.PlayerInstance.transform;
            else
                return;
        }

        // 1) выбираем материал под биом
        bool waterEnabled = UpdateWaterMaterial();

        // 2) плавное появление/исчезновение
        if (waterEnabled)
            FadeIn();
        else
            FadeOut();

        // 3) подгоняем высоту воды
        UpdateHeight();
    }

    // --------------------------------------------------------------
    //                ВЫБОР МАТЕРИАЛА ПО БИОМУ
    // --------------------------------------------------------------
    private bool UpdateWaterMaterial()
    {
        var world = RuntimeWorldGenerator.World;
        if (world == null) return false;

        var pos = player.position;
        var biomeBlend = world.GetBiomeBlend(pos);

        BiomeConfig dominant = null;
        float best = 0f;

        foreach (var b in biomeBlend)
        {
            if (b.weight > best)
            {
                best = b.weight;
                dominant = b.biome;
            }
        }

        if (dominant == null || !dominant.useWater)
            return false;

        // выбираем материал
        Material target = dominant.waterMaterial;

        if (dominant.waterType == WaterType.Ocean && dominant.oceanWaterMaterial != null)
            target = dominant.oceanWaterMaterial;

        if (dominant.waterType == WaterType.Lake && dominant.lakeWaterMaterial != null)
            target = dominant.lakeWaterMaterial;

        if (dominant.waterType == WaterType.Swamp && dominant.swampWaterMaterial != null)
            target = dominant.swampWaterMaterial;

        if (currentMaterial != target)
        {
            rend.sharedMaterial = target;
            currentMaterial = target;
        }

        return true;
    }

    // --------------------------------------------------------------
    //                ПЛАВНОЕ ПОЯВЛЕНИЕ / ИСЧЕЗНОВЕНИЕ
    // --------------------------------------------------------------
    private void FadeIn()
    {
        currentAlpha = Mathf.MoveTowards(currentAlpha, 1f, Time.deltaTime * fadeSpeed);
        ApplyAlpha(currentAlpha);
    }

    private void FadeOut()
    {
        currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, Time.deltaTime * fadeSpeed);
        ApplyAlpha(currentAlpha);
    }

    private void ApplyAlphaInstant(float a)
    {
        currentAlpha = a;
        ApplyAlpha(a);
    }

    private void ApplyAlpha(float a)
    {
        if (currentMaterial == null) return;

        currentMaterial.SetFloat("_Alpha", a);
    }

    // --------------------------------------------------------------
    //                УРОВЕНЬ ВОДЫ
    // --------------------------------------------------------------
    private void UpdateHeight()
    {
        var world = RuntimeWorldGenerator.World;
        if (world == null) return;

        var pos = player.position;
        var blend = world.GetBiomeBlend(pos);

        float sea = float.MaxValue;

        foreach (var b in blend)
        {
            if (b.biome.useWater)
                sea = Mathf.Min(sea, b.biome.seaLevel);
        }

        if (sea == float.MaxValue)
            return;

        transform.position = new Vector3(transform.position.x, sea, transform.position.z);
    }
}
