using Features.Input;
using Features.Player.UI;
using UnityEngine;

namespace Features.Stats.UnityIntegration
{
    public class StatsScreen : MonoBehaviour, IUIScreen
    {
        [SerializeField] private GameObject statsPanel;

        private StatsDebugStreamer streamer;

        public InputMode Mode => InputMode.Inventory;

        private void Awake()
        {
            if (statsPanel != null)
                statsPanel.SetActive(false);
            
            UIRegistry.I?.Register(this);

            var player = GetComponentInParent<PlayerUIRoot>().BoundPlayer;
            if (player != null)
                streamer = player.GetComponent<StatsDebugStreamer>();
        }

        private void OnDestroy()
        {
            UIRegistry.I?.Unregister(this);
        }

        // =========================
        // IUIScreen
        // =========================

        public void Show()
        {
            if (statsPanel != null)
                statsPanel.SetActive(true);

            streamer?.StartStreaming();
        }

        public void Hide()
        {
            if (statsPanel != null)
                statsPanel.SetActive(false);

            streamer?.StopStreaming();
        }

        // =========================
        // PUBLIC
        // =========================

        public void Open()
        {
            UIStackManager.I.Push(this);
        }
    }
}
