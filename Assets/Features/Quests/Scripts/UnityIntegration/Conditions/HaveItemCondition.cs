using Features.Quests.Domain;
using Features.Inventory.UnityIntegration;
using UnityEngine;

public sealed class HaveItemCondition : IQuestCondition
{
    private readonly string itemId;
    private readonly int required;

    public HaveItemCondition(string itemId, int required)
    {
        this.itemId = itemId;
        this.required = required;
    }

    public string GetDescription()
    {
        return $"Have {required} {itemId}";
    }

    public void OnStart(QuestRuntime quest)
    {
        UpdateProgress(quest);
    }

    public void OnEvent(QuestRuntime quest, IQuestEvent e)
    {
        // реагируем на любые изменения инвентаря
        if (e is ItemAddedEvent || e is ItemRemovedEvent)
        {
            UpdateProgress(quest);
        }
    }

    private void UpdateProgress(QuestRuntime quest)
    {
        var player = quest.Context as UnityEngine.GameObject;
        if (player == null)
            return;

        var inv = player.GetComponentInChildren<Features.Inventory.UnityIntegration.InventoryManager>();
        if (inv == null || !inv.IsReady)
            return;

        var def = ItemRegistrySO.Instance?.Get(itemId);
        if (def == null)
            return;

        int count = inv.GetItemCount(def);

        quest.SetProgress(this, count);
    }

    public bool IsCompleted(QuestRuntime quest)
    {
        return quest.GetProgress(this) >= required;
    }
}
