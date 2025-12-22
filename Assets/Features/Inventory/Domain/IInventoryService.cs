using Features.Items.Data;
using Features.Items.Domain;

namespace Features.Inventory.Application
{
    public interface IInventoryService
    {
        bool AddItem(ItemInstance item);
        bool TryRemove(Item itemDefinition, int amount);
        int GetItemCount(Item itemDefinition);
        ItemInstance GetFirst(Item itemDefinition);
        bool HasIngredients(RecipeIngredient[] ingredients);
        bool ConsumeIngredients(RecipeIngredient[] ingredients);
    }
}
