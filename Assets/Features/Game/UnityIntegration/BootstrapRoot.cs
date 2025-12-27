using UnityEngine;
using Features.Player.UnityIntegration;


namespace Features.Game
{
    public sealed class BootstrapRoot : MonoBehaviour
    {
        public static BootstrapRoot I { get; private set; }
        public GameObject LocalPlayer { get; private set; }

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            PlayerRegistry.SubscribeLocalPlayerReady(OnLocalPlayerReady);
        }

        private void OnDisable()
        {
            PlayerRegistry.UnsubscribeLocalPlayerReady(OnLocalPlayerReady);
        }

        private void OnLocalPlayerReady(PlayerRegistry reg)
        {
            if (reg.LocalPlayer != null)
                SetLocalPlayer(reg.LocalPlayer);
        }

        public void SetLocalPlayer(GameObject player)
        {
            LocalPlayer = player;
            Debug.Log($"[BootstrapRoot] LocalPlayer set to {player?.name}");
        }

        public void ClearLocalPlayer()
        {
            if (LocalPlayer == null)
                return;

            Debug.Log("[BootstrapRoot] LocalPlayer cleared");
            LocalPlayer = null;
        }
    }
}