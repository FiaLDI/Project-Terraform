using System;
using Biomes.Data;
using Features.Player.UnityIntegration;
using UnityEngine;

namespace Biomes.UnityIntegration
{
    public class BiomeFog : MonoBehaviour
    {
        public float blendSpeed = 0.25f;

        private Transform player;
        private WorldConfig world;

        private bool defaultFogEnabled;
        private FogMode defaultFogMode;
        private Color defaultFogColor;
        private float defaultFogDensity;
        private float defaultFogStart;
        private float defaultFogEnd;

        private Color fogColor;
        private float fogDensity;
        private float fogStart;
        private float fogEnd;

        private void OnEnable()
        {
            PlayerRegistry.SubscribeLocalPlayerReady(OnPlayerReady);

            if (PlayerRegistry.Instance != null && PlayerRegistry.Instance.LocalPlayer != null)
                OnPlayerReady(PlayerRegistry.Instance);
        }

        private void OnDisable()
        {
            PlayerRegistry.UnsubscribeLocalPlayerReady(OnPlayerReady);
            RestoreDefaults();
        }

        private void OnPlayerReady(PlayerRegistry reg)
        {
            player = reg.LocalPlayer.transform;
        }

        private void Start()
        {
            defaultFogEnabled = RenderSettings.fog;
            defaultFogMode = RenderSettings.fogMode;
            defaultFogColor = RenderSettings.fogColor;
            defaultFogDensity = RenderSettings.fogDensity;
            defaultFogStart = RenderSettings.fogStartDistance;
            defaultFogEnd = RenderSettings.fogEndDistance;

            ResetBlendState();
        }

        private void LateUpdate()
        {
            var activeWorld = RuntimeWorldGenerator.World;
            if (activeWorld != world)
                BindWorld(activeWorld);

            if (world == null || player == null)
                return;

            var blends = world.GetBiomeBlend(new Vector3(player.position.x, 0f, player.position.z));
            if (blends == null || blends.Length == 0)
                return;

            Color c = Color.black;
            float d = 0f;
            float s = 0f;
            float e = 0f;
            float w = 0f;
            bool hasFog = false;
            FogMode targetMode = defaultFogMode;
            float strongestWeight = -1f;

            foreach (var b in blends)
            {
                if (b.biome == null || !b.biome.enableFog)
                    continue;

                hasFog = true;
                c += b.biome.fogColor * b.weight;
                d += b.biome.fogDensity * b.weight;
                s += b.biome.fogLinearStart * b.weight;
                e += b.biome.fogLinearEnd * b.weight;
                w += b.weight;

                if (b.weight > strongestWeight)
                {
                    strongestWeight = b.weight;
                    targetMode = b.biome.fogMode;
                }
            }

            Color targetColor = defaultFogColor;
            float targetDensity = defaultFogDensity;
            float targetStart = defaultFogStart;
            float targetEnd = defaultFogEnd;

            if (hasFog && w > 0f)
            {
                targetColor = c / w;
                targetDensity = d / w;
                targetStart = s / w;
                targetEnd = e / w;
            }

            float t = Time.deltaTime * blendSpeed * 60f;

            fogColor = Color.Lerp(fogColor, targetColor, t);
            fogDensity = Mathf.Lerp(fogDensity, targetDensity, t);
            fogStart = Mathf.Lerp(fogStart, targetStart, t);
            fogEnd = Mathf.Lerp(fogEnd, targetEnd, t);

            RenderSettings.fog = hasFog || defaultFogEnabled;
            RenderSettings.fogMode = hasFog ? targetMode : defaultFogMode;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }

        internal float GetFogFactor()
        {
            throw new NotImplementedException();
        }

        private void BindWorld(WorldConfig activeWorld)
        {
            world = activeWorld;
            ResetBlendState();
        }

        private void ResetBlendState()
        {
            fogColor = defaultFogColor;
            fogDensity = defaultFogDensity;
            fogStart = defaultFogStart;
            fogEnd = defaultFogEnd;
        }

        private void RestoreDefaults()
        {
            RenderSettings.fog = defaultFogEnabled;
            RenderSettings.fogMode = defaultFogMode;
            RenderSettings.fogColor = defaultFogColor;
            RenderSettings.fogDensity = defaultFogDensity;
            RenderSettings.fogStartDistance = defaultFogStart;
            RenderSettings.fogEndDistance = defaultFogEnd;
        }
    }
}
