using UnityEngine;
using TMPro;
using Features.Input;
using Features.UI;
using FishNet.Object;
using System.Collections.Generic;
using System.Linq;
using Features.Quests.Data;

namespace Features.World.UI
{
    public sealed class WorldGeneratorUI : PlayerBoundUIView, IUIScreen
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_InputField seedInput;

        [Header("Available quests")]
        [SerializeField] private QuestAsset[] availableQuests;

        [Header("Available chains")]
        [SerializeField] private QuestChainAsset[] availableChains;

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
            if (BoundPlayer == null)
            {
                Debug.LogError("WorldGeneratorUI: BoundPlayer is null");
                return;
            }
            if (!int.TryParse(seedInput.text, out int seed))
                return;

            var questIds = availableQuests
                .Where(q => q != null)
                .Select(q => q.questId)
                .ToList();

            var chainIds = availableChains
                .Where(c => c != null)
                .Select(c => c.chainId)
                .ToList();

            var net = BoundPlayer.GetComponent<PlayerNetworkController>();

            net.RequestWorldServerRpc(seed, questIds, chainIds);

            UIStackManager.I?.Clear();
        }

        public void OnCloseClicked()
        {
            UIStackManager.I.Pop();
        }
    }
}
