using UnityEngine;
using Features.Quests.Domain;

namespace Features.Quests.UnityIntegration.Adapters
{
    public sealed class InventoryQuestEventsAdapter : MonoBehaviour
    {
        [SerializeField] private QuestManagerMB questManager;

        private void OnEnable()
        {
            InventoryManager.OnItemAdded += HandleItemAdded;
        }

        private void OnDisable()
        {
            InventoryManager.OnItemAdded -= HandleItemAdded;
        }

        private void HandleItemAdded(Item item, int amount)
        {
            if (item == null) return;

            // Переводим в доменный itemId
            string itemId = item.id.ToString(); 
            questManager?.Service.HandleEvent(new ItemAddedEvent(itemId, amount));
        }
    }
}
