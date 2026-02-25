using UnityEngine;
using TMPro;
using Features.Input;
using Features.UI;
using FishNet.Object;

namespace Features.World.UI
{
    public sealed class WorldGeneratorUI : PlayerBoundUIView, IUIScreen
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_InputField seedInput;
        private GameObject boundPlayer;

        public InputMode Mode => InputMode.Dialog;

        protected override void OnEnable()
        {
            base.OnEnable();
            UIRegistry.I?.Register(this);
            root.SetActive(false);
        }

        protected override void OnDisable()
        {
            UIRegistry.I?.Unregister(this);
            base.OnDisable();
        }

        protected override void OnPlayerBound(GameObject player)
        {
            if (player == null)
            {
                Debug.Log("[StatsUIRoot] OnPlayerBound: NULL (Unbind)", this);

                boundPlayer = null;
                return;
            }

            Debug.Log($"[StatsUIRoot] OnPlayerBound: {player.name}", this);

            boundPlayer = player;
            root.SetActive(false);
        }

        public void Show()
        {
            root.SetActive(true);
            InputModeManager.I.SetMode(Mode);
        }

        public void Hide()
        {
            root.SetActive(false);
            InputModeManager.I.SetMode(InputMode.Gameplay);
        }

        public void Open()
        {
            UIStackManager.I.Push(this);
        }

        public void OnRandomSeedClicked()
        {
            int randomSeed = Random.Range(int.MinValue, int.MaxValue);
            seedInput.text = randomSeed.ToString();
        }

        public void OnGenerateWorldClicked()
        {
            if (!int.TryParse(seedInput.text, out int seed))
                return;

            var net = boundPlayer.GetComponent<PlayerNetworkController>();
            net.RequestWorldServerRpc(seed);

            UIStackManager.I?.Clear();
        }

        public void OnCloseClicked()
        {
            UIStackManager.I.Pop();
        }
    }
}