using UnityEngine;
using Unity.Mathematics;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;

namespace Features.Biomes.Runtime.Visual
{
    [DefaultExecutionOrder(200)]
    public class BiomeFog : MonoBehaviour
    {
        [Header("Interpolation")]
        [Range(0.0f, 2.0f)]
        public float blendSpeed = 0.2f;

        private Color currentColor;
        private float currentDensity;
        private float currentStart;
        private float currentEnd;

        private bool initialized = false;

        private void Start()
        {
            if (RuntimeWorldGenerator.World == null)
            {
                Debug.LogWarning("[BiomeFog] RuntimeWorldGenerator not initialized.");
                enabled = false;
                return;
            }

            initialized = true;
        }

        private void LateUpdate()
        {
            if (!initialized) return;
            if (RuntimeWorldGenerator.PlayerInstance == null) return;

            // Fog must be enabled EVERY FRAME
            RenderSettings.fog = true;

            Vector3 pos = RuntimeWorldGenerator.PlayerInstance.transform.position;

            // Use the REAL GetBiomeBlend (array)
            var blends = RuntimeWorldGenerator.World.GetBiomeBlend(new Vector3(pos.x, 0, pos.z));
            if (blends == null || blends.Length == 0)
                return;

            BiomeConfig biome = blends[0].biome;
            if (biome == null || !biome.enableFog)
                return;

            Color tColor = biome.fogColor;
            float tDensity = biome.fogDensity;
            float tStart = biome.fogLinearStart;
            float tEnd = biome.fogLinearEnd;

            float f = blendSpeed * Time.deltaTime * 60f;

            currentColor = Color.Lerp(currentColor, tColor, f);
            currentDensity = Mathf.Lerp(currentDensity, tDensity, f);
            currentStart = Mathf.Lerp(currentStart, tStart, f);
            currentEnd = Mathf.Lerp(currentEnd, tEnd, f);

            RenderSettings.fogMode = biome.fogMode;
            RenderSettings.fogColor = currentColor;
            RenderSettings.fogDensity = currentDensity;
            RenderSettings.fogStartDistance = currentStart;
            RenderSettings.fogEndDistance = currentEnd;
        }
    }
}
