using UnityEngine;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;

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

        private bool initialized = false;

        private void Start()
        {
            if (RuntimeWorldGenerator.World == null)
            {
                Debug.LogWarning("[BiomeFog] No world config");
                enabled = false;
                return;
            }

            RenderSettings.fog = true;

            fogColor   = RenderSettings.fogColor;
            fogDensity = RenderSettings.fogDensity;
            fogStart   = RenderSettings.fogStartDistance;
            fogEnd     = RenderSettings.fogEndDistance;

            initialized = true;
        }

        private void LateUpdate()
        {
            if (!initialized) return;
            if (RuntimeWorldGenerator.PlayerInstance == null) return;

            Vector3 pos = RuntimeWorldGenerator.PlayerInstance.transform.position;
            var blends = RuntimeWorldGenerator.World.GetBiomeBlend(new Vector3(pos.x, 0, pos.z));

            if (blends == null || blends.Length == 0)
                return;

            BiomeConfig biome = blends[0].biome;
            float weight = blends[0].weight;

            if (biome == null || !biome.enableFog)
                return;

            RenderSettings.fog = true;

            Color  tColor   = biome.fogColor;
            float  tDensity = biome.fogDensity * weight;
            float  tStart   = biome.fogLinearStart;
            float  tEnd     = biome.fogLinearEnd;

            float t = Time.deltaTime * blendSpeed * 60f;

            fogColor   = Color.Lerp(fogColor, tColor, t);
            fogDensity = Mathf.Lerp(fogDensity, tDensity, t);
            fogStart   = Mathf.Lerp(fogStart, tStart, t);
            fogEnd     = Mathf.Lerp(fogEnd, tEnd, t);

            RenderSettings.fogColor         = fogColor;
            RenderSettings.fogDensity       = fogDensity;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance   = fogEnd;
            RenderSettings.fogMode          = biome.fogMode;
        }

        public float GetFogFactor()
        {
            return fogDensity;
        }
    }
}
