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
        }

        private void OnPlayerReady(PlayerRegistry reg)
        {
            player = reg.LocalPlayer.transform;
        }

        private void Start()
        {
            RenderSettings.fog = true;

            fogColor = RenderSettings.fogColor;
            fogDensity = RenderSettings.fogDensity;
            fogStart = RenderSettings.fogStartDistance;
            fogEnd = RenderSettings.fogEndDistance;
        }

        private void LateUpdate()
        {
            if (world == null)
                world = RuntimeWorldGenerator.World;

            if (world == null || player == null)
                return;

            var blends = world.GetBiomeBlend(new Vector3(player.position.x, 0f, player.position.z));
            if (blends == null || blends.Length == 0)
                return;

            Color c = Color.black;
            float d = 0, s = 0, e = 0, w = 0;

            foreach (var b in blends)
            {
                if (b.biome == null || !b.biome.enableFog)
                    continue;

                c += b.biome.fogColor * b.weight;
                d += b.biome.fogDensity * b.weight;
                s += b.biome.fogLinearStart * b.weight;
                e += b.biome.fogLinearEnd * b.weight;
                w += b.weight;
            }

            if (w > 0)
            {
                c /= w;
                d /= w;
                s /= w;
                e /= w;
            }

            float t = Time.deltaTime * blendSpeed * 60f;

            fogColor = Color.Lerp(fogColor, c, t);
            fogDensity = Mathf.Lerp(fogDensity, d, t);
            fogStart = Mathf.Lerp(fogStart, s, t);
            fogEnd = Mathf.Lerp(fogEnd, e, t);

            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }

        internal float GetFogFactor()
        {
            throw new NotImplementedException();
        }
    }
}
