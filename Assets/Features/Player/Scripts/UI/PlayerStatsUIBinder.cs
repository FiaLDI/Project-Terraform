using UnityEngine;
using Features.Player.UnityIntegration;
using Features.Stats.Adapter;

namespace Features.Player.UI
{
    public sealed class PlayerStatsUIBinder : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private EnergyBarUI energyUI;
        [SerializeField] private HPBarUI hpUI;

        private void OnEnable()
        {
            PlayerRegistry.OnLocalPlayerReady += OnLocalPlayerReady;

            // если локальный игрок уже есть
            var reg = PlayerRegistry.Instance;
            if (reg != null && reg.LocalPlayer != null)
            {
                TryBind(reg);
            }
        }

        private void OnDisable()
        {
            PlayerRegistry.OnLocalPlayerReady -= OnLocalPlayerReady;
        }

        private void OnLocalPlayerReady(PlayerRegistry reg)
        {
            TryBind(reg);
        }

        private void TryBind(PlayerRegistry reg)
        {
            if (reg == null || reg.LocalPlayer == null)
            {
                Debug.LogWarning("[PlayerStatsUIBinder] No local player yet");
                return;
            }

            var statsAdapter = reg.LocalPlayer.GetComponent<StatsFacadeAdapter>();
            if (statsAdapter == null)
            {
                Debug.LogError("[PlayerStatsUIBinder] StatsFacadeAdapter not found on LocalPlayer");
                return;
            }

            Debug.Log("[PlayerStatsUIBinder] Binding UI to local player stats");

            if (energyUI != null && statsAdapter.EnergyStats != null)
                energyUI.Bind(statsAdapter.EnergyStats);

            if (hpUI != null && statsAdapter.HealthStats != null)
                hpUI.Bind(statsAdapter.HealthStats);
        }
    }
}
