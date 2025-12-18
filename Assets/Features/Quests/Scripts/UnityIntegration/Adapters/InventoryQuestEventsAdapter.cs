using UnityEngine;
using Features.Quests.Domain;
using Features.Items.Domain;
using Features.Inventory;

namespace Features.Quests.UnityIntegration.Adapters
{
    /// <summary>
    /// Адаптер: Inventory → Quest events.
    /// Player-scoped, без singleton'ов.
    /// </summary>
    public sealed class InventoryQuestEventsAdapter : MonoBehaviour
    {
        [SerializeField] private QuestManagerMB questManager;

        private IInventoryContext inventory;

        // ======================================================
        // INIT
        // ======================================================

        public void Init(IInventoryContext inventory)
        {
            // Защита от повторной инициализации
            if (this.inventory != null)
                Unsubscribe();

            this.inventory = inventory;

            if (this.inventory != null)
                Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        // ======================================================
        // SUBSCRIBE
        // ======================================================

        private void Subscribe()
        {
            inventory.Service.OnItemAdded += HandleItemAdded;
        }

        private void Unsubscribe()
        {
            if (inventory != null)
                inventory.Service.OnItemAdded -= HandleItemAdded;
        }

        // ======================================================
        // HANDLER
        // ======================================================

        private void HandleItemAdded(ItemInstance inst)
        {
            if (inst == null || inst.itemDefinition == null)
                return;

            questManager?.Service.HandleEvent(
                new ItemAddedEvent(
                    inst.itemDefinition.id,
                    inst.quantity
                )
            );
        }
    }
}
