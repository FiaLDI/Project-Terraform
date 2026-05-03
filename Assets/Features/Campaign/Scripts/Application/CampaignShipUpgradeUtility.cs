using System.Collections.Generic;
using System.Linq;
using Features.Inventory.UnityIntegration;
using Features.Items.Data;
using UnityEngine;

public sealed class ShipUpgradeCostState
{
    public string ItemId;
    public string DisplayName;
    public int RequiredAmount;
    public int OwnedAmount;
    public bool IsSatisfied;
}

public sealed class ShipUpgradeEvaluation
{
    public int CurrentShipLevel;
    public int TargetShipLevel;
    public int CurrentClassLevel;
    public int RequiredClassLevel;
    public bool HasNextLevel;
    public bool CanUpgrade;
    public string FailureReason;
    public ShipLevelConfig TargetConfig;
    public readonly List<string> MissingCompletedPlanetIds = new();
    public readonly List<ShipUpgradeCostState> Costs = new();
}

public static class CampaignShipUpgradeUtility
{
    public static ShipUpgradeEvaluation Evaluate(
        CampaignCatalogSO catalog,
        CampaignProgressService campaign,
        PlayerProgressService playerProgress,
        InventoryManager inventory)
    {
        var evaluation = new ShipUpgradeEvaluation
        {
            CurrentShipLevel = campaign != null ? campaign.ShipLevel : 1
        };

        if (catalog == null)
        {
            evaluation.FailureReason = "Campaign Catalog is not assigned.";
            return evaluation;
        }

        if (campaign == null || campaign.ActiveExpedition == null)
        {
            evaluation.FailureReason = "Active expedition is not selected.";
            return evaluation;
        }

        evaluation.TargetConfig = CampaignCatalogUtility.GetNextShipLevelConfig(
            catalog,
            evaluation.CurrentShipLevel);

        if (evaluation.TargetConfig == null)
        {
            evaluation.FailureReason = "Ship is already at max level.";
            return evaluation;
        }

        evaluation.HasNextLevel = true;
        evaluation.TargetShipLevel = Mathf.Max(
            evaluation.CurrentShipLevel + 1,
            evaluation.TargetConfig.shipLevel);

        PlayerCharacterState activeCharacter = playerProgress != null
            ? playerProgress.GetActiveCharacter()
            : null;

        evaluation.RequiredClassLevel = Mathf.Max(1, evaluation.TargetConfig.requiredClassLevel);
        evaluation.CurrentClassLevel = activeCharacter != null ? activeCharacter.level : 0;

        if (activeCharacter == null)
            evaluation.FailureReason = "Active character is not selected.";
        else if (evaluation.CurrentClassLevel < evaluation.RequiredClassLevel)
            evaluation.FailureReason = $"Class Level {evaluation.RequiredClassLevel} required.";

        if (evaluation.TargetConfig.requiredCompletedPlanetIds != null)
        {
            foreach (string planetId in evaluation.TargetConfig.requiredCompletedPlanetIds
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .Distinct())
            {
                PlanetProgressData progress = campaign.GetOrCreatePlanetProgress(planetId);
                bool completed = progress != null && progress.isPlanetMissionCompleted;

                if (!completed)
                    evaluation.MissingCompletedPlanetIds.Add(planetId);
            }
        }

        if (evaluation.MissingCompletedPlanetIds.Count > 0 && string.IsNullOrWhiteSpace(evaluation.FailureReason))
            evaluation.FailureReason = "Required planet missions are not completed yet.";

        if (evaluation.TargetConfig.upgradeCosts != null)
        {
            foreach (ShipUpgradeCostConfig cost in evaluation.TargetConfig.upgradeCosts)
            {
                if (cost == null || string.IsNullOrWhiteSpace(cost.itemId) || cost.amount <= 0)
                    continue;

                Item item = ItemRegistrySO.Instance?.Get(cost.itemId);
                int owned = item != null && inventory != null
                    ? inventory.GetItemCount(item)
                    : 0;

                bool enough = owned >= cost.amount;

                evaluation.Costs.Add(new ShipUpgradeCostState
                {
                    ItemId = cost.itemId,
                    DisplayName = item != null && !string.IsNullOrWhiteSpace(item.itemName)
                        ? item.itemName
                        : cost.itemId,
                    RequiredAmount = cost.amount,
                    OwnedAmount = owned,
                    IsSatisfied = enough
                });

                if (!enough && string.IsNullOrWhiteSpace(evaluation.FailureReason))
                    evaluation.FailureReason = "Required materials are missing.";
            }
        }

        evaluation.CanUpgrade =
            evaluation.HasNextLevel &&
            string.IsNullOrWhiteSpace(evaluation.FailureReason) &&
            evaluation.MissingCompletedPlanetIds.Count == 0 &&
            evaluation.Costs.All(x => x.IsSatisfied);

        return evaluation;
    }

    public static bool TryApplyUpgrade(
        CampaignCatalogSO catalog,
        CampaignProgressService campaign,
        PlayerProgressService playerProgress,
        InventoryManager inventory,
        out ShipUpgradeEvaluation evaluation)
    {
        evaluation = Evaluate(catalog, campaign, playerProgress, inventory);
        if (!evaluation.CanUpgrade || inventory == null || campaign == null)
            return false;

        var definitions = new List<(Item item, int amount)>();

        foreach (ShipUpgradeCostState cost in evaluation.Costs)
        {
            Item item = ItemRegistrySO.Instance?.Get(cost.ItemId);
            if (item == null)
            {
                evaluation.FailureReason = $"Item '{cost.ItemId}' not found in Item Registry.";
                return false;
            }

            definitions.Add((item, cost.RequiredAmount));
        }

        for (int i = 0; i < definitions.Count; i++)
        {
            (Item item, int amount) = definitions[i];
            if (!inventory.RemoveItem(item, amount))
            {
                evaluation.FailureReason = $"Failed to consume {item.itemName}.";
                return false;
            }
        }

        campaign.SetShipLevel(evaluation.TargetShipLevel);
        return true;
    }
}
