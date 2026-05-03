using Features.Inventory.UnityIntegration;
using Features.Items.Data;
using Features.Player;
using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CampaignShipUpgradeUI : PlayerBoundStationUI
{
    [SerializeField] private CampaignCatalogSO campaignCatalog;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text currentShipLevelLabel;
    [SerializeField] private TMP_Text nextShipLevelLabel;
    [SerializeField] private TMP_Text requirementsLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private TMP_Text upgradeButtonLabel;

    private InventoryManager inventory;

    protected override void OnPlayerBound(GameObject player)
    {
        inventory = LocalPlayerContext.Inventory != null
            ? LocalPlayerContext.Inventory
            : player.GetComponent<InventoryManager>();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
    }

    public override void Show()
    {
        base.Show();
        Refresh();
    }

    public override void ShowRecipe(RecipeSO recipe)
    {
    }

    public void Refresh()
    {
        if (titleLabel != null)
            titleLabel.text = "Ship Upgrade";

        CampaignProgressService campaign = CampaignProgressService.EnsureExists();
        PlayerProgressService playerProgress = ResolvePlayerProgressService();

        bool isHost = InstanceFinder.NetworkManager != null &&
                      InstanceFinder.NetworkManager.IsServerStarted;

        ShipUpgradeEvaluation evaluation = CampaignShipUpgradeUtility.Evaluate(
            campaignCatalog,
            campaign,
            playerProgress,
            inventory);

        if (currentShipLevelLabel != null)
            currentShipLevelLabel.text = $"Ship Level: {evaluation.CurrentShipLevel}";

        if (nextShipLevelLabel != null)
        {
            nextShipLevelLabel.text = evaluation.HasNextLevel
                ? $"Next Ship Level: {evaluation.TargetShipLevel}"
                : "Next Ship Level: MAX";
        }

        if (requirementsLabel != null)
            requirementsLabel.text = BuildRequirementsText(evaluation);

        string status = !isHost
            ? "Only host can upgrade the ship."
            : (string.IsNullOrWhiteSpace(evaluation.FailureReason)
                ? "Ready for upgrade."
                : evaluation.FailureReason);

        if (statusLabel != null)
            statusLabel.text = status;

        if (upgradeButtonLabel != null)
        {
            upgradeButtonLabel.text = evaluation.HasNextLevel
                ? $"Upgrade to {evaluation.TargetShipLevel}"
                : "Max Level";
        }

        if (upgradeButton != null)
            upgradeButton.interactable = isHost && evaluation.CanUpgrade;
    }

    private void OnUpgradeClicked()
    {
        if (InstanceFinder.NetworkManager == null || !InstanceFinder.NetworkManager.IsServerStarted)
        {
            Refresh();
            return;
        }

        if (CampaignShipUpgradeUtility.TryApplyUpgrade(
                campaignCatalog,
                CampaignProgressService.EnsureExists(),
                ResolvePlayerProgressService(),
                inventory,
                out ShipUpgradeEvaluation evaluation))
        {
            Refresh();

            if (statusLabel != null)
                statusLabel.text = $"Ship upgraded to level {evaluation.TargetShipLevel}.";

            return;
        }

        Refresh();
    }

    private static string BuildRequirementsText(ShipUpgradeEvaluation evaluation)
    {
        if (evaluation == null)
            return "No upgrade data.";

        if (!evaluation.HasNextLevel || evaluation.TargetConfig == null)
            return "Ship is already at max level.";

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.AppendLine($"Class Level: {evaluation.CurrentClassLevel}/{evaluation.RequiredClassLevel}");

        if (evaluation.TargetConfig.requiredCompletedPlanetIds != null &&
            evaluation.TargetConfig.requiredCompletedPlanetIds.Count > 0)
        {
            builder.AppendLine("Planet Missions:");

            for (int i = 0; i < evaluation.TargetConfig.requiredCompletedPlanetIds.Count; i++)
            {
                string planetId = evaluation.TargetConfig.requiredCompletedPlanetIds[i];
                bool completed = !evaluation.MissingCompletedPlanetIds.Contains(planetId);
                builder.AppendLine($"- {(completed ? "[OK]" : "[ ]")} {planetId}");
            }
        }

        if (evaluation.Costs.Count > 0)
        {
            builder.AppendLine("Materials:");

            for (int i = 0; i < evaluation.Costs.Count; i++)
            {
                ShipUpgradeCostState cost = evaluation.Costs[i];
                builder.AppendLine(
                    $"- {(cost.IsSatisfied ? "[OK]" : "[ ]")} {cost.DisplayName}: {cost.OwnedAmount}/{cost.RequiredAmount}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static PlayerProgressService ResolvePlayerProgressService()
    {
        if (PlayerProgressService.Instance != null)
            return PlayerProgressService.Instance;

        PlayerProgressService existing = UnityEngine.Object.FindFirstObjectByType<PlayerProgressService>(FindObjectsInactive.Include);
        if (existing != null)
            return existing;

        GameObject go = new GameObject(nameof(PlayerProgressService));
        return go.AddComponent<PlayerProgressService>();
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        inventory = null;
    }
}
