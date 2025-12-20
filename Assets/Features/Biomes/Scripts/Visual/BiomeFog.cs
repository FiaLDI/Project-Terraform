using UnityEngine;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;
using Features.Player.UnityIntegration;

namespace Features.Biomes.Runtime.Visual
{
    [DefaultExecutionOrder(50)]
    public class BiomeFog : MonoBehaviour
    {
        [Range(0.0f, 2.0f)]
        public float blendSpeed = 0.25f;

        private Color fogColor;
        private float fogDensity;
        private float fogStart;
        private float fogEnd;

        private bool initialized;

        private void Start()
        {
            if (RuntimeWorldGenerator.World == null)
            {
                enabled = false;
                return;
            }

            RenderSettings.fog = true;

            fogColor = RenderSettings.fogColor;
            fogDensity = RenderSettings.fogDensity;
            fogStart = RenderSettings.fogStartDistance;
            fogEnd = RenderSettings.fogEndDistance;

            initialized = true;
        }

        private void LateUpdate()
        {
            if (!initialized)
                return;

            var registry = PlayerRegistry.Instance;
            if (registry == null || registry.LocalPlayer == null)
                return;

            Vector3 pos = registry.LocalPlayer.transform.position;

            var blends = RuntimeWorldGenerator.World.GetBiomeBlend(
                new Vector3(pos.x, 0f, pos.z)
            );

            if (blends == null || blends.Length == 0)
                return;

            // ------------------------
            // 1) ГЛАВНЫЙ БИОМ
            // ------------------------
            BiomeConfig mainBiome = null;
            float mainWeight = 0f;

            foreach (var b in blends)
            {
                if (b.biome == null)
                    continue;

                if (b.weight > mainWeight)
                {
                    mainBiome = b.biome;
                    mainWeight = b.weight;
                }
            }

            if (mainBiome == null)
                return;

            RenderSettings.fogMode = mainBiome.fogMode;

            // ------------------------
            // 2) BLEND БИОМОВ
            // ------------------------
            Color blendedColor = Color.black;
            float blendedDensity = 0f;
            float blendedStart = 0f;
            float blendedEnd = 0f;
            float totalW = 0f;

            foreach (var b in blends)
            {
                if (b.biome == null || !b.biome.enableFog)
                    continue;

                blendedColor += b.biome.fogColor * b.weight;
                blendedDensity += b.biome.fogDensity * b.weight;
                blendedStart += b.biome.fogLinearStart * b.weight;
                blendedEnd += b.biome.fogLinearEnd * b.weight;

                totalW += b.weight;
            }

            if (totalW > 0f)
            {
                blendedColor /= totalW;
                blendedDensity /= totalW;
                blendedStart /= totalW;
                blendedEnd /= totalW;
            }

            // ------------------------
            // 3) ПЛАВНЫЙ ПЕРЕХОД
            // ------------------------
            float t = Time.deltaTime * blendSpeed * 60f;

            fogColor = Color.Lerp(fogColor, blendedColor, t);
            fogDensity = Mathf.Lerp(fogDensity, blendedDensity, t);
            fogStart = Mathf.Lerp(fogStart, blendedStart, t);
            fogEnd = Mathf.Lerp(fogEnd, blendedEnd, t);

            // ------------------------
            // 4) ПРИМЕНЕНИЕ
            // ------------------------
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }

        public float GetFogFactor() => fogDensity;
    }
}
