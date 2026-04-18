using UnityEngine;
using Features.Player.UnityIntegration;
using Features.Enemy.Data;
using Features.Enemy.Integration.LOD;
using Biomes.Application;

namespace Features.Enemy.Presentation.LOD
{
    [RequireComponent(typeof(EnemyLODView))]
    public class EnemyDistanceLODSystem : MonoBehaviour
    {
        [SerializeField] private EnemyConfigSO config;

        private EnemyLODView view;
        private EnemyInstancingController instancing;
        private EnemyLogicLODAdapter logic;

        private int currentLOD = -1;

        private float nextUpdate;
        private const float Interval = 0.08f;
        public bool OffLod = false;

        private void Awake()
        {
            view = GetComponent<EnemyLODView>();
            instancing = GetComponent<EnemyInstancingController>();
            logic = GetComponent<EnemyLogicLODAdapter>();

            if (!HasValidLODConfig())
                enabled = false;

            if (config != null)
            {
                view.Init(
                    config.render.lod0Prefab,
                    config.render.lod1Prefab,
                    config.render.lod2Prefab
                );
            }
        }

        private bool HasValidLODConfig()
        {
            return config != null &&
                config.render.lod0Prefab != null &&
                config.render.lod1Prefab != null &&
                config.render.lod2Prefab != null &&
                !OffLod;
        }

        private void Update()
        {
            if (config == null) return;
            if (Time.time < nextUpdate) return;
            if (!HasValidLODConfig())
                return;

            nextUpdate = Time.time + Interval;

            var player = PlayerRegistry.Instance?.LocalPlayer;
            if (player == null) return;

            float dist = Vector3.Distance(player.transform.position, transform.position);
            float lodScale = EnemyPerformanceManager.Instance != null
                ? Mathf.Max(0.25f, EnemyPerformanceManager.Instance.LodScale)
                : 1f;

            float lod0Distance = config.render.lod0Distance * lodScale;
            float lod1Distance = config.render.lod1Distance * lodScale;
            float instancingDistance = config.render.instancingDistance * lodScale;

            bool useInstancing =
                config.render.useGPUInstancing &&
                dist > instancingDistance;

            if (useInstancing)
            {
                instancing?.EnableInstancing();
                return;
            }

            instancing?.DisableInstancing();

            int lod =
                dist <= lod0Distance ? 0 :
                dist <= lod1Distance ? 1 : 2;

            if (lod == currentLOD)
                return;

            currentLOD = lod;

            view.SetLOD(lod);
            logic?.ApplyLOD(lod);
        }
    }
}
