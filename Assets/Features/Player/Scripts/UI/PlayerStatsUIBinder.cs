// Assets/Features/Player/Scripts/UI/PlayerStatsUIBinder.cs
using UnityEngine;
using Features.Player.UnityIntegration;
using Features.Stats.UnityIntegration;

namespace Features.Player.UI
{
    /// <summary>
    /// Чистый UI-адаптер, который слушает OnStatsReady и
    /// привязывает HP/Energy к PlayerRegistry.
    /// </summary>
    public class PlayerStatsUIBinder : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private EnergyBarUI energyUI;
        [SerializeField] private HPBarUI hpUI;

        private void OnEnable()
        {
            PlayerStats.OnStatsReady += HandleStatsReady;
        }

        private void OnDisable()
        {
            PlayerStats.OnStatsReady -= HandleStatsReady;
        }

        private void Start()
        {
            // на случай, если к этому моменту статы уже готовы
            var reg = PlayerRegistry.Instance;
            if (reg != null && reg.LocalEnergy != null && reg.LocalHealth != null)
            {
                BindUI(reg);
            }
        }

        private void HandleStatsReady(PlayerStats _)
        {
            var reg = PlayerRegistry.Instance;
            if (reg == null)
            {
                Debug.LogError("[PlayerStatsUIBinder] PlayerRegistry.Instance == null");
                return;
            }

            if (reg.LocalEnergy == null || reg.LocalHealth == null)
            {
                Debug.LogError("[PlayerStatsUIBinder] LocalEnergy/LocalHealth == null!");
                return;
            }

            BindUI(reg);
        }

        private void BindUI(PlayerRegistry reg)
        {
            if (energyUI != null)
                energyUI.Bind(reg.LocalEnergy);

            if (hpUI != null)
                hpUI.Bind(reg.LocalHealth);
        }
    }
}
